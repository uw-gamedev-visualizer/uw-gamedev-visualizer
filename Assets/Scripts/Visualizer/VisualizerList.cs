using Visualizers;

public static class VisualizerList
{
    public static readonly IVisualizerModule[] List = {
        new SoundVisual(),
        new WindVisualizer(),
        new ModelVisualizer(),
        new JoseVisualizer(),
        new IonicVisualizer(),
        new GridVisualizer(),
        new VFXVisualizer()
    };
}
