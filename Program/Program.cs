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
                Console.WriteLine("Usage: MaciScriptRuntime <filenames>");
                return;
            }

            try
            {
                var compiler = new MaciCompiler(new MaciCompilerSettings());

                var runtimeData = compiler.Compile(args);

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