namespace MaciScript
{
    //TODO: fix SysCallPrintLabel always printing first label
    //TODO: stop functions from being excecuted in order regardless of whether or not they are called

    public static class MaciScriptRuntime
    {
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