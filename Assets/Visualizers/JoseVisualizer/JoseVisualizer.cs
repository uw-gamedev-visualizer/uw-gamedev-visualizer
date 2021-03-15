using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/JoseVisualizer")]
    public class JoseVisualizer : VisualizerModule
    {
        public const float BoneLengthSquared = 9;
        public const float LegWidth = 0.25f;
        private Material _black;

        private Transform _body;
        private LineRenderer _fkLeft;
        private LineRenderer _fkRight;
        private Transform _footLeft;
        private Transform _footRight;

        public GameObject JosePrefab;
        private Transform _kneeLeft;
        private Transform _kneeRight;

        private LineRenderer _ktLeft;
        private LineRenderer _ktRight;

        private Transform _targetLeft;
        private Transform _targetRight;

        private Vector3 _center;
        private Vector3 _activePeak;
        private Vector3 _inactivePeak;
        private float lockoutTime = 0;

        public override string Name => "Jose";
        public override bool Scale => false;

        // Use this for initialization
        public override void Spawn(Transform transform)
        {
            GameObject joseObject = Instantiate(JosePrefab, new Vector3(0, 0, -1), Quaternion.Euler(0, 0, 0));
            joseObject.transform.parent = transform;

            _body = joseObject.transform.Find("Body");
            _center = _body.transform.position + Vector3.up;
            _activePeak = new Vector3(_center.x - 4, _center.y - 2, _center.z);
            _inactivePeak = new Vector3(_center.x + 4, _center.y - 2, _center.z);

            _black = _body.GetComponent<MeshRenderer>().material;

            _targetLeft = _body.transform.Find("TargetLeft");
            _targetRight = _body.transform.Find("TargetRight");
            _kneeLeft = joseObject.transform.Find("KneeLeft");
            _kneeRight = joseObject.transform.Find("KneeRight");
            _footLeft = joseObject.transform.Find("FootLeft");
            _footRight = joseObject.transform.Find("FootRight");

            GameObject lines = new GameObject("Lines");
            lines.transform.parent = transform;
            _ktLeft = LegSetup(lines);
            _ktRight = LegSetup(lines);
            _fkLeft = LegSetup(lines);
            _fkRight = LegSetup(lines);
        }

        public override void UpdateVisuals()
        {
            lockoutTime -= Time.deltaTime;
            if (lockoutTime <= 0 && (VisualizerBeatDetector.SecsToBeat < 0.05f || VisualizerBeatDetector.IsBeat))
            {
                _body.position = _activePeak;
                
                Vector3 temp = _activePeak;
                _activePeak = _inactivePeak;
                _inactivePeak = temp;

                lockoutTime = 0.2f;
            }
            else
            {
                _body.position = Vector3.Lerp(_body.position, _center, Time.deltaTime * 5);
            }
            
            SetKneePositions();
            UpdateLinePositions();
            
            // Debug
            /*VisualizerBeatDetector.BeatDetectorData data = VisualizerBeatDetector.GetData();
            Debug.DrawLine(new Vector3(-3, 5 * data.amps.Max()), new Vector3(3, 5 * data.amps.Max()), Color.green);
            Debug.DrawLine(new Vector3(-3, 5 * data.prevAvg * data.minBeatMultiplier), new Vector3(3, 5 * data.prevAvg * data.minBeatMultiplier), Color.yellow);
            Debug.DrawLine(new Vector3(-3, 5 * data.prevAvg), new Vector3(3, 5 * data.prevAvg), Color.blue);
            Debug.DrawLine(new Vector3(-3, 5 * data.noiseGate), new Vector3(3, 5 * data.noiseGate), Color.red);*/
        }

        private LineRenderer LegSetup(GameObject lines)
        {
            GameObject line = new GameObject("Line");
            line.transform.parent = lines.transform;

            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.material = _black;
            lineRenderer.startWidth = LegWidth;
            lineRenderer.endWidth = LegWidth;
            return lineRenderer;
        }

        // Calculate knee positions and set them
        private void SetKneePositions()
        {
            // Get midpoints
            Vector3 halfLeft = (_targetLeft.position + _footLeft.position) / 2;
            Vector3 halfRight = (_targetRight.position + _footRight.position) / 2;

            // Get vectors from feet to targets
            Vector3 deltaLeft = _targetLeft.position - _footLeft.position;
            Vector3 deltaRight = _targetRight.position - _footRight.position;

            // Get direction of knee from midpoint (perpendicular to deltas)
            Vector3 directionLeft = new Vector3(-deltaLeft.y, deltaLeft.x).normalized;
            Vector3 directionRight = new Vector3(deltaRight.y, -deltaRight.x).normalized;

            // Pythagoras says, distance = sqrt(c^2 - a^2)
            // Midpoint + direction * distance = new position
            Vector3 kneeLeft =
                halfLeft + directionLeft * Mathf.Sqrt(Mathf.Max(0, BoneLengthSquared - deltaLeft.sqrMagnitude / 4));
            Vector3 kneeRight = halfRight + directionRight *
                            Mathf.Sqrt(Mathf.Max(0, BoneLengthSquared - deltaRight.sqrMagnitude / 4));

            // Set positions
            _kneeLeft.position = kneeLeft;
            _kneeRight.position = kneeRight;
        }

        private void UpdateLinePositions()
        {
            _ktLeft.SetPositions(new[] {_kneeLeft.position, _targetLeft.position});
            _ktRight.SetPositions(new[] {_kneeRight.position, _targetRight.position});
            _fkLeft.SetPositions(new[] {_footLeft.position, _kneeLeft.position});
            _fkRight.SetPositions(new[] {_footRight.position, _kneeRight.position});
        }
    }
}