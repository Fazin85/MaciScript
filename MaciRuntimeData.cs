namespace MaciScript
{
    public struct MaciRuntimeData(int[] registers, int[] systemRegisters, byte[] memory, MaciLabel[] labels, MaciFunction[] functions, MaciCallStack callstack, MaciInstruction[] instructions)
    {
        public int[] Registers = registers;
        public int[] SystemRegisters = systemRegisters;
        public byte[] Memory = memory;
        public int ProgramCounter;
        public MaciLabel[] Labels = labels;
        public MaciFunction[] Functions = functions;
        public MaciCallStack CallStack = callstack;
        public MaciInstruction[] Instructions = instructions;

        public const uint MaxMem = uint.MaxValue;

        public MaciRuntimeData() : this(new int[16], new int[8], new byte[1024 * 1024 * 64], [], [], new(16384), [])
        {
        }
    }
}
