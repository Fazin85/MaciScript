namespace MaciScript
{
    public class MaciInstructionHandler
    {
        private readonly MaciRuntimeData runtimeData;
        private readonly Action<string> debugLog;

        public MaciInstructionHandler(MaciRuntimeData runtimeData, Action<string> debugLog)
        {
            this.runtimeData = runtimeData;
            this.debugLog = debugLog;
        }

        public void Handle(SysCallExecutor sysCallExecutor, MaciInstruction instruction)
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
                                debugLog($"ADD: R{destReg} = R{destReg}({runtimeData.Registers[destReg] + runtimeData.Registers[srcReg]}) - R{srcReg}({runtimeData.Registers[srcReg]}) = {runtimeData.Registers[destReg]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] += value;
                                debugLog($"ADD: R{destReg} = R{destReg}({runtimeData.Registers[destReg] + value}) - {value} = {runtimeData.Registers[destReg]}");
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
                                debugLog($"SUB: R{destReg} = R{destReg}({runtimeData.Registers[destReg] + runtimeData.Registers[srcReg]}) - R{srcReg}({runtimeData.Registers[srcReg]}) = {runtimeData.Registers[destReg]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[destReg] -= value;
                                debugLog($"SUB: R{destReg} = R{destReg}({runtimeData.Registers[destReg] + value}) - {value} = {runtimeData.Registers[destReg]}");
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
                                debugLog($"CMP R{reg1}({runtimeData.Registers[reg1]}) with R{reg2}({runtimeData.Registers[reg2]}) = {runtimeData.Registers[15]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                runtimeData.Registers[15] = runtimeData.Registers[reg1].CompareTo(value);
                                debugLog($"CMP R{reg1}({runtimeData.Registers[reg1]}) with {value} = {runtimeData.Registers[15]}");
                            }
                        }
                        break;

                    case MaciOpcode.Jmp:
                        {
                            // Get the label index from the operand
                            int labelIndex = instruction.Operands[0].Value;

                            // Make sure it's a valid index
                            if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Count)
                            {
                                throw new Exception($"Invalid label index: {labelIndex}");
                            }

                            // Get the target address from the label list
                            int address = runtimeData.Labels[labelIndex].Item2;
                            runtimeData.ProgramCounter = address - 1; // -1 because PC will be incremented after this
                            debugLog($"JMP to label '{runtimeData.Labels[labelIndex].Item1}' at position {address}");
                        }
                        break;

                    case MaciOpcode.Je:
                        {
                            // Jump if equal (R15 == 0)
                            if (runtimeData.Registers[15] == 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Item2;
                                runtimeData.ProgramCounter = address - 1;
                                debugLog($"JE to label '{runtimeData.Labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                debugLog("JE condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jne:
                        {
                            // Jump if not equal (R15 != 0)
                            if (runtimeData.Registers[15] != 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Item2;
                                runtimeData.ProgramCounter = address - 1;
                                debugLog($"JNE to label '{runtimeData.Labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                debugLog("JNE condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jg:
                        {
                            // Jump if greater (R15 > 0)
                            if (runtimeData.Registers[15] > 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Item2;
                                runtimeData.ProgramCounter = address - 1;
                                debugLog($"JG to label '{runtimeData.Labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                debugLog("JG condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jl:
                        {
                            // Jump if less (R15 < 0)
                            if (runtimeData.Registers[15] < 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= runtimeData.Labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = runtimeData.Labels[labelIndex].Item2;
                                runtimeData.ProgramCounter = address - 1;
                                debugLog($"JL to label '{runtimeData.Labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                debugLog("JL condition not met, continuing");
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
                            if (functionIndex < 0 || functionIndex >= runtimeData.Functions.Count)
                            {
                                throw new Exception($"Invalid function index: {functionIndex}");
                            }

                            // Get the function address and push current position to call stack
                            int address = runtimeData.Functions[functionIndex];

                            // Push current PC to call stack
                            runtimeData.CallStack.Push(runtimeData.ProgramCounter);
                            runtimeData.ProgramCounter = address - 1; // -1 because PC will be incremented after this
                            //debugLog($"Call to function '{_functionNames[functionIndex]}' at position {address}");
                        }
                        break;

                    case MaciOpcode.Ret:
                        {
                            if (runtimeData.CallStack.Count > 0)
                            {
                                runtimeData.ProgramCounter = runtimeData.CallStack.Pop();
                                debugLog($"Returning to position {runtimeData.ProgramCounter + 1}");
                            }
                            else
                            {
                                throw new Exception("Return without call");
                            }
                        }
                        break;

                    case MaciOpcode.Syscall:
                        {
                            int syscallNumber = runtimeData.SystemRegisters[0];
                            debugLog($"Executing syscall {syscallNumber}");

                            sysCallExecutor.Execute(runtimeData, syscallNumber);
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
