using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/WindVisualizer")]
    public class WindVisualizer : VisualizerModule
    {
        public GameObject DustPrefab;
        private float _lastSpeed; // What the wind speed was last frame
        private Transform _parentTransform; // For keeping the particles organized in the editor
        private HashSet<GameObject> _particles; // For keeping the particles organized in the code
        private float _spawnCooldown; // Number of spawns to do in the next frame
        public float AudioStrength = 0.1f; // How quickly wind responds to audio level, 0 to 1
        public float CountPerSec = 10; // Approx. speed of particle spawn
        public float KillX = -10; // The size of the particle field, also influences spawn location.

        public float Size = 0.2f; // Size of particles
        public float Speed = 100; // Speed of the wind

        public override string Name => "Wind";
        public override bool Scale => false;

        // Use this for initialization
        public override void Spawn(Transform transform)
        {
            _parentTransform = transform;
            _particles = new HashSet<GameObject>();

            //Spawn some particles to begin with
            for (int i = 0; i < CountPerSec * 8; i++)
                MaybeSpawnParticle(Random.Range(-Mathf.Abs(KillX), Mathf.Abs(KillX)));
        }

        // Called every frame
        public override void UpdateVisuals()
        {
            // Set up speeds
            float sum = Mathf.Clamp01(VisualizerCore.Level()) * 0.2f;
            float audioSpeed = Speed * Time.deltaTime * (sum + 0.01f) * Mathf.Sign(KillX);
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
                Vector3 position = particle.transform.position;
                float thisParticleXSpeedModifier = Mathf.Sin((position.y + _spawnCooldown) * 3);
                float thisParticleXSpeed = thisSpeed * (0.9f + 0.1f * thisParticleXSpeedModifier);
                float thisParticleYSpeed = Random.Range(-0.05f, 0.05f) * thisParticleXSpeed;
                position += new Vector3(thisParticleXSpeed, thisParticleYSpeed);
                particle.transform.position = position;

                // Apply scale
                //particle.transform.localScale = new Vector3(Size, Size, 1);

                // Kill if too far
                if (Mathf.Abs(particle.transform.position.x) > Mathf.Abs(KillX))
                {
                    _particles.Remove(particle);
                    Destroy(particle);
                }
            }
        }

        // Has a 50% chance of spawning a particle at (x, random, -2)
        private void MaybeSpawnParticle(float x)
        {
            if (Random.value < 0.5f) return;

            GameObject g = Object.Instantiate(DustPrefab, _parentTransform, true);
            _particles.Add(g);

            // Set particle size and location
            g.transform.position = new Vector3(x, Random.Range(-3.5f, 5f), -2);
            g.transform.localScale = new Vector3(Size, Size, 1);

            // Give particle a random rotation
            Rotator r = g.AddComponent<Rotator>();
            r.RotateSpeed = Random.Range(-150f, 150f);
        }
    }
}