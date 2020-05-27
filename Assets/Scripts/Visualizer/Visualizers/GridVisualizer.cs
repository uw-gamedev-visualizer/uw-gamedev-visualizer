using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visualizers
{
    public class GridVisualizer : IVisualizerModule
    {

        private GameObject _circlePrefab;

        // Which percentage of samples to keep (most of the latter portion barely move
        // so i only use a portion for the visualization).
        public float keepPercentage = 0.1f;
    
        // Clamp the bars from stretching way too far so they look
        // somewhat comparative.
        public float maxVisualScale = 1;

        // Multplier by which to scale all the bars
        public float sizeModifier = 175;

        // How quickly the bars descend from a spike
        public float smoothSpeed = 10;
        
        // Min size of circles
        public float minSize = 0.1f;
    
        private Transform[] visuals; // Store the transforms of the bars.
        private SpriteRenderer[] colors;
        private float[] scales;      // Store the scales of the bars.
        public Vector2Int amountOfVisuals = new Vector2Int(12, 8); // How many circles to use.

        Gradient gradient;
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;

        public string Name => "Grid";

        public bool Scale => false;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            _circlePrefab = (GameObject) Resources.Load("Prefabs/_Circle");
        
            visuals = new Transform[amountOfVisuals.x * amountOfVisuals.y];
            scales = new float[amountOfVisuals.x * amountOfVisuals.y];
            colors = new SpriteRenderer[amountOfVisuals.x * amountOfVisuals.y];

            Camera cam = Camera.main;
            float xSpace = 1f / amountOfVisuals.x;
            float ySpace = 1f / amountOfVisuals.y;
            int sum = 0;
            for (int i = 0; i < amountOfVisuals.x; i++) {
                for (int j = 0; j < amountOfVisuals.y; j++) {
                    Vector3 spawnPos = cam.ViewportToWorldPoint(new Vector3(xSpace / 2f + xSpace*i, ySpace / 2f + ySpace*j));

                    GameObject g = Object.Instantiate(_circlePrefab, new Vector3(spawnPos.x, spawnPos.y, 1), Quaternion.identity);
                    g.transform.SetParent(transform, false);
                    visuals[sum] = g.transform;
                    colors[sum] = g.GetComponent<SpriteRenderer>();
                    sum++;
                }
            }

            InitializeGradient();
        }

        // Set scale of bars based on the values from analysis.
        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples)
        {
            int spectrumIndex = 0;
            int averageSize = (int)(sampleSize * keepPercentage / (amountOfVisuals.x * amountOfVisuals.y));

            for (int visualIndex = 0; visualIndex < visuals.Length; visualIndex++)
            {
                float sum = 0;
                for (int j = 0; j < averageSize; j++)
                {
                    sum += spectrum[spectrumIndex];
                    spectrumIndex++;
                }

                float scale = sum / averageSize * sizeModifier;
                scales[visualIndex] -= smoothSpeed * Time.deltaTime;
                scales[visualIndex] = Mathf.Clamp(scale, minSize, maxVisualScale);
                colors[visualIndex].material.color = gradient.Evaluate(scale * 1.0f / maxVisualScale);
                visuals[visualIndex].localScale = Vector3.one * scales[visualIndex];
            }
        }

        private void InitializeGradient() {
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

