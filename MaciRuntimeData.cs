using System.Diagnostics;

namespace MaciScript
{
    public struct MaciRuntimeData(
        int[] registers,
        int[] systemRegisters,
        byte[] memory,
        MaciLabel[] labels,
        MaciFunction[] functions,
        MaciCallStack callstack,
        MaciInstruction[] instructions,
        string[] strings)
    {
        public int[] Registers = registers;
        public int[] SystemRegisters = systemRegisters;
        public byte[] Memory = memory;
        public int ProgramCounter;
        public MaciLabel[] Labels = labels;
        public MaciFunction[] Functions = functions;
        public MaciCallStack CallStack = callstack;
        public MaciInstruction[] Instructions = instructions;
        public string[] Strings = strings;

        public const uint MaxMem = uint.MaxValue;

        public MaciRuntimeData() : this(new int[16], new int[8], new byte[1024 * 1024 * 64], [], [], new(16384), [], [])
        {
        }

        public void AddCodeUnits(List<MaciCodeUnit> codeUnits)
        {
            Debug.Assert(codeUnits.Count > 0);

            List<MaciLabel> labels = [];
            List<MaciFunction> functions = [];
            List<MaciInstruction> instructions = [];
            List<string> strings = [];

            foreach (MaciCodeUnit codeUnit in codeUnits)
            {
                labels.AddRange(codeUnit.Labels);
                functions.AddRange(codeUnit.Functions);
                instructions.AddRange(codeUnit.Instructions);
                strings.AddRange(codeUnit.Strings);
            }

            Labels = [.. labels];
            Functions = [.. functions];
            Instructions = [.. instructions];
            Strings = [.. strings];
        }
    }
}
