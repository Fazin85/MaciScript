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

            // we must add core lib after user code or the interpreter will get fucked up
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
                MaciCompiler.LogExpandedSource = true;
                
                var runtimeData = MaciCompiler.Compile(GetFiles(args));

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