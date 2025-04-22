namespace MaciScript
{
    //TODO: fix SysCallPrintLabel always printing first label
    //TODO: stop functions from being excecuted in order regardless of whether or not they are called

    public class MaciScriptRuntime
    {
        // Debug mode flag
        private readonly bool _debugMode = false;

        public MaciScriptRuntime(bool debugMode = false)
        {
            _debugMode = debugMode;
        }

        // Helper method for debug logging
        private void DebugLog(string message)
        {
            if (_debugMode)
            {
                Console.WriteLine(message);
            }
        }

        public void LoadProgram(ref MaciRuntimeData runtimeData, string source)
        {
            MaciFunctionLoader functionLoader = new();
            MaciLabelLoader labelLoader = new();

            List<MaciFunction> functions = [];
            List<MaciLabel> labels = [];

            try
            {
                // First pass: collect all labels and function addresses
                int instructionIndex = 0;
                var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                DebugLog($"Parsing program with {lines.Length} lines...");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                        continue;

                    // Check for function declarations
                    if (functionLoader.TryLoad(line, instructionIndex, functions))
                    {
                        DebugLog($"Function found: {functions[^1].Name} at position {instructionIndex}");
                        continue;
                    }

                    // Check for regular labels
                    if (labelLoader.TryLoad(line, instructionIndex, labels))
                    {
                        DebugLog($"Label found: {labels[^1].Name} at position {instructionIndex}");
                        continue;
                    }

                    // Actual instruction that will be executed
                    instructionIndex++;
                }

                if (_debugMode)
                {
                    Console.WriteLine("Label mappings:");
                    foreach (var kvp in labelLoader.LabelNameToIndex)
                    {
                        Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
                    }

                    Console.WriteLine("Function mappings:");
                    foreach (var kvp in functionLoader.FunctionNameToIndex)
                    {
                        Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
                    }
                }

                runtimeData.Functions = [.. functions];
                runtimeData.Labels = [.. labels];

                DebugLog($"Found {runtimeData.Labels.Length} labels and {runtimeData.Functions.Length} functions");
                DebugLog("Parsing instructions...");

                List<MaciInstruction> instructions = [];

                // Second pass: parse instructions
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip labels, comments, and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.EndsWith(":"))
                        continue;

                    // Parse instruction
                    var instruction = MaciScriptParser.ParseInstruction(functionLoader.FunctionNameToIndex, labelLoader.LabelNameToIndex, line);
                    instructions.Add(instruction);
                    DebugLog($"Added instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                }

                runtimeData.Instructions = [.. instructions];

                DebugLog($"Program loaded with {runtimeData.Instructions.Length} instructions");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading program: {ex.Message}");
            }
        }

        public void Execute(ref MaciRuntimeData runtimeData, MaciInstructionHandler instructionHandler, SysCallExecutor sysCallExecutor)
        {
            runtimeData.ProgramCounter = 0;

            while (runtimeData.ProgramCounter < runtimeData.Instructions.Length)
            {
                MaciInstruction instruction = runtimeData.Instructions[runtimeData.ProgramCounter];
                DebugLog($"Executing instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                instructionHandler.Handle(ref runtimeData, sysCallExecutor, instruction);
                runtimeData.ProgramCounter++;
            }
        }
    }
}