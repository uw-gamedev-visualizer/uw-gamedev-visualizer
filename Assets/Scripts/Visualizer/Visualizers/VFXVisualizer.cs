using Lasp;
using UnityEngine;
using UnityEngine.VFX;

namespace Visualizer.Visualizers
{
    public class VFXVisualizer : IVisualizerModule
    {
        private float _lastPower;
        private VisualEffect _visualEffect;

        public float Strength = 0.01f;
        public string Name => "VFX";

        public bool Scale => false;

        public void Spawn(Transform transform)
        {
            GameObject visual = Object.Instantiate(Resources.Load("VFXGRAPHS/TestVFX", typeof(GameObject)) as GameObject,
                Vector3.zero, Quaternion.identity, transform);
            _visualEffect = visual.GetComponent<VisualEffect>();
        }

        public void UpdateVisuals()
        {
            float[] low = VisualizerCore.Samples(FilterType.LowPass);
            float[] by = VisualizerCore.Samples(FilterType.Bypass);

            float sum = 0;
            for (int i = 0; i < VisualizerCore.SampleSize; i++)
                sum += (Mathf.Abs(by[i]) + 0.1f * Mathf.Abs(low[i])) /
                       Mathf.Log(i + 2); // when i < 2, this is divide by zero
            float scaledSum = sum * Strength;
            float framePower =
                Mathf.Max(scaledSum, Mathf.Lerp(scaledSum, _lastPower, Time.deltaTime)); // Let the bass kick
            _lastPower = framePower;
            //Debug.Log(framePower);
            _visualEffect.SetFloat("VelocityIntensity", framePower);
            //_visualEffect.playRate = (framePower + 3) / 4;
        }
    }
}