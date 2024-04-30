using System.IO;
using System.IO.Compression;

namespace regiondeployer
{
    public class ZipManager
    {
        public const string DefaultZipName = "regionfarmer";

        public string ZipUpProject(string projectPath, string outputFileName, Action<string> logMessage)
        {
            logMessage($"Zipping project directory... {projectPath}");
            string output = Path.Combine(Directory.GetCurrentDirectory(), $"{outputFileName}.zip");

            if (File.Exists(output))
            {
                File.Delete(output);
            }

            ZipFile.CreateFromDirectory(projectPath, output);
            logMessage("directory zipped.");
            return output;
        }
    }
}