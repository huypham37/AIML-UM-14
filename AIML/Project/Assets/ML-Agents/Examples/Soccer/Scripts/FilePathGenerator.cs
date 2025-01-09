using System.IO;

namespace ML_Agents.Examples.Soccer.Scripts
{
    public class FilePathGenerator
    {
        private int increment = 0;
        public string generateFilePath()
        {
            increment++;
            var currentDirectory = Directory.GetCurrentDirectory();
            var statsDirectory = Path.Combine(currentDirectory, "stats");

            var timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH") + "-" + increment;
            var csvFileName = $"SoccerStats_{timeStamp}.csv";
            var filePath = Path.Combine(statsDirectory, csvFileName);

            return filePath;
        }
    }
}
