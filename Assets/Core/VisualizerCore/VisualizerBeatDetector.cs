using System.Collections.Generic;
using System.Linq;
using Lasp;
using UnityEngine;

namespace Visualizer
{
    public static class VisualizerBeatDetector
    {
        public static float BufferTime = 0.5f;
        public static float MinBeatMultiplier = 2;
        public static float NoiseGate = 0.02f;

        private class SpectrumData
        {
            public float time;
            public float max;
        }
        
        private static Queue<SpectrumData> _prevSpectra = new Queue<SpectrumData>();

        private static float _level;
        private static bool _beatOld = true;
        private static bool _beat;

        public static void Update()
        {
            _beatOld = true;
            _level = VisualizerCore.Level(FilterType.LowPass);
            UpdatePrevs();
        }

        public static bool IsBeat()
        {
            if (!_beatOld)
            {
                return _beat;
            }

            if (_prevSpectra.Count < 1)
            {
                return false;
            }

            float prevAvg = _prevSpectra.Sum(x => x.max) / _prevSpectra.Count;
            _beat = _level > NoiseGate && _level > prevAvg * MinBeatMultiplier;
            _beatOld = false;
            return _beat;
        }

        /*
        public struct BeatDetectorData
        {
            public float level;
            public float prevAvg;
            public float minBeatMultiplier;
            public float noiseGate;
        }

        [System.Obsolete("Debug only!", false)]
        public static BeatDetectorData GetData()
        {
            return new BeatDetectorData
            {
                level = _level,
                noiseGate = NoiseGate,
                minBeatMultiplier = MinBeatMultiplier,
                prevAvg = _prevSpectra.Sum(x => x.max) / _prevSpectra.Count
            };
        }
        */

        private static void UpdatePrevs()
        {
            _prevSpectra.Enqueue(new SpectrumData
            {
                time = Time.unscaledTime,
                max = _level
            });

            while (_prevSpectra.Peek().time < Time.unscaledTime - BufferTime)
            {
                _prevSpectra.Dequeue();
            }
        }
    }
}
