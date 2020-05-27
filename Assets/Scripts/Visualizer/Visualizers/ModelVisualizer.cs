using System.Linq;
using UnityEngine;

namespace Visualizer.Visualizers
{
    public class ModelVisualizer : IVisualizerModule
    {
        private static readonly int AnimationTime = Animator.StringToHash("Time");

        private Animator _anim;
        private float _lastSpeed;
        private float _maxSum;
        private float _totalTime;
        public GameObject ModelPrefab;

        public float Speed = 0.001f;

        public string Name => "3D Model";

        public bool Scale => false;

        public void Spawn(Transform transform)
        {
            ModelPrefab = Resources.Load("Prefabs/Mimic") as GameObject;
            GameObject mimic = Object.Instantiate(ModelPrefab, transform, true);
            _anim = mimic.GetComponent<Animator>();
            //_anim.SetBool("Open", true);
        }

        public void UpdateVisuals()
        {
            // Set up speeds
            float sum = VisualizerCore.Level();
            if (sum > _maxSum)
                _maxSum = sum;

            float audioSpeed = Speed * Time.deltaTime * (sum * sum + 0.01f);
            _totalTime += audioSpeed;
            _totalTime %= 2;

            _anim.SetFloat(AnimationTime, Mathf.PingPong(_totalTime, 1));
            //_anim.SetFloat("Time", 1f - sum / _maxSum);
        }
    }
}