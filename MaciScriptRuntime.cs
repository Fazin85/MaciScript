using System.Diagnostics;

namespace MaciScript
{
    //TODO: fix SysCallPrintLabel always printing first label
    //TODO: stop functions from being excecuted in order regardless of whether or not they are called

    public class MaciScriptRuntime
    {
        // Debug mode flag
        private readonly bool _debugMode = false;

        public MaciScriptRuntime(bool debugMode = false)
        {
            _debugMode = debugMode;
        }

        // Helper method for debug logging
        private void DebugLog(string message)
        {
            if (_debugMode)
            {
                Console.WriteLine(message);
            }
        }

        public void Execute(ref MaciRuntimeData runtimeData, MaciInstructionHandler instructionHandler, SysCallExecutor sysCallExecutor)
        {
            runtimeData.ProgramCounter = 0;

            while (runtimeData.ProgramCounter < runtimeData.Instructions.Length)
            {
                MaciInstruction instruction = runtimeData.Instructions[runtimeData.ProgramCounter];
                DebugLog($"Executing instruction: {instruction.Opcode} with {instruction.Operands?.Length ?? 0} operands");
                instructionHandler.Handle(ref runtimeData, sysCallExecutor, instruction);
                runtimeData.ProgramCounter++;
            }
        }
    }
}