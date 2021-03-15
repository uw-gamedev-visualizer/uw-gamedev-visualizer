using System.Collections.Generic;
using System.Linq;
using Lasp;
using UnityEngine;

namespace Visualizer
{
    public static class VisualizerBeatDetector
    {
        public const float BufferTime = 2.2f;
        public const float NoiseGate = 0.6f;
        public const float MinBpm = 99;
        public const float MaxBpm = 183;

        public static float LastBeat { get; private set; }
        public static float SecsPerBeat { get; private set; }
        public static float BPM => 60 / SecsPerBeat;
        public static float SecsToBeat => LastBeat + SecsPerBeat - Time.unscaledTime;
        public static float BeatFraction => 1 - SecsToBeat / SecsPerBeat;
        public static bool IsBeat { get; private set; }

        private static bool _bufferFull;

        private struct BufferData
        {
            public float Time;
            public float Level;
        }

        private static Queue<BufferData> _buffer = new Queue<BufferData>();

        public static void Update()
        {
            IsBeat = false;
            float level = Mathf.Max(VisualizerCore.Level(FilterType.LowPass), 0);
            UpdateBuffer(level);
            
            if (_bufferFull)
            {
                float oldLastBeat = LastBeat;
                CheckBeat();
                if (oldLastBeat != LastBeat)
                {
                    IsBeat = true;
                }
            }

            DebugVisualize(_buffer.ToList(), Color.cyan);
        }

        private static void UpdateBuffer(float level)
        {
            _buffer.Enqueue(new BufferData
            {
                Time = Time.unscaledTime,
                Level = level
            });

            _bufferFull = false;
            while (_buffer.Peek().Time < Time.unscaledTime - BufferTime)
            {
                _bufferFull = true;
                _buffer.Dequeue();
            }
        }

        private static void CheckBeat()
        {
            BufferData[] bufferList = _buffer.ToArray();
            //float avg = bufferList.Select(x => x.Level).Average();
            List<BufferData> jumps = new List<BufferData>();
            for (int i = 1; i < bufferList.Length; i++)
            {
                float avg = 0;
                float max = 0;
                for (int j = Mathf.Max(i - 10, 0); j < i; j++)
                {
                    avg += bufferList[j].Level;
                    max = Mathf.Max(max, bufferList[j].Level);
                }
                avg /= 10;
                
                if (avg < 0.5f && max < 0.8f && bufferList[i].Level > 0.8f)
                {
                    jumps.Add(bufferList[i]);
                }
            }
            DebugVisualize(jumps, Color.red);

            if (jumps.Count < 2)
            {
                return;
            }

            List<float> gaps = new List<float>();
            for (int i = 1; i < jumps.Count; i++)
            {
                gaps.Add(jumps[i].Time - jumps[i - 1].Time);
            }

            SecsPerBeat = gaps[gaps.Count - 1]; //TODO
            FixSecsPerBeat();
            LastBeat = jumps[jumps.Count - 1].Time;
        }

        private static void FixSecsPerBeat()
        {
            float minSecs = 60 / MaxBpm;
            for (int i = 0; i < 3 && SecsPerBeat < minSecs; i++)
            {
                SecsPerBeat *= 2;
            }
            float maxSecs = 60 / MinBpm;
            for (int i = 0; i < 3 && SecsPerBeat > maxSecs; i++)
            {
                SecsPerBeat /= 2;
            }
        }

        private static void DebugVisualize(List<BufferData> list, Color color)
        {
            if (list.Count < 1)
            {
                return;
            }

            BufferData prev = new BufferData {Time = -1};
            foreach (BufferData bd in list)
            {
                if (prev.Time >= 0)
                {
                    Vector3 prevPos = new Vector3(-(Time.unscaledTime - prev.Time), prev.Level);
                    Vector3 nextPos = new Vector3(-(Time.unscaledTime - bd.Time), bd.Level);
                    Debug.DrawLine(prevPos, nextPos, color);
                }

                prev = bd;
            }
        }
    }
}
