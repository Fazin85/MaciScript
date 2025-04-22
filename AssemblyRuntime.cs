using System.Runtime.InteropServices;

namespace MaciScript
{
    /// <summary>
    /// The memory model and execution environment for our assembly language
    /// </summary>
    public class AssemblyRuntime
    {
        // Debug mode flag
        private readonly bool _debugMode = false;

        // Memory - 4GB max (32-bit addressable memory)
        private readonly byte[] _memory = new byte[1024 * 1024 * 64]; // Start with 64MB allocation for practicality
        private readonly int[] _registers = new int[16]; // 16 general-purpose registers (R0-R15)
        private readonly int[] _syscallRegisters = new int[8]; // 8 syscall registers (S0-S7)
        private int _programCounter = 0;
        private readonly List<Tuple<string, int>> _labels = [];
        private readonly List<int> _functions = [];
        // Add a list to store function names
        private readonly List<string> _functionNames = [];
        private readonly Stack<int> _callStack = new();
        private readonly Dictionary<int, Func<int[], int[], byte[], int>> _syscalls = [];

        // For mapping names to indices
        private readonly Dictionary<string, int> _labelNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _functionNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        // Parsed instructions ready for execution
        private Instruction[] _instructions = [];

        public AssemblyRuntime(bool debugMode = false)
        {
            _debugMode = debugMode;

            // Initialize registers to 0
            for (int i = 0; i < _registers.Length; i++)
                _registers[i] = 0;

            // Initialize syscall registers to 0
            for (int i = 0; i < _syscallRegisters.Length; i++)
                _syscallRegisters[i] = 0;

            InitializeSyscalls();
        }

        // Helper method for debug logging
        private void DebugLog(string message)
        {
            if (_debugMode)
            {
                Console.WriteLine(message);
            }
        }

        private void InitializeSyscalls()
        {
            // Syscall implementations remain unchanged
            // Syscall 1: Print integer in S1
            _syscalls[1] = (regs, sregs, mem) =>
            {
                Console.WriteLine($"{sregs[1]}");
                DebugLog($"PRINT INTEGER: {sregs[1]}");
                return 0;
            };

            // Syscall 2: Print string pointed to by S1
            _syscalls[2] = (regs, sregs, mem) =>
            {
                int address = sregs[1];
                string s = "";
                while (address < mem.Length && mem[address] != 0)
                {
                    s += (char)mem[address++];
                }
                Console.WriteLine(s);
                DebugLog($"PRINT STRING: {s}");
                return 0;
            };

            // Syscall 3: Read integer into S1
            _syscalls[3] = (regs, sregs, mem) =>
            {
                Console.Write("Enter an integer: ");
                if (int.TryParse(Console.ReadLine(), out int value))
                {
                    sregs[1] = value;
                    DebugLog($"Read value: {value}");
                }
                else
                {
                    DebugLog("Failed to read integer, defaulting to 0");
                    sregs[1] = 0;
                }
                return 0;
            };

            // Syscall 4: Exit program with status code in S1
            _syscalls[4] = (regs, sregs, mem) =>
            {
                DebugLog($"Program exiting with code: {sregs[1]}");
                Environment.Exit(sregs[1]);
                return 0;
            };

            // Syscall 5: Allocate memory block of size S1, returns address in S0
            _syscalls[5] = (regs, sregs, mem) =>
            {
                int size = sregs[1];
                // Simple memory allocation - start at 1MB to avoid low addresses
                int address = 1024 * 1024;

                // In a real implementation, you'd have a proper memory allocator
                // This is a very simple one that just returns a predetermined address
                sregs[0] = address;
                DebugLog($"MEMORY ALLOCATED: {size} bytes at address {address}");
                return 0;
            };

            // Syscall 6: Copy memory block from S1 to S2 of size S3
            _syscalls[6] = (regs, sregs, mem) =>
            {
                int sourceAddr = sregs[1];
                int destAddr = sregs[2];
                int size = sregs[3];

                if (sourceAddr < 0 || sourceAddr + size >= mem.Length ||
                    destAddr < 0 || destAddr + size >= mem.Length)
                {
                    DebugLog("MEMORY COPY ERROR: Invalid address range");
                    return -1;
                }

                for (int i = 0; i < size; i++)
                {
                    mem[destAddr + i] = mem[sourceAddr + i];
                }

                DebugLog($"MEMORY COPIED: {size} bytes from {sourceAddr} to {destAddr}");
                return 0;
            };

            // Syscall 7: Get random number between S1 and S2, result in S0
            _syscalls[7] = (regs, sregs, mem) =>
            {
                int min = sregs[1];
                int max = sregs[2];

                if (min > max)
                {
                    (max, min) = (min, max);
                }

                Random random = new();
                sregs[0] = random.Next(min, max + 1);
                DebugLog($"RANDOM NUMBER: {sregs[0]} (between {min} and {max})");
                return 0;
            };

            // Syscall 8: Get system time (seconds since epoch) in S0
            _syscalls[8] = (regs, sregs, mem) =>
            {
                DateTime epochStart = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan timeSpan = DateTime.UtcNow - epochStart;
                sregs[0] = (int)timeSpan.TotalSeconds;
                DebugLog($"SYSTEM TIME: {sregs[0]} seconds since epoch");
                return 0;
            };
        }

