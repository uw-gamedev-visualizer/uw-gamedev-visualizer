using System.Collections.Generic;
using System.Linq;
using Lasp;
using Unity.Collections;
using UnityEngine;

namespace Visualizer
{
    public class VisualizerCore : MonoBehaviour
    {
        // How many samples to split the sound data into.
        public const int SampleSize = 2048;

        // Setters included below
        private static bool _modifyScale;
        private static float _scale;

        // We need a lot of components
        private static AudioLevelTracker _bypassTracker;
        private static AudioLevelTracker _highpassTracker;
        private static AudioLevelTracker _bandpassTracker;
        private static AudioLevelTracker _lowpassTracker;

        private static SpectrumAnalyzer _spectrumAnalyzer;
        
        // Save these so we don't grab too much
        private static NativeSlice<float> _bypassSamples;
        private static NativeSlice<float> _highpassSamples;
        private static NativeSlice<float> _bandpassSamples;
        private static NativeSlice<float> _lowpassSamples;

        private static NativeArray<float> _spectrum;

        private static GameObject _scaledEmpty; // An empty for Monado scaling
        private List<IVisualizerModule> _activeVisualizers; // Visualizers in use, for UpdateVisuals()
        private List<IVisualizerModule> _availableVisualizers; // Visualizers that can be used

        // Used to create the list
        public VisualizerSelector VisualizerSelector;

        private void Start()
        {
            SelectDevice(AudioSystem.InputDevices.First().ID, gameObject);

            // Make an object to scale inside the Monado
            _scaledEmpty = new GameObject("ScaledEmpty");
            _scaledEmpty.transform.parent = transform;
            _scaledEmpty.AddComponent<Rotator>();
            ScaleToMonado();

            // Build the list of visualizers to use
            _availableVisualizers = new List<IVisualizerModule>(VisualizerList.List);
            _activeVisualizers = new List<IVisualizerModule>();

            // Update the UI with the list
            VisualizerSelector.Init(this, _availableVisualizers);
        }

        private void Update()
        {
            _bypassSamples = _bypassTracker.audioDataSlice;
            _highpassSamples = _highpassTracker.audioDataSlice;
            _bandpassSamples = _bandpassTracker.audioDataSlice;
            _lowpassSamples = _lowpassTracker.audioDataSlice;

            _spectrum = _spectrumAnalyzer.spectrumArray;
            
            VisualizerBeatDetector.Update();

            //Update visualizers
            foreach (IVisualizerModule visualizer in _activeVisualizers)
                visualizer.UpdateVisuals();
        }

        // Handles adding new visualizers to the scene
        public void AddVisualizer(IVisualizerModule visualizer)
        {
            // Create an empty to group the visualizer's particles
            GameObject empty = new GameObject(visualizer.Name);
            Transform parent = visualizer.Scale ? _scaledEmpty.transform : transform;
            empty.transform.SetParent(parent, false);

            // Create the visualizer
            visualizer.Spawn(empty.transform);

            // Register the visualizer
            _activeVisualizers.Add(visualizer);
        }

        // Handles removing visualizers from the scene
        public void RemoveVisualizer(IVisualizerModule visualizer)
        {
            // Delete the visualizer object (and all attached particles)
            Destroy(transform.Find((visualizer.Scale ? "ScaledEmpty/" : "") + visualizer.Name).gameObject);

            // Unregister the visualizer so it no long receives updates
            _activeVisualizers.RemoveAll(x => x.GetType() == visualizer.GetType());
        }

        // Fits scaled visualizers into the Monado
        private static void ScaleToMonado()
        {
            _scaledEmpty.transform.position = new Vector3(-0.07f, -2.345f, 0);
            _scaledEmpty.transform.localScale = Vector3.one * 0.055f;
        }

        public void SetScale(float value)
        {
            _scale = value;
        }

        public void SetModifyScale(bool value)
        {
            _modifyScale = value;
        }

        // Get a sample
        public static float Sample(int index, FilterType filter = FilterType.Bypass)
        {
            float multiplier = (_modifyScale ? Mathf.Exp(_scale) : 1);
            switch (filter)
            {
                case FilterType.Bypass:
                    if (index >= _bypassSamples.Length)
                        return 0;
                    return _bypassSamples[index] * multiplier;
                case FilterType.HighPass:
                    if (index >= _highpassSamples.Length)
                        return 0;
                    return _highpassSamples[index] * multiplier;
                case FilterType.BandPass:
                    if (index >= _bandpassSamples.Length)
                        return 0;
                    return _bandpassSamples[index] * multiplier;
                case FilterType.LowPass:
                    if (index >= _lowpassSamples.Length)
                        return 0;
                    return _lowpassSamples[index] * multiplier;
                default:
                    return 0;
            }
        }

        // Get an array of samples. PREFER TO GET INDIVIDUALLY!
        public static float[] Samples(FilterType filter)
        {
            float[] output = new float[SampleSize];
            for (int i = 0; i < SampleSize; i++)
                output[i] = Sample(i, filter);
            return output;
        }

        // Get a spectrum value
        public static float Spectrum(int index)
        {
            if (index >= _spectrum.Length)
                return 0;
            float multiplier = (_modifyScale ? Mathf.Exp(_scale) : 1);
            return _spectrum[index] * multiplier;
        }

        // Get a spectrum range, including startIndex and not including endIndex
        public static float[] Spectrum(int startIndex, int endIndex)
        {
            if (startIndex >= endIndex || endIndex >= _spectrum.Length)
                return null;
            float[] output = new float[endIndex - startIndex];
            for (int i = 0; i < endIndex - startIndex; i++)
                output[i] = Spectrum(i);
            return output;
        }

        // Get the total audio level of the current filter
        public static float Level(FilterType t = FilterType.Bypass)
        {
            switch (t)
            {
                case FilterType.Bypass:
                    return _bypassTracker.currentGain;
                case FilterType.HighPass:
                    return _highpassTracker.currentGain;
                case FilterType.BandPass:
                    return _bandpassTracker.currentGain;
                case FilterType.LowPass:
                    return _lowpassTracker.currentGain;
                default:
                    return 0;
            }
        }

        public static void SelectDevice(string id, GameObject go = null)
        {
            if (go == null)
                go = _bypassTracker.gameObject;
            Destroy(_bypassTracker);
            _bypassTracker = go.AddComponent<AudioLevelTracker>();
            _bypassTracker.deviceID = id;
            _bypassTracker.filterType = FilterType.Bypass;
            _bypassTracker.autoGain = false;
            Destroy(_highpassTracker);
            _highpassTracker = go.AddComponent<AudioLevelTracker>();
            _highpassTracker.deviceID = id;
            _highpassTracker.filterType = FilterType.HighPass;
            _highpassTracker.autoGain = false;
            Destroy(_bandpassTracker);
            _bandpassTracker = go.AddComponent<AudioLevelTracker>();
            _bandpassTracker.deviceID = id;
            _bandpassTracker.filterType = FilterType.BandPass;
            _bandpassTracker.autoGain = false;
            Destroy(_lowpassTracker);
            _lowpassTracker = go.AddComponent<AudioLevelTracker>();
            _lowpassTracker.deviceID = id;
            _lowpassTracker.filterType = FilterType.LowPass;
            _lowpassTracker.autoGain = false;
            
            Destroy(_spectrumAnalyzer);
            _spectrumAnalyzer = go.AddComponent<SpectrumAnalyzer>();
            _spectrumAnalyzer.deviceID = id;
            _spectrumAnalyzer.resolution = SampleSize;
            _spectrumAnalyzer.autoGain = false;
        }
    }
}