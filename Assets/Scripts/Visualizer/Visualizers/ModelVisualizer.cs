using System.Linq;
using UnityEngine;

namespace Visualizer.Visualizers
{
    public class ModelVisualizer : IVisualizerModule
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

        public string Name => "3D Model";

        public bool Scale => false;

        public void Spawn(Transform transform)
        {
            ModelPrefab = Resources.Load("Prefabs/Mimic") as GameObject;
            GameObject go = Object.Instantiate(ModelPrefab, transform, true);
            Mimic mimic = go.GetComponent<Mimic>();
            _animator = mimic.Animator;
            _light = mimic.InsideLight;
            //_anim.SetBool("Open", true);
        }

        public void UpdateVisuals()
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