        public void LoadProgram(string source)
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
                        _functions.Add(instructionIndex);
                        _functionNames.Add(funcName);
                        _functionNameToIndex[funcName] = _functions.Count - 1;

                        DebugLog($"Function found: {funcName} at position {instructionIndex}");
                        continue;
                    }

                    // Check for regular labels
                    if (line.EndsWith(":"))
                    {
                        string label = line[..^1].Trim();
                        foreach (var tuple in _labels)
                        {
                            if (label == tuple.Item1)
                            {
                                throw new Exception("Cannot have duplicate labels: " + label);
                            }
                        }

                        // Store label info
                        _labels.Add(new(label, instructionIndex));
                        _labelNameToIndex[label] = _labels.Count - 1;

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

                DebugLog($"Found {_labels.Count} labels and {_functions.Count} functions");
                DebugLog("Parsing instructions...");

                List<Instruction> instructions = [];

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

        private Instruction ParseInstruction(string line)
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
                Instruction instruction = new()
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

        public void Execute()
        {
            _programCounter = 0;

            while (_programCounter < _instructions.Length)
            {
                Instruction instruction = _instructions[_programCounter];
                DebugLog($"Executing instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                ExecuteInstruction(instruction);
                _programCounter++;
            }
        }

        private void ExecuteInstruction(Instruction instruction)
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
                                    _registers[destReg] = _registers[srcReg];
                                }
                                else if (instruction.Operands[1].IsSysReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    _registers[destReg] = _syscallRegisters[srcReg];
                                }
                                else
                                {
                                    _registers[destReg] = instruction.Operands[1].Value;
                                }
                            }
                            else if (instruction.Operands[0].IsSysReg)
                            {
                                int destReg = instruction.Operands[0].Value;

                                // Check if source is a register, syscall register, or immediate value
                                if (instruction.Operands[1].IsReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    _syscallRegisters[destReg] = _registers[srcReg];
                                }
                                else if (instruction.Operands[1].IsSysReg)
                                {
                                    int srcReg = instruction.Operands[1].Value;
                                    _syscallRegisters[destReg] = _syscallRegisters[srcReg];
                                }
                                else
                                {
                                    _syscallRegisters[destReg] = instruction.Operands[1].Value;
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
                                _registers[destReg] += _registers[srcReg];
                                DebugLog($"ADD: R{destReg} = R{destReg}({_registers[destReg] + _registers[srcReg]}) - R{srcReg}({_registers[srcReg]}) = {_registers[destReg]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                _registers[destReg] += value;
                                DebugLog($"ADD: R{destReg} = R{destReg}({_registers[destReg] + value}) - {value} = {_registers[destReg]}");
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
                                _registers[destReg] -= _registers[srcReg];
                                DebugLog($"SUB: R{destReg} = R{destReg}({_registers[destReg] + _registers[srcReg]}) - R{srcReg}({_registers[srcReg]}) = {_registers[destReg]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                _registers[destReg] -= value;
                                DebugLog($"SUB: R{destReg} = R{destReg}({_registers[destReg] + value}) - {value} = {_registers[destReg]}");
                            }
                        }
                        break;

                    case MaciOpcode.Mul:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            _registers[destReg] *= _registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Div:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            if (_registers[srcReg] == 0)
                                throw new DivideByZeroException("Division by zero");
                            _registers[destReg] /= _registers[srcReg];
                        }
                        break;

                    case MaciOpcode.And:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            _registers[destReg] &= _registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Or:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            _registers[destReg] |= _registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Xor:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int srcReg = instruction.Operands[1].Value;
                            _registers[destReg] ^= _registers[srcReg];
                        }
                        break;

