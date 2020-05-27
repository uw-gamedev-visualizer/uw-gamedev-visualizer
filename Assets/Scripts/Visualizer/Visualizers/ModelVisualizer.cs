using System.Linq;
using UnityEngine;

namespace Visualizers
{
    public class ModelVisualizer : IVisualizerModule
    {
        public GameObject _modelPrefab;

        public float speed = 3;
        private float _lastSpeed;

        private Animator _anim;
        private float _totalTime;
        private float _maxSum;

        public string Name => "3D Model";

        public bool Scale => false;

        public void Spawn(Transform transform)
        {
            _modelPrefab = Resources.Load("Prefabs/Mimic") as GameObject;
            GameObject mimic = Object.Instantiate(_modelPrefab);
            mimic.transform.SetParent(transform, true);
            _anim = mimic.GetComponent<Animator>();
            //_anim.SetBool("Open", true);
        }

        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples)
        {
            // Set up speeds
            float sum = spectrum.Sum();
            if (sum > _maxSum)
                _maxSum = sum;
        
            float audioSpeed = speed * Time.deltaTime * (sum * sum + 0.01f);
            _totalTime += audioSpeed;
            _totalTime %= 2;

            _anim.SetFloat("Time", Mathf.PingPong(_totalTime, 1));
            //_anim.SetFloat("Time", 1f - sum / _maxSum);
        }
    }
}
