namespace MaciScript
{
    public class MaciCompiler()
    {
        public static bool LogExpandedSource = false;

        public static MaciRuntimeData Compile(string[] filePaths)
        {
            MaciInputFileData[] fileData = new MaciInputFileData[filePaths.Length];

            for (int i = 0; i < fileData.Length; i++)
            {
                fileData[i].FilePath = filePaths[i];
                fileData[i].FileContent = File.ReadAllText(filePaths[i]);
            }

            return Compile(fileData);
        }

        public static MaciRuntimeData Compile(MaciInputFileData[] inputFileData)
        {
            ProcessMacros(inputFileData);

            if (LogExpandedSource)
            {
                Console.WriteLine("Expanded source files:");

                foreach (var file in inputFileData)
                {
                    Console.WriteLine($"file path: {file.FilePath}");
                    Console.WriteLine(file.FileContent);
                }
            }

            var filePaths = inputFileData.Select(x => x.FilePath).ToArray();
            var sources = inputFileData.Select(x => x.FileContent).ToArray();

            var compilationData = new MaciCompilationData();
            List<MaciSymbolCollection> symbolCollections = [];

            for (int i = 0; i < sources.Length; i++)
            {
                symbolCollections.Add(CollectSymbols(ref compilationData, symbolCollections, sources, filePaths, i));
            }

            List<MaciCodeUnit> codeUnits = [];

            for (int i = 0; i < symbolCollections.Count; i++)
            {
                codeUnits.Add(new()
                {
                    Functions = symbolCollections[i].Functions,
                    Labels = symbolCollections[i].Labels,
                    Strings = symbolCollections[i].Strings,
                    Instructions = CollectInstructions(i, symbolCollections, sources[i])
                });
            }

            var runtimeData = new MaciRuntimeData();
            runtimeData.AddCodeUnits(codeUnits);

            return runtimeData;
        }

        private static void ProcessMacros(MaciInputFileData[] inputFileData)
        {
            var macroProcessor = new MaciMacroProcessor();

            macroProcessor.CollectMacroDefinitions(inputFileData);
            var expandedFileContent = macroProcessor.ExpandMacros(inputFileData);

            for (int i = 0; i < expandedFileContent.Length; i++)
            {
                inputFileData[i].FileContent = expandedFileContent[i];
            }
        }

        private static MaciSymbolCollection CollectSymbols(
            ref MaciCompilationData compilationData,
            List<MaciSymbolCollection> existingSymbolCollections,
            string[] sources,
            string[] sourceFilePaths,
            int fileIndex)
        {
            MaciFunctionLoader functionLoader = new();
            MaciStringLoader stringLoader = new();
            MaciLabelLoader labelLoader = new();

            List<string> imports = [];
            List<MaciFunction> functions = [];
            List<MaciLabel> labels = [];
            List<string> strings = [];
            Dictionary<int, string> stringLines = [];

            try
            {
                // First pass: collect all labels and function addresses
                int instructionIndex = compilationData.InstructionIndex;
                int instructionCount = 0;
                var lines = sources[fileIndex].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    //I should probably abstract this loader mess but idc
                    if (string.IsNullOrWhiteSpace(line) ||
                        line.StartsWith(';') ||
                        functionLoader.TryLoad(ref compilationData, existingSymbolCollections, line, instructionIndex, functions) ||
                        labelLoader.TryLoad(ref compilationData, existingSymbolCollections, line, instructionIndex, labels) ||
                        MaciImportLoader.TryLoad(line, sourceFilePaths, imports)
                        )
                    {
                        continue;
                    }

                    stringLoader.TryLoad(ref compilationData, i, line, strings, stringLines);

                    instructionIndex++;
                    instructionCount++;
                }

                //increment this compilation's counts
                compilationData.FunctionOffset += functions.Count;
                compilationData.LabelOffset += labels.Count;
                compilationData.StringOffset += strings.Count;
                compilationData.InstructionIndex += instructionCount;

                return new()
                {
                    FilePath = sourceFilePaths[fileIndex],
                    Imports = [.. imports],
                    Functions = [.. functions],
                    Labels = [.. labels],
                    Strings = [.. strings],
                    FunctionNameToIndex = functionLoader.FunctionNameToIndex,
                    LabelNameToIndex = labelLoader.LabelNameToIndex,
                    StringToIndex = stringLoader.StringToIndex,
                    StringLines = stringLines
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error collecting symbols: {ex.Message}");
            }
        }

        private static MaciInstruction[] CollectInstructions(int symbolsIndex, List<MaciSymbolCollection> symbolCollections, string source)
        {
            try
            {
                var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                List<MaciInstruction> instructions = [];

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip labels, comments, and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.EndsWith(':') || line.StartsWith("import"))
                        continue;

                    instructions.Add(MaciScriptParser.ParseInstruction(new(symbolsIndex, symbolCollections, line, i)));
                }

                return [.. instructions];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error collecting instructions: {ex.Message}");
            }
        }
    }
}
