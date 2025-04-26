namespace MaciScript
{
    public class CoreSysCallPluginLoader() : IMaciScriptSysCallPluginLoader
    {
        private readonly IMaciMemoryAllocator intAllocator = new MaciMemoryAllocator<int>();
        private readonly IMaciMemoryAllocator floatAllocator = new MaciMemoryAllocator<float>();
        private readonly MaciNamedScopeVariableAllocator namedScopeVariableAllocator = new();
        private readonly MaciStackVariableAllocator stackVariableAllocator = new();

        private class UIDSysCall(string name) : SysCall(name)
        {
            private static int nextId = 2;
            private readonly int id = nextId++;

            public override int ID => id;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
            }
        }

        private class SysCallIDOfSysCall() : SysCall("GET_SYSCALL_ID")
        {
            public override int ID => 1;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string id = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                runtimeData.Registers[0] = syscalls[id].ID;
            }
        }

        private class SysCallPrintInt() : UIDSysCall("PRINT_INT")
        {
            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Console.WriteLine(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallPrintString() : UIDSysCall("PRINT_STRING_NT")
        {
            public override void Call(ref MaciRuntimeData runtimeData)
            {
                int address = runtimeData.SystemRegisters[1];
                string s = "";
                while (address < runtimeData.Memory.Length && runtimeData.Memory[address] != 0)
                {
                    s += (char)runtimeData.Memory[address++];
                }
                Console.WriteLine(s);
            }
        }

        private class SysCallExit() : UIDSysCall("EXIT")
        {
            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Environment.Exit(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallPrintStringByIndex() : UIDSysCall("PRINT_STRING")
        {
            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Console.WriteLine(runtimeData.Strings[runtimeData.SystemRegisters[1]]);
            }
        }

        private class SysCallAllocNamedScope(MaciNamedScopeVariableAllocator variableAllocator) : UIDSysCall("ALLOC_NAMED_SCOPE")
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.AllocateScope(scopeName);
            }
        }

        private class SysCallAllocVariable(MaciNamedScopeVariableAllocator variableAllocator) : UIDSysCall("ALLOC_VARIABLE")
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[2]];

                variableAllocator.AllocateVariable(scopeName, variableName);
            }
        }

        private class SysCallSetScopedVariable(MaciNamedScopeVariableAllocator variableAllocator) : UIDSysCall("SET_SCOPED_VARIABLE")
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[2]];
                int value = runtimeData.SystemRegisters[3];

                variableAllocator.SetVariable(scopeName, variableName, value);
            }
        }

        private class SysCallFreeNamedScope(MaciNamedScopeVariableAllocator variableAllocator) : UIDSysCall("FREE_NAMED_SCOPE")
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.FreeScope(scopeName);
            }
        }

        private class SysCallPushScope(MaciStackVariableAllocator variableAllocator) : UIDSysCall("PUSH_SCOPE")
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                variableAllocator.PushScope();
            }
        }

        private class SysCallPushVar(MaciStackVariableAllocator variableAllocator) : UIDSysCall("PUSH_VARIABLE")
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.AllocateVariable(variableName);
            }
        }

        private class SysCallSetVar(MaciStackVariableAllocator variableAllocator) : UIDSysCall("SET_VARIABLE")
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                int value = runtimeData.SystemRegisters[2];

                variableAllocator.SetVariable(variableName, value);
            }
        }

        private class SysCallPopScope(MaciStackVariableAllocator variableAllocator) : UIDSysCall("POP_SCOPE")
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                variableAllocator.PopScope();
            }
        }

        private class SysCallGetVar(MaciStackVariableAllocator variableAllocator) : UIDSysCall("GET_VARIABLE")
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string varName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                int register = runtimeData.SystemRegisters[2];
                runtimeData.Registers[register] = variableAllocator.GetVariable(varName);
            }
        }

        private class SysCallPrintFloat() : UIDSysCall("PRINT_FLOAT")
        {
            public override void Call(ref MaciRuntimeData runtimeData)
            {
                float value = BitConverter.ToSingle(BitConverter.GetBytes(runtimeData.SystemRegisters[1]));
                Console.WriteLine(value);
            }
        }

        private class SysCallAllocIntBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("ALLOC_INT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                runtimeData.Registers[0] = memoryAllocator.Alloc(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallReallocIntBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("REALLOC_INT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Realloc(runtimeData.SystemRegisters[1], runtimeData.SystemRegisters[2]);
            }
        }

        private class SysCallFreeIntBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("FREE_INT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Free(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallAllocFloatBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("ALLOC_FLOAT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                runtimeData.Registers[0] = memoryAllocator.Alloc(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallReallocFloatBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("REALLOC_FLOAT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Realloc(runtimeData.SystemRegisters[1], runtimeData.SystemRegisters[2]);
            }
        }

        private class SysCallFreeFloatBuffer(IMaciMemoryAllocator memoryAllocator) : UIDSysCall("FREE_FLOAT_BUFFER")
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Free(runtimeData.SystemRegisters[1]);
            }
        }

        public SysCallPlugin Load()
        {
            List<SysCall> sysCalls =
            [
                new SysCallIDOfSysCall(),
                new SysCallPrintInt(),
                new SysCallPrintString(),
                new SysCallExit(),
                new SysCallPrintStringByIndex(),
                new SysCallAllocNamedScope(namedScopeVariableAllocator),
                new SysCallAllocVariable(namedScopeVariableAllocator),
                new SysCallSetScopedVariable(namedScopeVariableAllocator),
                new SysCallFreeNamedScope(namedScopeVariableAllocator),
                new SysCallPushScope(stackVariableAllocator),
                new SysCallPushVar(stackVariableAllocator),
                new SysCallSetVar(stackVariableAllocator),
                new SysCallPopScope(stackVariableAllocator),
                new SysCallGetVar(stackVariableAllocator),
                new SysCallPrintFloat(),
                new SysCallAllocIntBuffer(intAllocator),
                new SysCallReallocIntBuffer(intAllocator),
                new SysCallFreeIntBuffer(intAllocator),
                new SysCallAllocFloatBuffer(floatAllocator),
                new SysCallReallocFloatBuffer(floatAllocator),
                new SysCallFreeFloatBuffer(floatAllocator)
            ];

            return new SysCallPlugin(sysCalls, "core");
        }
    }
}
