using System;
using System.IO;
using System.Text;

namespace ML_Agents.Examples.Soccer.Scripts
{
    public class StatsLogger
    {
        private readonly string _filePath;

        public StatsLogger(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
            {
                WriteHeader();
            }
        }

        private void WriteHeader()
        {
            var header = "Physics Time (ms),Script Time (ms),System Memory (MB),Wall Time (ms),Blue Cumulative Reward";
            File.WriteAllText(_filePath, header + Environment.NewLine);
        }

        public void LogStats(double physicsTime, double scriptTime, float systemMemory, double wallTime, float blueCumulativeReward)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{physicsTime:F1},{scriptTime},{systemMemory},{wallTime:F1},{blueCumulativeReward}");
            File.AppendAllText(_filePath, sb.ToString());
        }
    }
}
