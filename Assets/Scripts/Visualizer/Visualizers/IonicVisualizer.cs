using System.Collections.Generic;
using System.Linq;
using Lasp;
using UnityEngine;

namespace Visualizer.Visualizers
{
    [CreateAssetMenu(menuName = "Visualizers/IonicVisualizer")]
    public class IonicVisualizer : VisualizerModule
    {
        private const int NumberOfChunks = 8; // How many rings
        private const int TextureX = 256; // Size of the textures
        private const int TextureY = 1024; // Size of the textures

        private const int
            SampleBarCount = 220; // TextureY / (SampleBarWidth + SampleBarSeparation) is full ring, don't go over

        private const float SampleMultiplier = 0.3f;
        private const int SampleBarDistance = 150; // How far the rings are from the center
        private const int SampleBarWidth = 3;
        private const int SampleBarSeparation = 1;
        private const int SampleBarMinHeight = 2;
        private const int SampleBarMaxHeight = 32;
        private const float JumpPower = 0.1f; // Scaling factor for rings jumping
        private const float FallRate = 0.1f;
        private const float RingMinScale = 4f;
        private const float RingMaxScale = 14f;
        private const float MinRotSpeed = 5; // Scalar
        private const float MaxRotSpeed = 50; // Scalar
        private Texture2D _blank; // Used to erase previous frame's _texture
        
        public Shader Shader;
        public GameObject IonicPrefab;

        private Texture2D _texture; // Represents the bars
        private readonly Vector3 _subPos = new Vector3(1, 0, 0); // Start position of _subParent
        private readonly Vector3 _subRot = new Vector3(20, -30, 0); // Start rotation of _subParent
        private List<GameObject> _disks; // Used for updating

        private GameObject _subParent; // Wrapper to handle rotation and position

        public override string Name => "Ionic";
        public override bool Scale => false;

        // Build the visualiser
        public override void Spawn(Transform transform)
        {
            // Setup
            _blank = new Texture2D(TextureX, TextureY, TextureFormat.RGBA32, false);
            _blank.SetPixels(0, 0, TextureX, TextureY, Enumerable.Repeat(Color.clear, TextureX * TextureY).ToArray());
            _blank.Apply(false);

            _texture = new Texture2D(TextureX, TextureY, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(_blank, _texture);
            
            // Spawn _subParent
            _subParent = new GameObject("SubParent");
            // Set position
            _subParent.transform.position = _subPos;
            // Parent is the usual parent
            _subParent.transform.parent = transform;

            // Spawn disks
            _disks = new List<GameObject>();
            // Make NumberOfChunks of them
            for (int i = 0; i < NumberOfChunks; i++)
            {
                // Spawn the actual GameObject from plane
                GameObject g = Instantiate(IonicPrefab, _subParent.transform, true);
                // Set its size
                g.transform.localScale =
                    Mathf.Lerp(RingMinScale, RingMaxScale, (float) i / NumberOfChunks) * Vector3.one;
                // Rotate around by a random amount
                g.transform.Rotate(Vector3.forward * Random.value * 360);
                // Set material
                g.GetComponent<MeshRenderer>().material.mainTexture = _texture;
                g.GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(Random.value, 1, 1);
                // Register for updates
                _disks.Add(g);
            }

            // Rotate _subParent for that off-center look
            _subParent.transform.localRotation = Quaternion.Euler(_subRot);
        }

        public override void UpdateVisuals()
        {
            // Build a logarithmic spectrum
            float[] scaledSpectrum = new float[NumberOfChunks];
            float b = Mathf.Pow(VisualizerCore.SpectrumSize, 1f / NumberOfChunks);
            float bPow = 1;
            for (int i = 0; i < NumberOfChunks; i++)
            {
                float prevBPow = bPow;
                bPow *= b;
                for (int j = (int) prevBPow; j < bPow - 1 && j < VisualizerCore.SpectrumSize; j++)
                    scaledSpectrum[i] += Mathf.Abs(VisualizerCore.Spectrum(j));
            }

            // Update texture
            Graphics.CopyTexture(_blank, _texture);
            float[] samples = VisualizerCore.Samples(FilterType.Bypass);
            for (int i = 0; i < SampleBarCount && i < samples.Length; i++)
            {
                // Height between 2 and 32
                float heightFrac = Mathf.Clamp01(Mathf.Abs(samples[i])) * SampleMultiplier;
                int height = (int) Mathf.Ceil(Mathf.Lerp(SampleBarMinHeight, SampleBarMaxHeight, heightFrac));
                // Draw the rect on the texture buffer
                _texture.SetPixels(
                    SampleBarDistance - height,
                    (SampleBarWidth + SampleBarSeparation) * i,
                    2 * height,
                    SampleBarWidth,
                    Enumerable.Repeat(Color.white, 2 * SampleBarWidth * height).ToArray()
                );
            }

            // Draw the texture buffer to the texture (no mipmaps)
            _texture.Apply(false);

            // Rotate and jump
            for (int i = 0; i < NumberOfChunks; i++)
            {
                _disks[i].transform.Rotate(Vector3.forward * RotationAmount(i));
                float target = scaledSpectrum[i] * JumpPower;
                float height = Mathf.Clamp(
                    Mathf.Lerp(-_disks[i].transform.localPosition.z, target, FallRate),
                    target,
                    20
                );
                _disks[i].transform.localPosition = Vector3.back * height;
            }

            _subParent.transform.localRotation =
                Quaternion.Euler(_subRot + new Vector3(5 * Mathf.Sin(Time.time), 5 * Mathf.Sin(Time.time * 1.234f), 0));
        }

        private float RotationAmount(int i)
        {
            int mod = i % 2 * 2 - 1; // -1 if even, 1 if odd
            return mod * Mathf.Lerp(MaxRotSpeed, MinRotSpeed, (float) i / NumberOfChunks) * Time.deltaTime;
        }
    }
}