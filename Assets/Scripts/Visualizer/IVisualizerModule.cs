using UnityEngine;

public interface IVisualizerModule
{
    // Used to populate the list
    string Name { get; }
    
    // Called before start. True: Set inside Monado; False: Use whole screen
    bool Scale { get; }
    
    // Called once on start.
    void Spawn(Transform transform);
    
    // Called every frame
    void UpdateVisuals(int sampleSize, float[] spectrum, float[] samples);
}
