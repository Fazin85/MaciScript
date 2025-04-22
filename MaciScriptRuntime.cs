namespace MaciScript
{
    public class MaciScriptRuntime
    {
        // Debug mode flag
        private readonly bool _debugMode = false;

        // For mapping names to indices
        private readonly Dictionary<string, int> _labelNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _functionNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        // Parsed instructions ready for execution
        private MaciInstruction[] _instructions = [];

        private readonly MaciInstructionHandler instructionHandler;

        public MaciScriptRuntime(MaciInstructionHandler instructionHandler, bool debugMode = false)
        {
            _debugMode = debugMode;

            this.instructionHandler = instructionHandler;
        }

        // Helper method for debug logging
        private void DebugLog(string message)
        {
            if (_debugMode)
            {
                Console.WriteLine(message);
            }
        }

        public void LoadProgram(MaciRuntimeData runtimeData, string source)
        {
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
                    if (line.StartsWith("function") && line.EndsWith(":"))
                    {
                        // Extract just the function name between "function" and ":"
                        string funcName = line[8..^1].Trim();

                        // Store function info
                        runtimeData.Functions.Add(instructionIndex);
                        _functionNameToIndex[funcName] = runtimeData.Functions.Count - 1;

                        DebugLog($"Function found: {funcName} at position {instructionIndex}");
                        continue;
                    }

                    // Check for regular labels
                    if (line.EndsWith(":"))
                    {
                        string label = line[..^1].Trim();
                        foreach (var tuple in runtimeData.Labels)
                        {
                            if (label == tuple.Item1)
                            {
                                throw new Exception("Cannot have duplicate labels: " + label);
                            }
                        }

                        // Store label info
                        runtimeData.Labels.Add(new(label, instructionIndex));
                        _labelNameToIndex[label] = runtimeData.Labels.Count - 1;

                        DebugLog($"Label found: {label} at position {instructionIndex}");
                        continue;
                    }

                    // Actual instruction that will be executed
                    instructionIndex++;
                }

                if (_debugMode)
                {
                    Console.WriteLine("Label mappings:");
                    foreach (var kvp in _labelNameToIndex)
                    {
                        Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
                    }

                    Console.WriteLine("Function mappings:");
                    foreach (var kvp in _functionNameToIndex)
                    {
                        Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
                    }
                }

                DebugLog($"Found {runtimeData.Labels.Count} labels and {runtimeData.Functions.Count} functions");
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
                    var instruction = ParseInstruction(line);
                    instructions.Add(instruction);
                    DebugLog($"Added instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                }

                _instructions = [.. instructions];

                DebugLog($"Program loaded with {_instructions.Length} instructions");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading program: {ex.Message}");
            }
        }

        private MaciInstruction ParseInstruction(string line)
        {
            try
            {
                // Remove comments
                int commentIndex = line.IndexOf(';');
                if (commentIndex >= 0)
                    line = line[..commentIndex].Trim();

                // Split into opcode and operands
                string[] parts = line.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    throw new Exception("Empty instruction");

                string opcode = parts[0].ToLower();
                string[] operandStrings = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

                MaciOpcode parsedOpcode = ParseOpcode(opcode);

                // Create instruction with properly sized operands array
                MaciInstruction instruction = new()
                {
                    Opcode = parsedOpcode,
                    Operands = new MaciOperand[Math.Max(1, operandStrings.Length)]
                };

                // Check if this is a control flow instruction
                bool isControlFlow = IsControlFlowInstruction(parsedOpcode);

                if (isControlFlow && operandStrings.Length > 0)
                {
                    string targetName = operandStrings[0];

                    // For calls, look up in function dictionary
                    if (parsedOpcode == MaciOpcode.Call)
                    {
                        if (_functionNameToIndex.TryGetValue(targetName, out int index))
                        {
                            instruction.Operands[0].Value = index;
                            DebugLog($"Resolved function '{targetName}' to index {index}");
                        }
                        else
                        {
                            throw new Exception($"Function not found: {targetName}");
                        }
                    }
                    // For jumps, look up in label dictionary
                    else
                    {
                        if (_labelNameToIndex.TryGetValue(targetName, out int index))
                        {
                            instruction.Operands[0].Value = index;
                            DebugLog($"Resolved label '{targetName}' to index {index}");
                        }
                        else
                        {
                            throw new Exception($"Label not found: {targetName}");
                        }
                    }
                }
                else if (operandStrings.Length > 0)
                {
                    // For regular instructions, parse operands normally
                    instruction.Operands = ParseOperands(operandStrings);
                }

                return instruction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse instruction '{line}': {ex.Message}");
            }
        }

        private static bool IsControlFlowInstruction(MaciOpcode opcode)
        {
            return opcode == MaciOpcode.Call ||
                   opcode == MaciOpcode.Jmp ||
                   opcode == MaciOpcode.Je ||
                   opcode == MaciOpcode.Jne ||
                   opcode == MaciOpcode.Jg ||
                   opcode == MaciOpcode.Jl;
        }

        private static MaciOperand[] ParseOperands(string[] operands)
        {
            MaciOperand[] resultOperands = new MaciOperand[operands.Length];

            for (int i = 0; i < operands.Length; i++)
            {
                if (operands[i].StartsWith("R", StringComparison.OrdinalIgnoreCase))
                {
                    resultOperands[i].IsReg = true;
                    resultOperands[i].Value = ParseRegister(operands[i]);
                }
                else if (operands[i].StartsWith("S", StringComparison.OrdinalIgnoreCase))
                {
                    resultOperands[i].IsSysReg = true;
                    resultOperands[i].Value = ParseSyscallRegister(operands[i]);
                }
                else
                {
                    resultOperands[i].IsImmediate = true;
                    resultOperands[i].Value = ParseImmediate(operands[i]);
                }
            }

            return resultOperands;
        }

        public void Execute(MaciRuntimeData runtimeData, SysCallExecutor sysCallExecutor)
        {
            runtimeData.ProgramCounter = 0;

            while (runtimeData.ProgramCounter < _instructions.Length)
            {
                MaciInstruction instruction = _instructions[runtimeData.ProgramCounter];
                DebugLog($"Executing instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                instructionHandler.Handle(sysCallExecutor, instruction);
                runtimeData.ProgramCounter++;
            }
        }

        private static int ParseRegister(string reg)
        {
            if (!reg.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid register: {reg}");

            if (!int.TryParse(reg.AsSpan(1), out int regNum) || regNum < 0 || regNum > 15)
                throw new ArgumentException($"Invalid register number: {reg}");

            return regNum;
        }

        private static int ParseSyscallRegister(string reg)
        {
            if (!reg.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid syscall register: {reg}");

            if (!int.TryParse(reg.AsSpan(1), out int regNum) || regNum < 0 || regNum > 7)
                throw new ArgumentException($"Invalid syscall register number: {reg}");

            return regNum;
        }

        private static int ParseImmediate(string value)
        {
            // Hex value
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToInt32(value[2..], 16);
            }

            // Decimal value
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            throw new ArgumentException($"Invalid immediate value: {value}");
        }

        public static MaciOpcode ParseOpcode(string opcodeStr)
        {
            if (Enum.TryParse<MaciOpcode>(opcodeStr, ignoreCase: true, out var opcode))
            {
                return opcode;
            }

            throw new ArgumentException($"Invalid opcode: '{opcodeStr}'", nameof(opcodeStr));
        }
    }
}