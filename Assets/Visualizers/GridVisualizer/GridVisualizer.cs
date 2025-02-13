﻿using UnityEngine;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/GridVisualizer")]
    public class GridVisualizer : VisualizerModule
    {
        public GameObject CirclePrefab;
        private GradientAlphaKey[] alphaKey;
        public Vector2Int amountOfVisuals = new Vector2Int(12, 8); // How many circles to use.
        private GradientColorKey[] colorKey;
        private SpriteRenderer[] colors;

        private Gradient gradient;

        // Which percentage of samples to keep (most of the latter portion barely move
        // so i only use a portion for the visualization).
        public float keepPercentage = 0.1f;

        // Clamp the bars from stretching way too far so they look
        // somewhat comparative.
        public float maxVisualScale = 1;

        // Min size of circles
        public float minSize = 0.1f;
        private float[] scales; // Store the scales of the bars.

        // Multplier by which to scale all the bars
        public float sizeModifier = 175;

        // How quickly the bars descend from a spike
        public float smoothSpeed = 10;

        private Transform[] visuals; // Store the transforms of the bars.

        public override string Name => "Grid";
        public override bool Scale => false;

        // Use this for initialization
        public override void Spawn(Transform transform)
        {
            visuals = new Transform[amountOfVisuals.x * amountOfVisuals.y];
            scales = new float[amountOfVisuals.x * amountOfVisuals.y];
            colors = new SpriteRenderer[amountOfVisuals.x * amountOfVisuals.y];

            Camera cam = Camera.main;
            float xSpace = 1f / amountOfVisuals.x;
            float ySpace = 1f / amountOfVisuals.y;
            int sum = 0;
            for (int i = 0; i < amountOfVisuals.x; i++)
                for (int j = 0; j < amountOfVisuals.y; j++)
                {
                    Vector3 spawnPos =
                        cam.ViewportToWorldPoint(new Vector3(xSpace / 2f + xSpace * i, ySpace / 2f + ySpace * j));
    
                    GameObject g = Object.Instantiate(CirclePrefab, new Vector3(spawnPos.x, spawnPos.y, 1), Quaternion.identity);
                    g.transform.SetParent(transform, false);
                    visuals[sum] = g.transform;
                    colors[sum] = g.GetComponent<SpriteRenderer>();
                    sum++;
                }

            InitializeGradient();
        }

        // Set scale of bars based on the values from analysis.
        public override void UpdateVisuals()
        {
            int spectrumIndex = 0;
            int averageSize =
                (int) (VisualizerCore.SpectrumSize * keepPercentage / (amountOfVisuals.x * amountOfVisuals.y));

            for (int visualIndex = 0; visualIndex < visuals.Length; visualIndex++)
            {
                float sum = 0;
                for (int j = 0; j < averageSize; j++)
                {
                    sum += VisualizerCore.Spectrum(spectrumIndex);
                    spectrumIndex++;
                }

                float scale = sum / averageSize * sizeModifier;
                scales[visualIndex] -= smoothSpeed * Time.deltaTime;
                scales[visualIndex] = Mathf.Clamp(scale, minSize, maxVisualScale);
                colors[visualIndex].material.color = gradient.Evaluate(scale * 1.0f / maxVisualScale);
                visuals[visualIndex].localScale = Vector3.one * scales[visualIndex];
            }
        }

        private void InitializeGradient()
        {
            gradient = new Gradient();

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            colorKey = new GradientColorKey[6];
            colorKey[0].color = Color.white;
            colorKey[0].time = 0.0f;
            colorKey[1].color = Color.yellow;
            colorKey[1].time = 0.2f;
            colorKey[2].color = Color.blue;
            colorKey[2].time = 0.4f;
            colorKey[3].color = Color.magenta;
            colorKey[3].time = 0.6f;
            colorKey[4].color = Color.red;
            colorKey[4].time = 0.8f;
            colorKey[5].color = Color.black;
            colorKey[5].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 0.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);
        }
    }
}