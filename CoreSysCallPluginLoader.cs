namespace MaciScript
{
    public class CoreSysCallPluginLoader : IMaciScriptSysCallPluginLoader
    {
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

        public SysCallPlugin Load()
        {
            List<SysCall> sysCalls = [
                new SysCallPrintInt(),
                new SysCallPrintString(),
                new SysCallExit(),
                new SysCallPrintStringByIndex()];

            return new SysCallPlugin(sysCalls, "core");
        }
    }
}
