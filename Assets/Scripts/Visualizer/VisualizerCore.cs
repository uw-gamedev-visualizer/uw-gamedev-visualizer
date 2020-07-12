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
    
        // How many samples to split the sound data into.
        public const int SampleSize = 2048;
    
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
            switch (filter)
            {
                case FilterType.Bypass:
                    return _bypassTracker;
                case FilterType.HighPass:
                    return _highpassTracker;
                case FilterType.BandPass:
                    return _bandpassTracker;
                case FilterType.LowPass:
                    return _lowpassTracker;
                default:
                    throw new ArgumentException("Tried to get invalid tracker type");
            }
        }

        // Get a sample
        public static float Sample(int index, FilterType filter = FilterType.Bypass)
        {
            NativeSlice<float> samples = GetTracker(filter).audioDataSlice;

            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            return samples[index] * multiplier;
        }

        // Get an array of samples
        public static float[] Samples(FilterType filter)
        {
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            
            NativeSlice<float> samples = GetTracker(filter).audioDataSlice;
            float[] output = new float[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = samples[i] * multiplier;
            }

            return output;
        }

        // Get a spectrum value
        public static float Spectrum(int index)
        {
            NativeSlice<float> spectrum = _spectrumAnalyzer.spectrumArray;
            
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);
            return Mathf.Clamp01(Mathf.Abs(spectrum[index]) * multiplier);
        }

        // Get a spectrum range, including startIndex and not including endIndex
        public static float[] Spectrum(int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
            {
                throw new ArgumentException("Invalid range");
            }
            
            NativeSlice<float> spectrum = _spectrumAnalyzer.spectrumArray;
            float multiplier = (ModifyScale ? Mathf.Exp(Scale) : 1);

            float[] output = new float[endIndex - startIndex];
            for (int i = 0; i < endIndex - startIndex; i++)
            {
                if (i >= spectrum.Length)
                {
                    break;
                }
                output[i] = Mathf.Clamp01(Mathf.Abs(spectrum[i]) * multiplier);
            }

            return output;
        }

        public static float[] Spectrum()
        {
            return Spectrum(0, SampleSize);
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
            return samples.Max(Mathf.Abs) * multiplier;
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
            _spectrumAnalyzer.resolution = SampleSize;
            _spectrumAnalyzer.dynamicRange = 50;
            
            // Fixes a bug where spectrumArray may be filled with unexpected values
            // Theory: SpectrumAnalyzer doesn't calloc and inits lazily. The first poll gets garbage, but later polls are good.
            float f = _spectrumAnalyzer.spectrumArray[0];
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
