namespace MaciScript
{
    public static class MaciInstructionHandler
    {
        public static void Handle(ref MaciRuntimeData runtimeData, SysCallExecutor sysCallExecutor, MaciInstruction instruction)
        {
            try
            {
                switch (instruction.Opcode)
                {
                    case MaciOpcode.Mov:
                        {
                            // Check if destination is a regular register or syscall register
                            if (instruction.Operands[0].IsReg)
                            {
                                int destReg = instruction.Operands[0].Value;

                                // Check if source is a register, syscall register, or immediate value
                                if (instruction.Operands[1].IsReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    runtimeData.Registers[destReg] = runtimeData.Registers[srcReg];
                                }
                                else if (instruction.Operands[1].IsSysReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    runtimeData.Registers[destReg] = runtimeData.SystemRegisters[srcReg];
                                }
                                else
                                {
                                    runtimeData.Registers[destReg] = instruction.Operands[1].Value;
                                }
                            }
                            else if (instruction.Operands[0].IsSysReg)
                            {
                                int destReg = instruction.Operands[0].Value;

                                // Check if source is a register, syscall register, or immediate value
                                if (instruction.Operands[1].IsReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    runtimeData.SystemRegisters[destReg] = runtimeData.Registers[srcReg];
                                }
                                else if (instruction.Operands[1].IsSysReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    runtimeData.SystemRegisters[destReg] = runtimeData.SystemRegisters[srcReg];
                                }
                                else
                                {
                                    runtimeData.SystemRegisters[destReg] = instruction.Operands[1].Value;
                                }
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid register type: {instruction.Operands[0]}");
                            }
                        }
                        break;

                    case MaciOpcode.Add:
                        {
                            int destReg = instruction.Operands[0].Value;

                            // Check if source is a register or immediate value
                            if (instruction.Operands[1].IsReg)
                            {
                                int srcReg = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] += runtimeData.Registers[srcReg];
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] += value;
                            }
                        }
                        break;

                    case MaciOpcode.Sub:
                        {
                            int destReg = instruction.Operands[0].Value;

                            // Check if source is a register or immediate value
                            if (instruction.Operands[1].IsReg)
                            {
                                int srcReg = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] -= runtimeData.Registers[srcReg];
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] -= value;
                            }
                        }
                        break;

