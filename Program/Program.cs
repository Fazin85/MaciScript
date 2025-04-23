namespace MaciScript.Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    class Program
    {
        public static List<string> ValidateFiles(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentException("No files specified");
            }

            List<string> validFiles = [];

            foreach (string filePath in args)
            {
                if (File.Exists(filePath))
                {
                    validFiles.Add(filePath);
                }
                else
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
            }

            return validFiles;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: MaciScriptRuntime <filenames>");
                return;
            }

            try
            {
                var files = ValidateFiles(args);

                List<MaciCodeUnit> codeUnits = [];

                foreach (var file in files)
                {
                    var source = File.ReadAllText(file);
                    var codeUnit = MaciCodeUnit.FromString(source);

                    codeUnits.Add(codeUnit);
                }

                var runtimeData = new MaciRuntimeData();
                runtimeData.AddCodeUnits(codeUnits);

                var syscallExcecutor = new SysCallExecutor(new SysCallLoaderPlugins());

                MaciScriptRuntime.Execute(ref runtimeData, syscallExcecutor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}