namespace MaciScript
{
    public class MaciRuntimeData
    {
        public int[] Registers;
        public int[] SystemRegisters;
        public byte[] Memory;
        public int ProgramCounter;
        public List<Tuple<string, int>> Labels;
        public List<int> Functions;
        public Stack<int> CallStack;

        public const uint MaxMem = uint.MaxValue;

        public MaciRuntimeData() : this(new int[16], new int[8], new byte[1024 * 1024 * 64], [], [], [])
        {
        }

        public MaciRuntimeData(int[] registers, int[] systemRegisters, byte[] memory, List<Tuple<string, int>> labels, List<int> functions, Stack<int> callstack)
        {
            Registers = registers;
            SystemRegisters = systemRegisters;
            Memory = memory;
            Labels = labels;
            Functions = functions;
            CallStack = callstack;
        }
    }
}
