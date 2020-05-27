using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Visualizers
{
    public class WindVisualizer : IVisualizerModule
    {

        public float Size = 0.3f;           // Size of particles
        public float Speed = 10;            // Speed of the wind
        public float AudioStrength = 0.1f;  // Impact of the audio on wind, 0 to 1
        public float CountPerSec = 10;      // Approx. speed of particle spawn
        public float KillX = -10;           // The size of the particle field, also influences spawn location.

        private GameObject _dustPrefab;
        private Transform _parentTransform;      // For keeping the particles organized in the editor
        private HashSet<GameObject> _particles;  // For keeping the particles organized in the code
        private float _spawnCooldown;            // Number of spawns to do in the next frame
        private float _lastSpeed;                // What the wind speed was last frame

        public string Name => "Wind";

        public bool Scale => false;

        // Use this for initialization
        public void Spawn(Transform transform)
        {
            _dustPrefab = Resources.Load("Prefabs/MimicDust") as GameObject;
            _parentTransform = transform;
            _particles = new HashSet<GameObject>();
        
            //Spawn some particles to begin with
            for (int i = 0; i < CountPerSec * 8; i++)
                MaybeSpawnParticle(Random.Range(-Mathf.Abs(KillX), Mathf.Abs(KillX)));
        }

        // Called every frame
        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples)
        {
            // Set up speeds
            float sum = spectrum.Sum();
            float audioSpeed = Speed * Time.deltaTime * (sum * sum + 0.1f) * Mathf.Sign(KillX);
            float thisSpeed = Mathf.Lerp(_lastSpeed, audioSpeed, AudioStrength);
            _lastSpeed = thisSpeed;

            // Set up spawns
            _spawnCooldown += CountPerSec * Time.deltaTime * Mathf.Abs(thisSpeed) * 20;
            for (; _spawnCooldown > 1; _spawnCooldown--)
                MaybeSpawnParticle(-KillX);
        
            // Apply speeds
            foreach (GameObject particle in _particles.ToList())
            {
                // Calculate and apply speeds
                float thisParticleXSpeedModifier = Mathf.Sin((particle.transform.position.y + _spawnCooldown) * 3);
                float thisParticleXSpeed = thisSpeed * (0.9f + 0.1f * thisParticleXSpeedModifier);
                float thisParticleYSpeed = Random.Range(-0.05f, 0.05f) * thisParticleXSpeed;
                particle.transform.position += new Vector3(thisParticleXSpeed, thisParticleYSpeed);
            
                // Apply scale
                particle.transform.localScale = Vector3.one * thisSpeed;
            
                // Kill if too far
                if (particle.transform.position.x / KillX > 1)
                {
                    _particles.Remove(particle);
                    Object.Destroy(particle, 1);
                }
            }
        }

        // Has a 50% chance of spawning a particle at (x, random, -2)
        private void MaybeSpawnParticle(float x)
        {
            if (Random.value < 0.5f) return;
        
            GameObject g = Object.Instantiate(_dustPrefab);
        
            // Keep a clean workspace
            _particles.Add(g);
            g.transform.parent = _parentTransform;
        
            // Set particle size and location
            g.transform.position = new Vector3(x, Random.Range(-3.5f, 5f), -2);
            g.transform.localScale = new Vector3(Size, Size);
        
            // Give particle a random rotation
            Rotator r = g.AddComponent<Rotator>();
            r.RotateSpeed = Random.Range(-150f, 150f);
        }
    }
}
