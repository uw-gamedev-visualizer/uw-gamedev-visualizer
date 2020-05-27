using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace Visualizer.Visualizers
{
    public class FireVisualizer : IVisualizerModule
    {
        private const int FLAMES = 128;
        private const int BITS = 7;
        
        private GameObject _fireVfx;
        private VisualEffect[] _flames;
        private float[] _powers;
        
        public string Name => "Fire";
        public bool Scale => false;
        
        public void Spawn(Transform transform)
        {
            _fireVfx = Resources.Load("Prefabs/Fire") as GameObject;
            
            _flames = new VisualEffect[FLAMES];
            _powers = new float[FLAMES];

            for (uint i = 0; i < FLAMES; i++)
            {
                //float posX = GetX(i+1, BITS);
                float posX = NormalRandom();
                GameObject go = Object.Instantiate(_fireVfx, transform.position + new Vector3(5 * posX - 2, -2, 0), Quaternion.identity);
                go.transform.parent = transform;
                _flames[i] = go.GetComponent<VisualEffect>();
            }
        }

        public void UpdateVisuals()
        {
            float sumSpectrum = VisualizerCore.Level();
            
            // Build a logarithmic spectrum
            float[] scaledSpectrum = new float[FLAMES];
            float b = Mathf.Pow(VisualizerCore.SampleSize, 1f / FLAMES);
            float bPow = 1;
            for (int i = 0; i < FLAMES; i++)
            {
                float prevBPow = bPow;
                bPow *= b;
                for (int j = (int) prevBPow; j < bPow - 1; j++)
                    scaledSpectrum[i] += VisualizerCore.Spectrum(j);
            }
            
            // Update
            for (int i = 0; i < FLAMES; i++)
            {
                float nextPower = 0.2f + scaledSpectrum[i] * 6 + sumSpectrum * 0.5f;
                _powers[i] = Mathf.Max(_powers[i] * (1 - 10 * Time.deltaTime), nextPower);
                _flames[i].SetFloat("Power", _powers[i]);
            }
        }

        private static float GetX(uint index, uint bits)
        {
            float output = 0;
            int denominator = 2;

            for (int i = 0; i < bits; i++)
            {
                output += (index % 2) / (float) denominator;
                index /= 2;
                denominator *= 2;
            }

            return output;
        }
        
        private static float NormalRandom()
        {
            float u1 = Random.value;
            float u2 = Random.value;
            float normal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            return 0.5f + normal * 0.2f;
        }
    }
}
