using Visualizer.Visualizers;

namespace Visualizer
{
    public static class VisualizerList
    {
        public static readonly IVisualizerModule[] List =
        {
            new MonadoBarVisualizer(),
            new WindVisualizer(),
            new ModelVisualizer(),
            //new JoseVisualizer(),
            new IonicVisualizer(),
            new GridVisualizer(),
            new VFXVisualizer(),
            //new SpookyVisualizer(),
            new FireVisualizer()
        };
    }
}