namespace MaciScript
{
    public struct MaciInstruction
    {
#nullable disable
        public MaciOpcode Opcode { get; set; }
        public MaciOperand[] Operands { get; set; }
#nullable enable
    }
}
