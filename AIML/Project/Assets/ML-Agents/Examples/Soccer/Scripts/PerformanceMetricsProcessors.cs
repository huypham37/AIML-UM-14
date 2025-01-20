using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Profiling;

namespace ML_Agents.Examples.Soccer.Scripts
{

    public class PerformanceMetricsProcessor
    {
        private List<float> mainThreadTimeSamples = new List<float>();
        private List<float> physicsTimeSamples = new List<float>();
        private List<float> systemMemorySamples = new List<float>();
        private List<float> blueRewardSamples = new List<float>();

        private float lastRecordTime;
        private StatsLogger _statsLogger;

        public PerformanceMetricsProcessor(StatsLogger statsLogger)
        {
            _statsLogger = statsLogger;
        }

        public void AddMainThreadTimeSample(float mainThreadTime)
        {
            mainThreadTimeSamples.Add(mainThreadTime);
        }

        public void AddPhysicsTimeSample(float physicsTime)
        {
            physicsTimeSamples.Add(physicsTime);
        }

        public void AddSystemMemorySample(float systemMemory)
        {
            systemMemorySamples.Add(systemMemory);
        }

        public void AddBlueRewardSample(float blueReward)
        {
            blueRewardSamples.Add(blueReward);
        }

        public void ProcessMetrics(float wallTime, ProfilerRecorder systemMemoryRecorder,
            ProfilerRecorder physicsRecorder, ProfilerRecorder scriptRecorder, float blueCumulativeReward)
        {
            if (wallTime - lastRecordTime >= 30000)
            {
                lastRecordTime = wallTime;

                var meanMainThreadTime = mainThreadTimeSamples.Count > 0 ? mainThreadTimeSamples.Average() : 0;
                var meanPhysicsTime = physicsTimeSamples.Count > 0 ? physicsTimeSamples.Average() : 0;
                var meanSystemMemory = systemMemorySamples.Count > 0 ? systemMemorySamples.Average() : 0;
                var meanBlueReward = blueRewardSamples.Count > 0 ? blueRewardSamples.Average() : 0;
                Debug.Log("mean blue reward: " + meanBlueReward);

                mainThreadTimeSamples.Clear();
                physicsTimeSamples.Clear();
                systemMemorySamples.Clear();
                blueRewardSamples.Clear();


                var systemMemory = systemMemoryRecorder.LastValue / (1024 * 1024);
                var physicsTime = physicsRecorder.LastValue * (1e-6f);
                var scriptTime = scriptRecorder.LastValue * (1e-6f);

                var sb = new StringBuilder(500);
                sb.AppendLine($"Mean Main Thread Time: {meanMainThreadTime:F1} ms");
                sb.AppendLine($"Mean Physics Time: {meanPhysicsTime:F1} ms");
                sb.AppendLine($"Mean System Memory: {meanSystemMemory:F1} MB");
                sb.AppendLine($"Script Time: {scriptTime:F1} ms");
                sb.AppendLine($"Wall Time: {wallTime:F1} ms");

                Debug.Log(sb.ToString());

                _statsLogger.LogStats(meanPhysicsTime, meanMainThreadTime, meanSystemMemory, wallTime,
                    meanBlueReward);
            }
        }
    }
}
