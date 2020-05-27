using UnityEngine;

// Changes object's audiosource to the stereo mix
// Requires headphones to be plugged in to 3.5mm jack
// Requires Stereo Mix enabled and set to default.
[RequireComponent(typeof(AudioSource))]
public class GetMicInput : MonoBehaviour {

    private AudioSource _aud;
    public bool UseDesktopAudio;
    
    // Use this for initialization
    private void Start () {
        if (!UseDesktopAudio) return;

        // TODO add a way to change input devices
        string preferredDevice = null;
        foreach (string device in Microphone.devices) {
            print(device);
            if (device.Contains("CABLE Output"))
            {
                preferredDevice = device;
                break;
            }
            if (device.Contains("Stereo Mix"))
            {
                preferredDevice = device;
                break;
            }
        }
        
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.dspBufferSize = 8;
        config.numRealVoices = 1;
        config.numVirtualVoices = 1;
        config.speakerMode = AudioSpeakerMode.Mono;
        
        _aud = GetComponent<AudioSource>();
        // If there's no Stereo Mix or VB Cable, preferredDevice will be null
        _aud.clip = Microphone.Start(preferredDevice, true, 1, AudioSettings.outputSampleRate);
        // Wait until the recording has started
        _aud.PlayDelayed(0.001f);
    }
}
