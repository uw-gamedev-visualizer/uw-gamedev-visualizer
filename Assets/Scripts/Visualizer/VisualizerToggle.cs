using UnityEngine;
using UnityEngine.UI;

namespace Visualizer
{
    [RequireComponent(typeof(Toggle))]
    public class VisualizerToggle : MonoBehaviour
    {
        private Image _image; // The component attached
        private Toggle _toggle; // The component attached
        private VisualizerModule _visualizer; // Which visualizer this toggle is attached to

        private VisualizersManager _visualizersManager; // Used to tell the parent which visualizers to use

        // Can't use Start since we need to set the variables first
        public void Init(VisualizersManager visualizersManager, VisualizerModule visualizerModule)
        {
            _visualizersManager = visualizersManager;
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
                _visualizersManager.AddVisualizer(_visualizer);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0.9f);
            }
            else
            {
                _visualizersManager.RemoveVisualizer(_visualizer);
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
            }
        }
    }
}