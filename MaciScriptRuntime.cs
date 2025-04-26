namespace MaciScript
{
    public static class MaciScriptRuntime
    {
        //TODO: convert some syscalls to functions for better performance

        public static void Execute(ref MaciRuntimeData runtimeData, SysCallExecutor sysCallExecutor)
        {
            runtimeData.ProgramCounter = 0;

            while (runtimeData.ProgramCounter < runtimeData.Instructions.Length)
            {
                MaciInstruction instruction = runtimeData.Instructions[runtimeData.ProgramCounter];
                MaciInstructionHandler.Handle(ref runtimeData, sysCallExecutor, instruction);
                runtimeData.ProgramCounter++;
            }
        }
    }
}