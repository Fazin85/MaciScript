using System.Text.Json;

namespace MaciScript
{
    public class MaciCoreLibraryLoader
    {
        public static string[] GetFilePaths()
        {
            try
            {
                var filePaths = JsonSerializer.Deserialize<string[]>(File.ReadAllText("core/files.json")) ?? throw new Exception("failed to get file paths json");
                return filePaths;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to get core lib file paths: {e.Message}");
            }
        }
    }
}
