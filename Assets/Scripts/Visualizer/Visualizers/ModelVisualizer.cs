using UnityEngine;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/ModelVisualizer")]
    public class ModelVisualizer : VisualizerModule
    {
        private static readonly int AnimationTime = Animator.StringToHash("Time");

        public GameObject ModelPrefab;
        
        private Animator _animator;
        private Light _light;
        private float _lastSpeed;
        private float _maxSum;
        private float _totalTime;

        public float Speed = 20;
        public float Brightness = 5;

        public override string Name => "3D Model";
        public override bool Scale => false;

        public override void Spawn(Transform transform)
        {
            GameObject go = Object.Instantiate(ModelPrefab, transform, true);
            Mimic mimic = go.GetComponent<Mimic>();
            _animator = mimic.Animator;
            _light = mimic.InsideLight;
            //_anim.SetBool("Open", true);
        }

        public override void UpdateVisuals()
        {
            // Set up speeds
            float sum = VisualizerCore.Level();
            if (sum > _maxSum)
                _maxSum = sum;

            float audioSpeed = Speed * Time.deltaTime * (sum + 0.01f);
            _totalTime += audioSpeed;
            _totalTime %= 2;

            _animator.SetFloat(AnimationTime, Mathf.PingPong(_totalTime, 1));
            //_anim.SetFloat("Time", 1f - sum / _maxSum);

            _light.intensity = Mathf.Max(sum * Brightness, _light.intensity * 0.95f);
        }
    }
}