using Lasp;
using UnityEngine;
using UnityEngine.VFX;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/SpookyVisualizer")]
    public class SpookyVisualizer : VisualizerModule
    {
        private float _lastPower;
        private VisualEffect _visualEffect;

        public float Strength = 0.01f;
        
        public override string Name => "Spooky";
        public override bool Scale => false;

        public override void Spawn(Transform transform) {
            GameObject visual = Object.Instantiate(Resources.Load("VFXGRAPHS/HalloweenVisualizer", typeof(GameObject)) as GameObject,
                Vector3.zero, Quaternion.identity, transform);
            _visualEffect = visual.GetComponent<VisualEffect>();
        }

        public override void UpdateVisuals() {
            float[] low = VisualizerCore.Samples(FilterType.LowPass);
            float[] by = VisualizerCore.Samples(FilterType.Bypass);

            float sum = 0;
            for (int i = 0; i < low.Length; i++)
                sum += (Mathf.Abs(by[i]) + 0.1f * Mathf.Abs(low[i])) /
                       Mathf.Log(i + 2); // when i < 2, this is divide by zero
            float scaledSum = sum * Strength;
            float framePower =
                Mathf.Max(scaledSum, Mathf.Lerp(scaledSum, _lastPower, Time.deltaTime)); // Let the bass kick
            _lastPower = framePower;
            _visualEffect.SetFloat("LerpSpeed", framePower);
        }
    }
}