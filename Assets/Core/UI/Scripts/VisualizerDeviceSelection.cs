using System.Linq;
using Lasp;
using UnityEngine;
using UnityEngine.UI;

namespace Visualizer
{
    public class VisualizerDeviceSelection : MonoBehaviour
    {
        public Dropdown DeviceList;
    
        private void Start()
        {
            DeviceList.options.AddRange (Lasp.AudioSystem.InputDevices.Select(dev => new DeviceItem(dev)));

            DeviceList.RefreshShownValue();

            // If there is any input device, select the first one
            if (AudioSystem.InputDevices.Any())
            {
                OnDeviceSelected(0);
            }
        }
    
        public void OnDeviceSelected(int index)
        {
            string id = ((DeviceItem) DeviceList.options[index]).id;
            VisualizerCore.SelectDevice(id);
        }

        private class DeviceItem : Dropdown.OptionData
        {
            public string id;
            public DeviceItem(in DeviceDescriptor device) => (text, id) = (device.Name, device.ID);
        }
    }
}
