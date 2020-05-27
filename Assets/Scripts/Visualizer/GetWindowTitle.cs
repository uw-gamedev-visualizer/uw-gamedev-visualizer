using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


// Get the title of the window playing YouTube
namespace Visualizer
{
    public class GetWindowTitle : MonoBehaviour
    {
        private Process _p;
        private string _songName;
        private Thread _t;
        private Text _text;

        private void Start()
        {
            _text = GetComponent<Text>();

            _p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true,
                    FileName = Application.streamingAssetsPath + "/FindYT.exe"
                }
            };

            _t = new Thread(GetTitle);
            _t.Start();
        }

        private void Update()
        {
            _text.text = "Currently Playing: " + _songName;
        }

        private void GetTitle()
        {
            while (true)
            {
                _p.Start();
                _p.WaitForExit();
                string windowName = _p.StandardOutput.ReadToEnd();
                if (windowName.Length < 14)
                {
                    _songName = "";
                    Thread.Sleep(1000);
                    continue;
                }

                windowName = windowName.Split('\n')[0];

                // Format the window title
                _songName = Regex.Replace(windowName, "- YouTube.*", "");

                Thread.Sleep(3000);
            }
        }
    }
}