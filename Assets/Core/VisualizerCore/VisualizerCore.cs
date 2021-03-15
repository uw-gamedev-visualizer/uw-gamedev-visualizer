using System;
using System.Linq;
using Lasp;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visualizer
{
    public static class VisualizerCore
    {
    
        // Length of the spectrum array
        public const int SpectrumSize = 2048;
    
        public static float Scale = 0;
        public static bool ModifyScale = false;
        
        // Where to attach trackers
        private static GameObject _trackerGameObject;
        
        // We need a lot of components
        private static AudioLevelTracker _bypassTracker;
        private static AudioLevelTracker _highpassTracker;
        private static AudioLevelTracker _bandpassTracker;
        private static AudioLevelTracker _lowpassTracker;
        private static SpectrumAnalyzer _spectrumAnalyzer;

        public static void Init(GameObject parent)
        {
            _trackerGameObject = new GameObject("Trackers");
            _trackerGameObject.transform.parent = parent.transform;
        }
        
        private static AudioLevelTracker GetTracker(FilterType filter)
        {
            return filter switch
            {
                FilterType.Bypass => _bypassTracker,
                FilterType.HighPass => _highpassTracker,
                FilterType.BandPass => _bandpassTracker,
                FilterType.LowPass => _lowpassTracker,
                _ => throw new ArgumentException("Tried to get invalid tracker type")
            };
        }

        // Get a sample
        public static float Sample(int index, FilterType filter = FilterType.Bypass)
        {
            NativeSlice<float> samples = GetTracker(filter).audioDataSlice;

            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            float f = samples[index];
            if (float.IsNaN(f))
            {
                f = 0;
            }
            return Mathf.Clamp01(f * multiplier);
        }

        // Get an array of samples
        public static float[] Samples(FilterType filter)
        {
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            
            NativeSlice<float> samples = GetTracker(filter).audioDataSlice;
            float[] output = new float[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                float f = samples[i];
                if (float.IsNaN(f))
                {
                    f = 0;
                }
                output[i] = Mathf.Clamp01(f * multiplier);
            }

            return output;
        }

        // Get a spectrum value
        public static float Spectrum(int index)
        {
            NativeSlice<float> spectrum = _spectrumAnalyzer.spectrumArray;
            
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            float f = spectrum[index];
            if (float.IsNaN(f))
            {
                f = 0;
            }
            return Mathf.Clamp01(f * multiplier);
        }

        // Get a spectrum range, including startIndex and not including endIndex
        public static float[] Spectrum(int startIndex, int endIndex)
        {
            if (startIndex >= endIndex || startIndex < 0 || endIndex > SpectrumSize)
            {
                throw new ArgumentException("Invalid range");
            }
            
            NativeSlice<float> spectrum = _spectrumAnalyzer.spectrumArray;
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);

            float[] output = new float[endIndex - startIndex];
            for (int i = 0; i < endIndex - startIndex; i++)
            {
                float f = spectrum[i + startIndex];
                if (float.IsNaN(f))
                {
                    f = 0;
                }
                output[i] = Mathf.Clamp01(f * multiplier);
            }
            
            return output;
        }

        // Get the whole spectrum
        public static float[] Spectrum()
        {
            return Spectrum(0, SpectrumSize);
        }

        // Get the total audio level of the current filter
        public static float Level(FilterType filter = FilterType.Bypass)
        {
            NativeSlice<float> samples = GetTracker(filter).audioDataSlice;
            if (samples.Length == 0)
            {
                return 0;
            }

            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            return GetTracker(filter).normalizedLevel;
        }

        // When changing devices we need to make new trackers
        public static void SelectDevice(string deviceId)
        {
            Object.Destroy(_bypassTracker);
            _bypassTracker = MakeTracker(FilterType.Bypass, deviceId);
            Object.Destroy(_highpassTracker);
            _highpassTracker = MakeTracker(FilterType.HighPass, deviceId);
            Object.Destroy(_bandpassTracker);
            _bandpassTracker = MakeTracker(FilterType.BandPass, deviceId);
            Object.Destroy(_lowpassTracker);
            _lowpassTracker = MakeTracker(FilterType.LowPass, deviceId);
            
            Object.Destroy(_spectrumAnalyzer);
            _spectrumAnalyzer = _trackerGameObject.AddComponent<SpectrumAnalyzer>();
            _spectrumAnalyzer.deviceID = deviceId;
            _spectrumAnalyzer.resolution = SpectrumSize;
            _spectrumAnalyzer.dynamicRange = 50;
        }
        
        // Makes and configures a new tracker
        private static AudioLevelTracker MakeTracker(FilterType filter, string deviceId)
        {
            AudioLevelTracker tracker = _trackerGameObject.AddComponent<AudioLevelTracker>();
            tracker.filterType = filter;
            tracker.deviceID = deviceId;
            tracker.smoothFall = false;
            return tracker;
        }
    }
}
