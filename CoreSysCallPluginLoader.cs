namespace MaciScript
{
    public class CoreSysCallPluginLoader(IMaciMemoryAllocator memoryAllocator) : IMaciScriptSysCallPluginLoader
    {
        private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;
        private readonly MaciNamedScopeVariableAllocator namedScopeVariableAllocator = new();
        private readonly MaciStackVariableAllocator stackVariableAllocator = new();

        private class SysCallPrintInt : SysCall
        {
            public override int ID => 1;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Console.WriteLine($"{runtimeData.SystemRegisters[1]}");
            }
        }

        private class SysCallPrintString : SysCall
        {
            public override int ID => 2;

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

        private class SysCallExit : SysCall
        {
            public override int ID => 3;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Environment.Exit(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallPrintStringByIndex : SysCall
        {
            public override int ID => 4;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                Console.WriteLine(runtimeData.Strings[runtimeData.SystemRegisters[1]]);
            }
        }

        private class SysCallAlloc(IMaciMemoryAllocator memoryAllocator) : SysCall
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override int ID => 5;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                runtimeData.Registers[0] = memoryAllocator.Alloc(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallRealloc(IMaciMemoryAllocator memoryAllocator) : SysCall
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override int ID => 6;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                runtimeData.Registers[0] = memoryAllocator.Realloc(runtimeData.SystemRegisters[1], runtimeData.SystemRegisters[2]);
            }
        }

        private class SysCallFree(IMaciMemoryAllocator memoryAllocator) : SysCall
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override int ID => 7;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Free(runtimeData.SystemRegisters[1]);
            }
        }

        private class SysCallAllocNamedScope(MaciNamedScopeVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 8;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.AllocateScope(scopeName);
            }
        }

        private class SysCallAllocVariable(MaciNamedScopeVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 9;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[2]];

                variableAllocator.AllocateVariable(scopeName, variableName);
            }
        }

        private class SysCallSetScopedVariable(MaciNamedScopeVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 10;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[2]];
                int value = runtimeData.SystemRegisters[3];

                variableAllocator.SetVariable(scopeName, variableName, value);
            }
        }

        private class SysCallFreeNamedScope(MaciNamedScopeVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciNamedScopeVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 11;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string scopeName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.FreeScope(scopeName);
            }
        }

        private class SysCallPushScope(MaciStackVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 12;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                variableAllocator.PushScope();
            }
        }

        private class SysCallPushVar(MaciStackVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 13;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[1]];

                variableAllocator.AllocateVariable(variableName);
            }
        }

        private class SysCallSetVar(MaciStackVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 14;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string variableName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                int value = runtimeData.SystemRegisters[2];

                variableAllocator.SetVariable(variableName, value);
            }
        }

        private class SysCallPopScope(MaciStackVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 15;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                variableAllocator.PopScope();
            }
        }

        private class SysCallGetVar(MaciStackVariableAllocator variableAllocator) : SysCall
        {
            private readonly MaciStackVariableAllocator variableAllocator = variableAllocator;

            public override int ID => 16;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                string varName = runtimeData.Strings[runtimeData.SystemRegisters[1]];
                int register = runtimeData.SystemRegisters[2];
                runtimeData.Registers[register] = variableAllocator.GetVariable(varName);
                Console.WriteLine($"get var: {runtimeData.Registers[register]}");
            }
        }

        public SysCallPlugin Load()
        {
            List<SysCall> sysCalls =
            [
                new SysCallPrintInt(),
                new SysCallPrintString(),
                new SysCallExit(),
                new SysCallPrintStringByIndex(),
                new SysCallAlloc(memoryAllocator),
                new SysCallRealloc(memoryAllocator),
                new SysCallFree(memoryAllocator),
                new SysCallAllocNamedScope(namedScopeVariableAllocator),
                new SysCallAllocVariable(namedScopeVariableAllocator),
                new SysCallSetScopedVariable(namedScopeVariableAllocator),
                new SysCallFreeNamedScope(namedScopeVariableAllocator),
                new SysCallPushScope(stackVariableAllocator),
                new SysCallPushVar(stackVariableAllocator),
                new SysCallSetVar(stackVariableAllocator),
                new SysCallPopScope(stackVariableAllocator),
                new SysCallGetVar(stackVariableAllocator)
            ];

            return new SysCallPlugin(sysCalls, "core");
        }
    }
}
