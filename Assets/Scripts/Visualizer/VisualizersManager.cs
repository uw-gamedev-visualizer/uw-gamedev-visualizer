using System.Collections.Generic;
using UnityEngine;

namespace Visualizer
{
    public class VisualizersManager : MonoBehaviour
    {
        private GameObject _unscaledEmpty;
        private GameObject _scaledEmpty; // for Monado scaling
        private Dictionary<IVisualizerModule, GameObject> _activeVisualizers = new Dictionary<IVisualizerModule, GameObject>(); // Visualizers in use, for UpdateVisuals()

        // Used to create the list
        public VisualizerSelector VisualizerSelector;

        private void Start()
        {
            VisualizerCore.Init(gameObject);
            VisualizerSelector.Init(this);

            MakeUnscaledEmpty();
            MakeScaledEmpty();
        }

        private void Update()
        {
            VisualizerBeatDetector.Update();

            foreach (IVisualizerModule visualizer in _activeVisualizers.Keys)
            {
                visualizer.UpdateVisuals();
            }
        }

        // Handles adding new visualizers to the scene
        public void AddVisualizer(IVisualizerModule visualizer)
        {
            // Create an empty to group the visualizer's particles
            GameObject parent = visualizer.Scale ? _scaledEmpty : _unscaledEmpty;
            GameObject empty = new GameObject(visualizer.Name);
            empty.transform.SetParent(parent.transform, false);

            visualizer.Spawn(empty.transform);
            _activeVisualizers.Add(visualizer, empty);
        }

        // Handles removing visualizers from the scene
        public void RemoveVisualizer(IVisualizerModule visualizer)
        {
            Destroy(_activeVisualizers[visualizer]);
            _activeVisualizers.Remove(visualizer);
        }

        // Fits scaled visualizers into the Monado
        private void MakeScaledEmpty()
        {
            _scaledEmpty = new GameObject("ScaledEmpty");
            _scaledEmpty.transform.parent = transform;
            
            _scaledEmpty.transform.position = new Vector3(-0.07f, -2.345f, 0);
            _scaledEmpty.transform.localScale = new Vector3(0.275f, 0.275f, 1f);
        }

        private void MakeUnscaledEmpty()
        {
            _unscaledEmpty = new GameObject("UnscaledEmpty");
            _unscaledEmpty.transform.parent = transform;
        }
    }
}
