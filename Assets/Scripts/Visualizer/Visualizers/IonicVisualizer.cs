using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Visualizers
{
    public class IonicVisualizer : IVisualizerModule
    {
        private const int NumberOfChunks = 8;                       // How many rings
        private const int TextureX = 256;                           // Size of the textures
        private const int TextureY = 1024;                          // Size of the textures
        private const int SampleBarCount = 220;                     // TextureY / (SampleBarWidth + SampleBarSeparation) is full ring, don't go over
        private const int SampleBarDistance = 150;                  // How far the rings are from the center
        private const int SampleBarWidth = 3;
        private const int SampleBarSeparation = 1;
        private const int SampleBarMinHeight = 2;
        private const int SampleBarMaxHeight = 32;
        private const int JumpPower = 5;                            // Scaling factor for rings jumping
        private readonly Vector3 SubPos = new Vector3(2, 0, 0);     // Start position of _subParent
        private readonly Vector3 SubRot = new Vector3(20, -30, 0);  // Start rotation of _subParent
        private const float RingMinScale = 0.4f;
        private const float RingMaxScale = 1.4f;
        private const float MinRotSpeed = 5;                        // Scalar
        private const float MaxRotSpeed = 50;                       // Scalar

        private readonly Texture2D _texture;  // Represents the bars
        private readonly Texture2D _blank;    // Used to erase previous frame's _texture
        private readonly Shader _shader;      // We only want to grab this once

        private GameObject _subParent;        // Wrapper to handle rotation and position
        private List<GameObject> _disks;      // Used for updating


        public string Name => "Ionic";

        public bool Scale => false;

        // Build the textures and find the shader, which are both slow operations
        public IonicVisualizer()
        {
            _blank = new Texture2D(TextureX, TextureY, TextureFormat.Alpha8, false);
            _blank.SetPixels(0, 0, TextureX, TextureY, Enumerable.Repeat(Color.clear, TextureX * TextureY).ToArray());
            _blank.Apply(false);

            _texture = new Texture2D(TextureX, TextureY, TextureFormat.Alpha8, false);
            Graphics.CopyTexture(_blank, _texture);
            
            _shader = Shader.Find("DiskGraph");
        }

        // Build the visualiser
        public void Spawn(Transform transform)
        {
            // Spawn _subParent
            _subParent = new GameObject("SubParent");
            // Set position
            _subParent.transform.position = SubPos;
            // Parent is the usual parent
            _subParent.transform.parent = transform;

            // Spawn disks
            _disks = new List<GameObject>();
            // Make NumberOfChunks of them
            for (int i = 0; i < NumberOfChunks; i++)
            {
                // Create a material with tint
                Material m = new Material(_shader);
                m.SetColor("_Tint", Color.HSVToRGB(Random.value, 1, 1));
                m.SetTexture("_Bars", _texture);
                
                // Spawn the actual GameObject from plane
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
                g.name = "Disk";
                // Set its parent
                g.transform.parent = _subParent.transform;
                // Set its size
                g.transform.localScale = Vector3.one * Mathf.Lerp(RingMinScale, RingMaxScale, (float) i / NumberOfChunks);
                // Rotate to face the camera
                g.transform.rotation = Quaternion.Euler(-90, 0, 0);
                // Rotate around by a random amount
                g.transform.Rotate(Vector3.up * Random.value * 360);
                // Set material
                g.GetComponent<MeshRenderer>().material = m;
                // Register for updates
                _disks.Add(g);
            }
            
            // Rotate _subParent for that off-center look
            _subParent.transform.localRotation = Quaternion.Euler(SubRot);
        }

        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples)
        {
            // Build a logarithmic spectrum
            float[] scaledSpectrum = new float[NumberOfChunks];
            float b = Mathf.Pow(sampleSize, 1f / NumberOfChunks);
            float bPow = 1;
            for (int i = 0; i < NumberOfChunks; i++)
            {
                float prevBPow = bPow;
                bPow *= b;
                for (int j = (int) prevBPow; j < bPow - 1; j++)
                    scaledSpectrum[i] += spectrum[j];
            }
            
            // Update texture
            Graphics.CopyTexture(_blank, _texture);
            for (int i = 0; i < SampleBarCount; i++)
            {
                // Height between 2 and 32
                int height = (int) Mathf.Abs(Mathf.Ceil(samples[i] * (SampleBarMaxHeight - SampleBarMinHeight))) + SampleBarMinHeight;
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
            float fallFactor = 1 - 1f / (spectrum.Sum() + 1);
            for (int i = 0; i < NumberOfChunks; i++)
            {
                _disks[i].transform.Rotate(Vector3.up * RotationAmount(i));
                float height = Mathf.Max(
                    -_disks[i].transform.localPosition.z * fallFactor,
                    scaledSpectrum[i] * JumpPower
                    );
                _disks[i].transform.localPosition = Vector3.back * height;
            }
            
            _subParent.transform.localRotation = Quaternion.Euler(SubRot + new Vector3(5 * Mathf.Sin(Time.time), 5 * Mathf.Sin(Time.time * 1.234f), 0));
        }

        private float RotationAmount(int i)
        {
            int mod = (i % 2) * 2 - 1;  // -1 if even, 1 if odd
            return mod * Mathf.Lerp(MaxRotSpeed, MinRotSpeed, (float) i / NumberOfChunks) * Time.deltaTime;
        }
    }
}
