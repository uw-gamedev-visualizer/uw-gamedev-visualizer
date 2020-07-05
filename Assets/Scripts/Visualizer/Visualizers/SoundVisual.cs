using System;
using UnityEngine;
using Object = UnityEngine.Object;

// The meat of the project. It grabs the sound data from the audio
// source that is attached. Analyzes the sound, and then uses the value
// to scale bars that are generated in various patterns. (At this time: Circle).
namespace Visualizer.Visualizers
{
    public class SoundVisual : IVisualizerModule
    {
        private GameObject _barPrefab;
        public int amountOfVisuals = 64; // How many bars to use.

        public float circleRadius = 5;

        // Which percentage of samples to keep (most of the latter portion barely move
        // so i only use a portion for the visualization).
        public float keepPercentage = 0.5f;

        // Clamp the bars from stretching way too far so they look
        // somewhat comparative.
        public float maxVisualScale = 15;
        private float[] scales; // Store the scales (y) of the bars.

        // Multplier by which to scale all the bars
        public float sizeModifier = 25;

        // How quickly the bars descend from a spike
        public float smoothSpeed = 10;

        private Transform[] visuals; // Store the transforms of the bars.

        public string Name => "Bars";

        public bool Scale => true;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            _barPrefab = (GameObject) Resources.Load("Prefabs/Bar");

            visuals = new Transform[amountOfVisuals];
            scales = new float[amountOfVisuals];

            // Spawn bars
            for (int i = 0; i < amountOfVisuals; i++)
            {
                float rads = Mathf.PI * 2 * i / amountOfVisuals; // For position
                float degs = 360.0f * i / amountOfVisuals; // For rotation

                float x = Mathf.Cos(rads) * circleRadius;
                float y = Mathf.Sin(rads) * circleRadius;

                GameObject g = Object.Instantiate(_barPrefab, new Vector3(x, y, 0), Quaternion.Euler(new Vector3(0, 0, degs)));
                g.transform.SetParent(transform, false);
                visuals[i] = g.transform;
            }
        }

        // Set scale of bars based on the values from analysis.
        public void UpdateVisuals()
        {
            int spectrumIndex = 0;
            int averageSize = (int) (VisualizerCore.SampleSize * keepPercentage / amountOfVisuals);

            for (int visualIndex = 0; visualIndex < amountOfVisuals; visualIndex++)
            {
                float sum = 0;
                for (int j = 0; j < averageSize; j++)
                {
                    sum += VisualizerCore.Spectrum(spectrumIndex);
                    spectrumIndex++;
                }

                float audioScale = sum / averageSize * sizeModifier;
                float fallScale = scales[visualIndex] - smoothSpeed * Time.deltaTime;
                float finalScale = Mathf.Clamp(fallScale, audioScale, maxVisualScale);
                scales[visualIndex] = finalScale;

                visuals[visualIndex].localScale = new Vector3(1 + finalScale, 1, 1);
            }
        }
    }
}