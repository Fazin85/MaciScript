namespace MaciScript
{
    public static class MaciCompiler
    {
        public static MaciRuntimeData Compile(string[] sources)
        {
            var compilationData = new MaciCompilationData();

            List<MaciSymbolsCollection> symbolsCollections = [];

            foreach (var source in sources)
            {
                symbolsCollections.Add(CollectSymbols(compilationData, source));
            }

            List<MaciCodeUnit> codeUnits = [];

            for (int i = 0; i < symbolsCollections.Count; i++)
            {
                codeUnits.Add(new()
                {
                    Functions = symbolsCollections[i].Functions,
                    Labels = symbolsCollections[i].Labels,
                    Strings = symbolsCollections[i].Strings,
                    Instructions = CollectInstructions(symbolsCollections[i], compilationData, sources[i])
                });
            }

            var runtimeData = new MaciRuntimeData();
            runtimeData.AddCodeUnits(codeUnits);

            return runtimeData;
        }

        private static MaciSymbolsCollection CollectSymbols(MaciCompilationData compilationData, string source)
        {
            MaciFunctionLoader functionLoader = new();
            MaciStringLoader stringLoader = new();
            MaciLabelLoader labelLoader = new();

            List<MaciFunction> functions = [];
            List<MaciLabel> labels = [];
            List<string> strings = [];
            Dictionary<int, string> stringLines = [];

            try
            {
                // First pass: collect all labels and function addresses
                int instructionIndex = compilationData.InstructionIndex;
                int instructionCount = 0;
                var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (string.IsNullOrWhiteSpace(line) ||
                        line.StartsWith(';') ||
                        functionLoader.TryLoad(compilationData, line, instructionIndex, functions) ||
                        labelLoader.TryLoad(compilationData, line, instructionIndex, labels)
                        )
                    {
                        continue;
                    }

                    stringLoader.TryLoad(compilationData, i, line, strings, stringLines);

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

        private static MaciInstruction[] CollectInstructions(MaciSymbolsCollection symbolsCollection, MaciCompilationData compilationData, string source)
        {
            try
            {
                var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                List<MaciInstruction> instructions = [];

                // Second pass: parse instructions
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip labels, comments, and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.EndsWith(':'))
                        continue;

                    var parseInput = new MaciParseInput(
                        symbolsCollection.FunctionNameToIndex,
                        symbolsCollection.LabelNameToIndex,
                        symbolsCollection.StringToIndex,
                        symbolsCollection.StringLines,
                        line,
                        i);

                    // Parse instruction
                    var instruction = MaciScriptParser.ParseInstruction(parseInput);
                    instructions.Add(instruction);
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