                    case MaciOpcode.Shl:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int shiftAmount = instruction.Operands[1].Value;
                            _registers[destReg] <<= shiftAmount;
                        }
                        break;

                    case MaciOpcode.Shr:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int shiftAmount = instruction.Operands[1].Value;
                            _registers[destReg] >>= shiftAmount;
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
                                _registers[15] = _registers[reg1].CompareTo(_registers[reg2]);
                                DebugLog($"CMP R{reg1}({_registers[reg1]}) with R{reg2}({_registers[reg2]}) = {_registers[15]}");
                            }
                            else
                            {
                                int value = instruction.Operands[1].Value;
                                _registers[15] = _registers[reg1].CompareTo(value);
                                DebugLog($"CMP R{reg1}({_registers[reg1]}) with {value} = {_registers[15]}");
                            }
                        }
                        break;

                    case MaciOpcode.Jmp:
                        {
                            // Get the label index from the operand
                            int labelIndex = instruction.Operands[0].Value;

                            // Make sure it's a valid index
                            if (labelIndex < 0 || labelIndex >= _labels.Count)
                            {
                                throw new Exception($"Invalid label index: {labelIndex}");
                            }

                            // Get the target address from the label list
                            int address = _labels[labelIndex].Item2;
                            _programCounter = address - 1; // -1 because PC will be incremented after this
                            DebugLog($"JMP to label '{_labels[labelIndex].Item1}' at position {address}");
                        }
                        break;

                    case MaciOpcode.Je:
                        {
                            // Jump if equal (R15 == 0)
                            if (_registers[15] == 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= _labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = _labels[labelIndex].Item2;
                                _programCounter = address - 1;
                                DebugLog($"JE to label '{_labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                DebugLog("JE condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jne:
                        {
                            // Jump if not equal (R15 != 0)
                            if (_registers[15] != 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= _labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = _labels[labelIndex].Item2;
                                _programCounter = address - 1;
                                DebugLog($"JNE to label '{_labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                DebugLog("JNE condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jg:
                        {
                            // Jump if greater (R15 > 0)
                            if (_registers[15] > 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= _labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = _labels[labelIndex].Item2;
                                _programCounter = address - 1;
                                DebugLog($"JG to label '{_labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                DebugLog("JG condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Jl:
                        {
                            // Jump if less (R15 < 0)
                            if (_registers[15] < 0)
                            {
                                int labelIndex = instruction.Operands[0].Value;
                                if (labelIndex < 0 || labelIndex >= _labels.Count)
                                {
                                    throw new Exception($"Invalid label index: {labelIndex}");
                                }

                                int address = _labels[labelIndex].Item2;
                                _programCounter = address - 1;
                                DebugLog($"JL to label '{_labels[labelIndex].Item1}' at position {address}");
                            }
                            else
                            {
                                DebugLog("JL condition not met, continuing");
                            }
                        }
                        break;

                    case MaciOpcode.Load:
                        {
                            int destReg = instruction.Operands[0].Value;
                            int addressReg = instruction.Operands[1].Value;

                            // Load a 4-byte (32-bit) integer from memory
                            int memAddress = _registers[addressReg];
                            if (memAddress < 0 || memAddress + 3 >= _memory.Length)
                                throw new IndexOutOfRangeException("Memory access out of bounds");

                            _registers[destReg] = BitConverter.ToInt32(_memory, memAddress);
                        }
                        break;

                    case MaciOpcode.Store:
                        {
                            int srcReg = instruction.Operands[0].Value;
                            int addressReg = instruction.Operands[1].Value;

                            // Store a 4-byte (32-bit) integer to memory
                            int memAddress = _registers[addressReg];
                            if (memAddress < 0 || memAddress + 3 >= _memory.Length)
                                throw new IndexOutOfRangeException("Memory access out of bounds");

                            int valueToStore = _registers[srcReg];

                            Buffer.BlockCopy(new[] { valueToStore }, 0, _memory, memAddress, sizeof(int)
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
                            if (functionIndex < 0 || functionIndex >= _functions.Count)
                            {
                                throw new Exception($"Invalid function index: {functionIndex}");
                            }

                            // Get the function address and push current position to call stack
                            int address = _functions[functionIndex];

                            // Push current PC to call stack
                            _callStack.Push(_programCounter);
                            _programCounter = address - 1; // -1 because PC will be incremented after this
                            DebugLog($"Call to function '{_functionNames[functionIndex]}' at position {address}");
                        }
                        break;

                    case MaciOpcode.Ret:
                        {
                            if (_callStack.Count > 0)
                            {
                                _programCounter = _callStack.Pop();
                                DebugLog($"Returning to position {_programCounter + 1}");
                            }
                            else
                            {
                                throw new Exception("Return without call");
                            }
                        }
                        break;

                    case MaciOpcode.Syscall:
                        {
                            int syscallNumber = _syscallRegisters[0];
                            DebugLog($"Executing syscall {syscallNumber}");

                            if (_syscalls.TryGetValue(syscallNumber, out var handler))
                            {
                                handler(_registers, _syscallRegisters, _memory);
                            }
                            else
                            {
                                if (_debugMode)
                                {
                                    // Debug print all registers to help diagnosis
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Console.WriteLine($"R{i} = {_registers[i]}");
                                    }
                                    for (int i = 0; i < 5; i++)
                                    {
                                        Console.WriteLine($"S{i} = {_syscallRegisters[i]}");
                                    }
                                }
                                throw new Exception($"Unknown syscall: {syscallNumber}");
                            }
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

    public struct Instruction
    {
#nullable disable
        public MaciOpcode Opcode { get; set; }
        public MaciOperand[] Operands { get; set; }
#nullable enable
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaciOperand
    {
        [FieldOffset(0)]
        private byte _flags;

        public bool IsReg
        {
            readonly get => (_flags & 0b0001) != 0;
            set => _flags = value ? (byte)(_flags | 0b0001) : (byte)(_flags & ~0b0001);
        }

        public bool IsSysReg
        {
            readonly get => (_flags & 0b0010) != 0;
            set => _flags = value ? (byte)(_flags | 0b0010) : (byte)(_flags & ~0b0010);
        }

        public bool IsImmediate
        {
            readonly get => (_flags & 0b0100) != 0;
            set => _flags = value ? (byte)(_flags | 0b0100) : (byte)(_flags & ~0b0100);
        }

        [FieldOffset(4)]
        public int Value;
    }
}