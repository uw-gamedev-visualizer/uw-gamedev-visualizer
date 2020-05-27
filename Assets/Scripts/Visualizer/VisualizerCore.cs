using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class VisualizerCore : MonoBehaviour {
	
	// Used to create the list
	public VisualizerSelector VisualizerSelector;

	public bool ModifyScale;
	public float scale = 1f;

	// How many samples to split the sound data into.
	private const int SAMPLE_SIZE = 1024;

	// These three values are set by the analysis of the sound
	private float rmsValue;
	private float dbValue;
	private float pitchValue;

	private AudioSource source;
	private float[] samples;  // Passed into the audiosouce methods to get data.
	private float[] spectrum; // Same.
	private float sampleRate;

	private GameObject _scaledEmpty;                      // An empty for Monado scaling
	private List<IVisualizerModule> _availableVisualizers; // Visualizers that can be used
	private List<IVisualizerModule> _activeVisualizers;    // Visualizers in use, for UpdateVisuals()

	private void Start ()
	{
		source = GetComponent<AudioSource>();
		samples = new float[SAMPLE_SIZE];
		spectrum = new float[SAMPLE_SIZE];
		sampleRate = AudioSettings.outputSampleRate;
		
		// Make an object to scale inside the Monado
		_scaledEmpty = new GameObject("ScaledEmpty");
		_scaledEmpty.transform.parent = transform;
		_scaledEmpty.AddComponent<Rotator>();

		// Build the list of visualizers to use
		_availableVisualizers = new List<IVisualizerModule>(VisualizerList.List);
		_activeVisualizers = new List<IVisualizerModule>();
		
		// Update the UI with the list
		VisualizerSelector.Init(this, _availableVisualizers);
		
		ScaleToMonado();
	}

	private void Update()
	{
		AnalyzeSound();
		
		if (ModifyScale) {
			spectrum = spectrum.Select(i => i * scale).ToArray();
			samples = samples.Select(i => i * scale).ToArray();
		}
		//Update visualizers
		foreach (IVisualizerModule visualizer in _activeVisualizers)
			visualizer.UpdateVisuals(SAMPLE_SIZE, spectrum, samples);
	}

	// Analyze sound and assign pitch, db, and rms values.
	private void AnalyzeSound()
	{
		source.GetOutputData(samples, 0);

		// Get the RMS Value
		float sum = 0;
		for (int i = 0; i < SAMPLE_SIZE; i++)
		{
			sum += samples[i] * samples[i];
		}
		rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

		// Get the DB Value
		dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

		// Get Sound Spectrum
		source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

		// Find Pitch Value
		float maxV = 0;
		int maxN = 0;
		for (int i = 0; i < SAMPLE_SIZE; i++)
		{
			if (spectrum[i] < maxV || spectrum[i] < 0.0f)
				continue;
			maxV = spectrum[i];
			maxN = i;
		}

		float freqN = maxN;
		var dL = spectrum[(maxN - 1 + SAMPLE_SIZE) % SAMPLE_SIZE] / spectrum[maxN];
		var dR = spectrum[(maxN + 1) % SAMPLE_SIZE] / spectrum[maxN];
		freqN += 0.5f * (dR * dR - dL * dL);
		pitchValue = freqN * (sampleRate / 2) / SAMPLE_SIZE;
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
	private void ScaleToMonado()
	{
		_scaledEmpty.transform.position = new Vector3(-0.07f, -2.345f, 0);
		_scaledEmpty.transform.localScale = Vector3.one * 0.055f;
	}

	public void OnScaleValueChanged(float value) {
		scale = value;
	}

	public void OnModifyScaleValueChanged(bool value) {
		ModifyScale = value;
	}
}
