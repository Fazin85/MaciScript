namespace MaciScript
{
    public struct MaciCodeUnit
    {
        public required MaciFunction[] Functions;
        public required MaciLabel[] Labels;
        public required MaciInstruction[] Instructions;
        public required string[] Strings;
    }
}