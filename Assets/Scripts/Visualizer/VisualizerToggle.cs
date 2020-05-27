using UnityEngine;
using UnityEngine.UI;

namespace Visualizer
{
    [RequireComponent(typeof(Toggle))]
    public class VisualizerToggle : MonoBehaviour
    {
        private Image _image; // The component attached
        private Toggle _toggle; // The component attached
        private IVisualizerModule _visualizer; // Which visualizer this toggle is attached to

        private VisualizerCore _visualizerCore; // Used to tell the parent which visualizers to use

        // Can't use Start since we need to set the variables first
        public void Init(VisualizerCore visualizerCore, IVisualizerModule visualizerModule)
        {
            _visualizerCore = visualizerCore;
            _visualizer = visualizerModule;

            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged
                .AddListener(OnValueChanged); // OnValueChanged will run every time the toggle is clicked

            // Set the text attached to this toggle
            _toggle.GetComponentInChildren<Text>().text = _visualizer.Name;

            _image = GetComponent<Image>();
        }

        // Update the image and the manager
        private void OnValueChanged(bool value)
        {
            if (value)
            {
                _visualizerCore.AddVisualizer(_visualizer);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0.9f);
            }
            else
            {
                _visualizerCore.RemoveVisualizer(_visualizer);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
            }
        }
    }
}