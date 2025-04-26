using System.Globalization;

namespace MaciScript
{
    public static class MaciScriptParser
    {
        private static bool CanSymbolCollectionBeUsed(MaciSymbolCollection current, MaciSymbolCollection other)
        {
            return current.Imports.Contains(other.FilePath) || other == current;
        }

        public static MaciInstruction ParseInstruction(MaciParseInput input)
        {
            try
            {
                MaciSymbolCollection currentSymbols = input.SymbolCollections[input.SymbolCollectionIndex];

                // Remove comments
                int commentIndex = input.Line.IndexOf(';');
                if (commentIndex >= 0)
                    input.Line = input.Line[..commentIndex].Trim();

                // Split into opcode and operands
                string[] parts = input.Line.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    throw new Exception("Empty instruction");

                string opcode = parts[0].ToLower();
                string[] operandStrings = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

                MaciOpcode parsedOpcode = ParseOpcode(opcode);

                bool isLdstrLine = currentSymbols.StringLines.ContainsKey(input.LineNumber);

                int operandCount = operandStrings.Length;
                operandCount = Math.Clamp(operandCount, 1, 2);

                // Create instruction with properly sized operands array
                MaciInstruction instruction = new()
                {
                    Opcode = parsedOpcode,
                    Operands = new MaciOperand[operandCount]
                };

                // Check if this is a control flow instruction
                bool isControlFlow = IsControlFlowInstruction(parsedOpcode);

                if (operandStrings.Length > 0)
                {
                    if (isControlFlow)
                    {
                        string targetName = operandStrings[0];

                        // For calls, look up in function dictionary
                        if (parsedOpcode == MaciOpcode.Call || IsControlFlowCall(parsedOpcode))
                        {
                            bool found = false;
                            foreach (var symbolCollection in input.SymbolCollections.Where(x => CanSymbolCollectionBeUsed(currentSymbols, x)))
                            {
                                if (symbolCollection.FunctionNameToIndex.TryGetValue(targetName, out int index))
                                {
                                    instruction.Operands[0].Value = index;
                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                throw new Exception($"Function not found: {targetName}");
                            }
                        }
                        // For jumps, look up in label dictionary
                        else
                        {
                            bool found = false;
                            foreach (var symbolCollection in input.SymbolCollections.Where(x => CanSymbolCollectionBeUsed(currentSymbols, x)))
                            {
                                if (symbolCollection.LabelNameToIndex.TryGetValue(targetName, out int index))
                                {
                                    instruction.Operands[0].Value = index;
                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                throw new Exception($"Label not found: {targetName}");
                            }
                        }
                    }
                    else if (parsedOpcode == MaciOpcode.Ldstr)
                    {
                        if (currentSymbols.StringLines.TryGetValue(input.LineNumber, out string? targetName))
                        {
                            targetName = Util.ExtractNestedQuotes(targetName) ?? throw new Exception("Failed to extract string from targetName");

                            if (currentSymbols.StringToIndex.TryGetValue(targetName, out int index))
                            {
                                if (!operandStrings[0].StartsWith("R", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new Exception("ldstr first operand must be a register");
                                }

                                instruction.Operands[0].IsReg = true;
                                instruction.Operands[0].Value = ParseRegister(operandStrings[0]);

                                instruction.Operands[1].Value = index;
                            }
                            else
                            {
                                throw new Exception($"String not found: {targetName}");
                            }
                        }
                        else
                        {
                            throw new Exception($"No string at line {input.LineNumber}");
                        }
                    }
                    else
                    {
                        instruction.Operands = ParseOperands(operandStrings);
                    }
                }

                return instruction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse instruction '{input.Line}': {ex.Message}");
            }
        }

        private static bool IsControlFlowInstruction(MaciOpcode opcode)
        {
            return opcode == MaciOpcode.Call ||
                   opcode == MaciOpcode.Jmp ||
                   opcode == MaciOpcode.Je ||
                   opcode == MaciOpcode.Jne ||
                   opcode == MaciOpcode.Jg ||
                   opcode == MaciOpcode.Jl ||
                   IsControlFlowCall(opcode);
        }

        private static bool IsControlFlowCall(MaciOpcode opcode)
        {
            return opcode == MaciOpcode.Jmpf ||
                   opcode == MaciOpcode.Jef ||
                   opcode == MaciOpcode.Jnef ||
                   opcode == MaciOpcode.Jgf ||
                   opcode == MaciOpcode.Jlf;
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

                    var immediate = ParseImmediate(operands[i]);

                    if (immediate.Type == ImmediateValue.ImmediateType.Float)
                    {
                        resultOperands[i].IsFloat = true;
                    }

                    resultOperands[i].Value = immediate.Value;
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

        private static ImmediateValue ParseImmediate(string value)
        {
            // Hex value
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return new()
                {
                    Type = ImmediateValue.ImmediateType.Int,
                    Value = Convert.ToInt32(value[2..], 16)
                };
            }

            // float value
            if (value.Contains('.'))
            {
                return new()
                {
                    Type = ImmediateValue.ImmediateType.Float,
                    Value = BitConverter.ToInt32(BitConverter.GetBytes(float.Parse(value, CultureInfo.InvariantCulture)))
                };
            }

            // Decimal value
            if (int.TryParse(value, out int result))
            {
                return new()
                {
                    Type = ImmediateValue.ImmediateType.Int,
                    Value = result
                };
            }

            throw new ArgumentException($"Invalid immediate value: {value}");
        }

        private struct ImmediateValue
        {
            public enum ImmediateType
            {
                Float,
                Int
            }

            public required ImmediateType Type;
            public required int Value;
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
