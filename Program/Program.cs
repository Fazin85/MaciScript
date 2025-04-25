namespace MaciScript.Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    class Program
    {
        private static string[] GetFiles(string[] userFilePaths)
        {
            List<string> filePaths = [];
            var coreLib = MaciCoreLibraryLoader.GetFilePaths();

            filePaths.AddRange(userFilePaths);
            filePaths.AddRange(coreLib);

            return [.. filePaths];
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
                var compiler = new MaciCompiler();

                var runtimeData = compiler.Compile(GetFiles(args));

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