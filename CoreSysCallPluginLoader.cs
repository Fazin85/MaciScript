namespace MaciScript
{
    public class CoreSysCallPluginLoader(IMaciMemoryAllocator memoryAllocator) : IMaciScriptSysCallPluginLoader
    {
        private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

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
                Console.WriteLine("alloc");
            }
        }

        private class SysCallRealloc(IMaciMemoryAllocator memoryAllocator) : SysCall
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override int ID => 6;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                runtimeData.Registers[0] = memoryAllocator.Realloc(runtimeData.SystemRegisters[1], runtimeData.SystemRegisters[2]);
                Console.WriteLine("realloc");
            }
        }

        private class SysCallFree(IMaciMemoryAllocator memoryAllocator) : SysCall
        {
            private readonly IMaciMemoryAllocator memoryAllocator = memoryAllocator;

            public override int ID => 7;

            public override void Call(ref MaciRuntimeData runtimeData)
            {
                memoryAllocator.Free(runtimeData.SystemRegisters[1]);
                Console.WriteLine("free");
            }
        }

        public SysCallPlugin Load()
        {
            List<SysCall> sysCalls = [
                new SysCallPrintInt(),
                new SysCallPrintString(),
                new SysCallExit(),
                new SysCallPrintStringByIndex(),
                new SysCallAlloc(memoryAllocator),
                new SysCallRealloc(memoryAllocator),
                new SysCallFree(memoryAllocator)];

            return new SysCallPlugin(sysCalls, "core");
        }
    }
}
