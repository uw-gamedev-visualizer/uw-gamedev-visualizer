using UnityEngine;

namespace Visualizer.Visualizers
{
    public class FlareVisualizer : IVisualizerModule
    {
        private float[] _scales; // Store the brightnesses of the flares

        private GameObject[] _visuals; // Store the flare objects

        public float CircleRadius = 7; // Used for scaling

        public float MaxVisualScale = 0.25f; // Clamp the brightnesses
        public int NumberOfFlares = 64; // How many flares to use
        public float SizeModifier = 2.5f; // Multplier by which to scale all the flares
        public float SmoothSpeed = 0.25f; // How quickly the flares descend from a spike

        public string Name => "Flare";

        public bool Scale => true;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            GameObject lensFlarePrefab = Resources.Load("Prefabs/LensFlareObject") as GameObject;

            _visuals = new GameObject[NumberOfFlares];
            _scales = new float[NumberOfFlares];

            // Spawn flare objects
            for (int i = 0; i < NumberOfFlares; i++)
            {
                float rads = Mathf.PI * 2 * i / NumberOfFlares; // For position
                float degs = 360.0f * i / NumberOfFlares; // For rotation

                float x = Mathf.Cos(rads) * CircleRadius;
                float y = Mathf.Sin(rads) * CircleRadius;

                GameObject g = Object.Instantiate(lensFlarePrefab, new Vector3(x, y, -1), Quaternion.Euler(0, 0, degs));
                g.transform.SetParent(transform, false);
                _visuals[i] = g;
            }
        }

        // Set brightnesses of flares based on the values from analysis.
        public void UpdateVisuals()
        {
            // Build a logarithmic spectrum
            float[] scaledSpectrum = new float[NumberOfFlares];
            float b = Mathf.Pow(VisualizerCore.SampleSize, 1f / NumberOfFlares);
            float bPow = 1;
            for (int i = 0; i < NumberOfFlares; i++)
            {
                float prevBPow = bPow;
                bPow *= b;
                for (int j = (int) prevBPow; j < bPow - 1; j++)
                    scaledSpectrum[i] += VisualizerCore.Spectrum(j);
            }

            // Set brightnesses of flares
            for (int i = 0; i < NumberOfFlares; i++)
            {
                // Smooth falling and sharp rising.
                float thisY = scaledSpectrum[i] * SizeModifier;
                _scales[i] -= SmoothSpeed * Time.deltaTime;
                _scales[i] = Mathf.Clamp(_scales[i], thisY, MaxVisualScale);

                _visuals[i].GetComponent<LensFlare>().brightness = _scales[i];
            }
        }
    }
}