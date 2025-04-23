namespace MaciScript.Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: MaciScriptRuntime <filename> [--debug]");
                return;
            }

            try
            {
                bool debugMode = args.Length > 1 && args[1] == "--debug";
                string filename = args[0];

                if (!File.Exists(filename))
                {
                    Console.WriteLine($"Error: File '{filename}' not found.");
                    return;
                }

                string source = File.ReadAllText(filename);
                var codeUnit = MaciCodeUnit.FromString(source);

                var runtimeData = new MaciRuntimeData();
                runtimeData.AddCodeUnits([codeUnit]);

                var syscallExcecutor = new SysCallExecutor(new SysCallLoaderPlugins());

                if (debugMode)
                {
                    Console.WriteLine($"Debug mode enabled. Loading program from '{filename}'...");
                }

                MaciScriptRuntime.Execute(ref runtimeData, syscallExcecutor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}