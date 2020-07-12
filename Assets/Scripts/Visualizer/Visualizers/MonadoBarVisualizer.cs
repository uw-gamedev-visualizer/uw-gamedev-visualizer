using System;
using UnityEngine;
using Object = UnityEngine.Object;

// The meat of the project. It grabs the sound data from the audio
// source that is attached. Analyzes the sound, and then uses the value
// to scale bars that are generated in various patterns. (At this time: Circle).
namespace Visualizer.Visualizers
{
    public class MonadoBarVisualizer : IVisualizerModule
    {
        private GameObject _barPrefab;
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

        public string Name => "Monado Bars";

        public bool Scale => true;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            _barPrefab = (GameObject) Resources.Load("Prefabs/Bar");

            _visuals = new Transform[AmountOfVisuals];
            _scales = new float[AmountOfVisuals];
            
            // Spawn bars
            for (int i = 0; i < AmountOfVisuals; i++)
            {
                float rads = Mathf.PI * 2 * i / AmountOfVisuals; // For position
                float degs = 360.0f * i / AmountOfVisuals; // For rotation

                float x = Mathf.Cos(rads);
                float y = Mathf.Sin(rads);

                GameObject g = Object.Instantiate(_barPrefab, new Vector3(x, y, 0), Quaternion.Euler(new Vector3(0, 0, degs)));
                g.transform.SetParent(transform, false);
                _visuals[i] = g.transform;
            }
        }

        // Set scale of bars based on the values from analysis.
        public void UpdateVisuals()
        {
            int spectrumIndex = 0;
            int averageSize = (int) (VisualizerCore.SampleSize * KeepPercentage / AmountOfVisuals);

            float[] spectrum = VisualizerCore.Spectrum();

            for (int visualIndex = 0; visualIndex < AmountOfVisuals; visualIndex++)
            {
                float sum = 0;
                for (int j = 0; j < averageSize; j++)
                {
                    sum += spectrum[spectrumIndex];
                    spectrumIndex++;
                }

                float audioScale = sum / averageSize * SizeModifier;
                float fallScale = Mathf.Max(_scales[visualIndex] - SmoothSpeed * Time.deltaTime, 0);
                float finalScale = Mathf.Clamp(audioScale, fallScale, MaxVisualScale);

                _scales[visualIndex] = finalScale;

                _visuals[visualIndex].localScale = new Vector3(1 + finalScale, 1, 1);
            }
        }
    }
}