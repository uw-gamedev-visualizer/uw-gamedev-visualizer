using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace Visualizers {

    public class VFXVisualizer : IVisualizerModule
    {
        public string Name => "VFX";

        public bool Scale => false;

        public float rate = 7f;

        private float _lastSpeed;
        private VisualEffect clip;

        public void Spawn(Transform transform) {
            GameObject visual = Object.Instantiate(Resources.Load("VFXGRAPHS/TestVFX", typeof(GameObject)) as GameObject, Vector3.zero, Quaternion.identity, transform);
            clip = visual.GetComponent<VisualEffect>();
            if (clip == null)
                return;
        }

        public void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples) {
            float sum = spectrum.Sum();
            float amount = sum * rate * 10;
            float speed = Mathf.Lerp(_lastSpeed, amount, 0.3f);
            _lastSpeed = speed;
            Debug.Log(speed);
            clip.playRate = speed;
        }
    }
}
