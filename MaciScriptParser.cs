using System.Collections.Frozen;

namespace MaciScript
{
    public static class MaciScriptParser
    {
        public static MaciInstruction ParseInstruction(
    FrozenDictionary<string, int> functionNameToIndex,
    FrozenDictionary<string, int> labelNameToIndex,
    string line)
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
                        if (functionNameToIndex.TryGetValue(targetName, out int index))
                        {
                            instruction.Operands[0].Value = index;
                        }
                        else
                        {
                            throw new Exception($"Function not found: {targetName}");
                        }
                    }
                    // For jumps, look up in label dictionary
                    else
                    {
                        if (labelNameToIndex.TryGetValue(targetName, out int index))
                        {
                            instruction.Operands[0].Value = index;
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
                    instruction.Operands = ParseOperands(parsedOpcode, operandStrings, functionNameToIndex, labelNameToIndex);
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

        private static MaciOperand[] ParseOperands(
            MaciOpcode opcode,
            string[] operands,
            FrozenDictionary<string, int> functionNameToIndex,
            FrozenDictionary<string, int> labelNameToIndex)
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
                else if ((opcode == MaciOpcode.Fidx || opcode == MaciOpcode.Lidx) && i == 1)
                {
                    resultOperands[i].Value = opcode == MaciOpcode.Fidx ? functionNameToIndex[operands[i]] : labelNameToIndex[operands[i]];
                }
                else
                {
                    resultOperands[i].IsImmediate = true;
                    resultOperands[i].Value = ParseImmediate(operands[i]);
                }
            }

            return resultOperands;
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
