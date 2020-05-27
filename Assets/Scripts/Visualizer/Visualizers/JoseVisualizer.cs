using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visualizers
{
    public class JoseVisualizer : IVisualizerModule
    {
        public const float BoneLengthSquared = 9;
        public const float LegWidth = 0.25f;

        private GameObject _josePrefab;
        private Material _black;

        private Transform _body;
        private Vector3 _target;
        
        private Transform _targetLeft;
        private Transform _targetRight;
        private Transform _kneeLeft;
        private Transform _kneeRight;
        private Transform _footLeft;
        private Transform _footRight;

        private LineRenderer _ktLeft;
        private LineRenderer _ktRight;
        private LineRenderer _fkLeft;
        private LineRenderer _fkRight;

        private float _lastStrongest;  // Positive/negative do matter!

        public string Name => "Jose";

        public bool Scale => false;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            _josePrefab = Resources.Load("Prefabs/JosePrefab") as GameObject;

            _black = new Material(Shader.Find("Unlit/Color")) {color = Color.black};

            GameObject joseObject = Object.Instantiate(_josePrefab, new Vector3(0, 0, -1), Quaternion.Euler(0, 0, 0));
            joseObject.transform.parent = transform;

            _body = joseObject.transform.Find("Body");
            _target = _body.transform.position;

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
    
        // Set target based on the values from analysis.
        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples)
        {
            float strength = Mathf.Sqrt(spectrum.Take(16).Sum()) * 10f;

            if (strength * 1.5f < Mathf.Abs(_lastStrongest))
            {
                _lastStrongest = -Mathf.Sign(_lastStrongest) * strength;
            }
            else if (strength > Mathf.Abs(_lastStrongest))
            {
                _lastStrongest = Mathf.Sign(_lastStrongest) * strength;
            }
            
            float bodyX = strength * Mathf.Sign(_lastStrongest);
            float bodyY = 2 - bodyX * bodyX * 0.2f;
            _target = new Vector3(bodyX, bodyY, 0);
            _body.position = Vector3.Lerp(_body.position, _target, 0.1f);
            
            SetKneePositions();
            UpdateLinePositions();
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
            Vector3 kneeLeft = halfLeft + directionLeft * Mathf.Sqrt(Mathf.Max(0, BoneLengthSquared - deltaLeft.sqrMagnitude / 4));
            Vector3 kneeRight = halfRight + directionRight * Mathf.Sqrt(Mathf.Max(0, BoneLengthSquared - deltaRight.sqrMagnitude / 4));

            // Set positions
            _kneeLeft.position = kneeLeft;
            _kneeRight.position = kneeRight;
        }

        private void UpdateLinePositions()
        {
            _ktLeft.SetPositions(new[]{_kneeLeft.position, _targetLeft.position});
            _ktRight.SetPositions(new[]{_kneeRight.position, _targetRight.position});
            _fkLeft.SetPositions(new[]{_footLeft.position, _kneeLeft.position});
            _fkRight.SetPositions(new[]{_footRight.position, _kneeRight.position});
        }
    }
}
