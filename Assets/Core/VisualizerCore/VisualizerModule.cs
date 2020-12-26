using UnityEngine;

namespace Visualizer
{
    public abstract class VisualizerModule : ScriptableObject
    {
        // Used to populate the list
        public abstract string Name { get; }

        // Called before start. True: Set inside Monado; False: Use whole screen
        public abstract bool Scale { get; }

        // Called once on start.
        public abstract void Spawn(Transform transform);

        // Called every frame
        public abstract void UpdateVisuals();
    }
}