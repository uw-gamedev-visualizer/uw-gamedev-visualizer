using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Threading;


// Get the title of the window playing YouTube
public class GetWindowTitle : MonoBehaviour
{
    private Text _text;
    private Process _p;
    private Thread _t;
    private string _songName;

    private void Start () {
        _text = GetComponent<Text>();
        
        _p = new Process
        {
            StartInfo = {CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, FileName = Application.dataPath + "/Scripts/Other/FindYT.exe"}
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
