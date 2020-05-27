using System.Collections.Generic;
using UnityEngine;

public class VisualizerSelector : MonoBehaviour
{
    public GameObject ScrollView;   // Toggle prefabs go in here
    public GameObject TogglePrefab;

    public void Init(VisualizerCore visualizerCore, List<IVisualizerModule> availableVisualizers)
    {
        for (int i = 0; i < availableVisualizers.Count; i++)
        {
            // Make the toggle in the UI
            GameObject toggle = Instantiate(TogglePrefab, ScrollView.transform);
            toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(400, -50 - 100 * i);
            
            // Set up the toggle's scripts
            VisualizerToggle script = toggle.GetComponent<VisualizerToggle>();
            script.Init(visualizerCore, availableVisualizers[i]);
        }
    }
}
