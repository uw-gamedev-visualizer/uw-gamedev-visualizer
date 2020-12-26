using System.Collections.Generic;
using UnityEngine;

namespace Visualizer
{
    public class VisualizerSelector : MonoBehaviour
    {
        public GameObject ScrollView; // Toggle prefabs go in here
        public GameObject TogglePrefab;

        public void Init(VisualizersManager visualizersManager, IList<VisualizerModule> visualizerList)
        {
            for (int i = 0; i < visualizerList.Count; i++)
            {
                // Make the toggle in the UI
                GameObject toggle = Instantiate(TogglePrefab, ScrollView.transform);
                toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(400, -50 - 100 * i);

                // Set up the toggle's scripts
                VisualizerToggle script = toggle.GetComponent<VisualizerToggle>();
                script.Init(visualizersManager, visualizerList[i]);
            }
        }
    }
}