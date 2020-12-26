using UnityEngine;

// The meat of the project. It grabs the sound data from the audio
// source that is attached. Analyzes the sound, and then uses the value
// to scale bars that are generated in various patterns. (At this time: Circle).
namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/MonadoBarVisualizer")]
    public class MonadoBarVisualizer : VisualizerModule
    {
        public GameObject BarPrefab;
        private const int AmountOfVisuals = 64; // How many bars to use.

        // Which percentage of samples to use (most of the higher freqs barely move).
        private const float KeepPercentage = 0.5f;

        // Clamp the bars from stretching way too far
        private const float MaxVisualScale = 15;
        private float[] _scales; // Store the scales (y) of the bars.

        // Multiplier by which to scale all the bars
        private const float SizeModifier = 25;

        // How quickly the bars descend from a spike
        private const float SmoothSpeed = 10;

        private Transform[] _visuals; // Store the transforms of the bars.

        public override string Name => "Monado Bars";
        public override bool Scale => true;

        // Use this for initialization
        public override void Spawn(Transform transform)
        {
            _visuals = new Transform[AmountOfVisuals];
            _scales = new float[AmountOfVisuals];
            
            // Spawn bars
            for (int i = 0; i < AmountOfVisuals; i++)
            {
                float rads = Mathf.PI * 2 * i / AmountOfVisuals; // For position
                float degs = 360.0f * i / AmountOfVisuals; // For rotation

                float x = Mathf.Cos(rads);
                float y = Mathf.Sin(rads);

                GameObject g = Instantiate(BarPrefab, new Vector3(x, y, 0), Quaternion.Euler(new Vector3(0, 0, degs)));
                g.transform.SetParent(transform, false);
                _visuals[i] = g.transform;
            }
        }

        // Set scale of bars based on the values from analysis.
        public override void UpdateVisuals()
        {
            float[] spectrum = VisualizerCore.Spectrum();
            
            int spectrumIndex = 0;
            int averageSize = (int) (spectrum.Length * KeepPercentage / AmountOfVisuals);

            for (int visualIndex = 0; visualIndex < AmountOfVisuals; visualIndex++)
            {
                float sum = 0;
                for (int j = 0; j < averageSize; j++)
                {
                    sum += spectrum[spectrumIndex];
                    spectrumIndex++;
                }

                float audioScale = sum / averageSize * SizeModifier;
                float fallScale = Mathf.Clamp(_scales[visualIndex] - SmoothSpeed * Time.deltaTime, 0, MaxVisualScale);
                float finalScale = Mathf.Clamp(audioScale, fallScale, MaxVisualScale);

                _scales[visualIndex] = finalScale;

                _visuals[visualIndex].localScale = new Vector3(1 + finalScale, 1, 1);
            }
        }
    }
}