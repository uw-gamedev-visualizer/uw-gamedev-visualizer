using UnityEngine;

namespace Visualizer
{
    public class VisualizerSettings : MonoBehaviour
    {
        public void SetScale(float value)
        {
            VisualizerCore.Scale = value;
        }

        public void SetModifyScale(bool value)
        {
            VisualizerCore.ModifyScale = value;
        }
    }
}