                    case MaciOpcode.Mul:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] *= runtimeData.Registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Div:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            if (runtimeData.Registers[srcReg] == 0)
                                throw new DivideByZeroException("Division by zero");
                            runtimeData.Registers[destReg] /= runtimeData.Registers[srcReg];
                        }
                        break;

                    case MaciOpcode.And:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] &= runtimeData.Registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Or:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] |= runtimeData.Registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Xor:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] ^= runtimeData.Registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Shl:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int shiftAmount = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] <<= shiftAmount;
                        }
                        break;

                    case MaciOpcode.Shr:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int shiftAmount = instruction.Operands[1].Value;
                            runtimeData.Registers[destReg] >>= shiftAmount;
                        }
                        break;

                    case MaciOpcode.Cmp:
                        {
                            int reg1 = instruction.Operands[0].Value;

                            // Special register R15 used for comparison result
                            // 0 = equal, 1 = greater, -1 = less
                            if (instruction.Operands[1].IsReg)
                            {
                                int reg2 = instruction.Operands[1].Value;
                                runtimeData.Registers[15] = runtimeData.Registers[reg1].CompareTo(runtimeData.Registers[reg2]);
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[15] = runtimeData.Registers[reg1].CompareTo(value);
                            }
                        }
                        break;

                    case MaciOpcode.Jmp:
                        {
                            // Get the label index from the operand
                            int labelIndex = instruction.Operands[0].Value;

                            // Make sure it's a valid index
                            if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Length)
                            {
                                throw new Exception($"Invalid label index: {labelIndex}");
                            }

                            // Get the target address from the label list
                            int address = runtimeData.Labels[labelIndex].Address;
                            runtimeData.ProgramCounter = address - 1; // -1 because PC will be incremented after this
                        }
                        break;

                    case MaciOpcode.Je:
                        {
                            // Jump if equal (R15 == 0)
                            if (runtimeData.Registers[15] == 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Length)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Address;
                                runtimeData.ProgramCounter = address - 1;
                            }
                        }
                        break;

                    case MaciOpcode.Jne:
                        {
                            // Jump if not equal (R15 != 0)
                            if (runtimeData.Registers[15] != 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Length)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Address;
                                runtimeData.ProgramCounter = address - 1;
                            }
                        }
                        break;

                    case MaciOpcode.Jg:
                        {
                            // Jump if greater (R15 > 0)
                            if (runtimeData.Registers[15] > 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Length)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Address;
                                runtimeData.ProgramCounter = address - 1;
                            }
                        }
                        break;

                    case MaciOpcode.Jl:
                        {
                            // Jump if less (R15 < 0)
                            if (runtimeData.Registers[15] < 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Length)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Address;
                                runtimeData.ProgramCounter = address - 1;
                            }
                        }
                        break;

                    case MaciOpcode.Load:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int addressReg = instruction.Operands[1].Value;

                            // Load a 4-byte (32-bit) integer from memory
                            int memAddress = runtimeData.Registers[addressReg];
                            if (memAddress < 0 || memAddress + 3 >= runtimeData.Memory.Length)
                                throw new IndexOutOfRangeException("Memory access out of bounds");

                            runtimeData.Registers[destReg] = BitConverter.ToInt32(runtimeData.Memory, memAddress);
                        }
                        break;

                    case MaciOpcode.Store:
                        {
                            int srcReg = instruction.Operands[0].Value;
                            int addressReg = instruction.Operands[1].Value;

                            // Store a 4-byte (32-bit) integer to memory
                            int memAddress = runtimeData.Registers[addressReg];
                            if (memAddress < 0 || memAddress + 3 >= runtimeData.Memory.Length)
                                throw new IndexOutOfRangeException("Memory access out of bounds");

                            int valueToStore = runtimeData.Registers[srcReg];

                            Buffer.BlockCopy(new[] { valueToStore }, 0, runtimeData.Memory, memAddress, sizeof(int)
                            );
                        }
                        break;

                    case MaciOpcode.Call:
                        {
                            // Check that we have an operand
                            if (instruction.Operands == null || instruction.Operands.Length == 0)
                            {
                                throw new Exception("Call instruction requires an operand");
                            }

                            // Get the function index from the operand
                            int functionIndex = instruction.Operands[0].Value;

                            // Make sure it's a valid index
                            if (functionIndex < 0 || functionIndex >= runtimeData.Functions.Length)
                            {
                                throw new Exception($"Invalid function index: {functionIndex}");
                            }

                            // Get the function address and push current position to call stack
                            int address = runtimeData.Functions[functionIndex].Address;

                            // Push current PC to call stack
                            runtimeData.CallStack.Push(runtimeData.ProgramCounter);
                            runtimeData.ProgramCounter = address - 1; // -1 because PC will be incremented after this
                        }
                        break;

                    case MaciOpcode.Ret:
                        {
                            runtimeData.ProgramCounter = runtimeData.CallStack.Pop();
                        }
                        break;

                    case MaciOpcode.Syscall:
                        {
                            int syscallNumber = runtimeData.SystemRegisters[0];

                            sysCallExecutor.Execute(ref runtimeData, syscallNumber);
                        }
                        break;

                    case MaciOpcode.Ldstr:
                        {
                            runtimeData.Registers[instruction.Operands[0].Value] = instruction.Operands[1].Value;
                        }
                        break;

                    default:
                        throw new Exception($"Unknown opcode: {instruction.Opcode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing instruction '{instruction.Opcode}': {ex.Message}");
            }
        }
    }
}
