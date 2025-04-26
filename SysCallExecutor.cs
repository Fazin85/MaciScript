namespace MaciScript
{
    public class SysCallExecutor
    {
        private readonly Dictionary<string, SysCall> _systemCalls;

        public SysCallExecutor(ISysCallLoader sysCallLoader)
        {
            _systemCalls = [];

            IEnumerable<SysCall> syscalls = sysCallLoader.Load();

            foreach (SysCall sysCall in syscalls)
            {
                if (_systemCalls.ContainsKey(sysCall.Name))
                {
                    throw new Exception("Cannot have syscalls with the same name");
                }

                _systemCalls.Add(sysCall.Name, sysCall);
            }
        }

        public void Execute(ref MaciRuntimeData runtimeData, string syscallName)
        {
            if (_systemCalls.TryGetValue(syscallName, out SysCall? sysCall))
            {
                sysCall.Call(ref runtimeData);
            }
            else
            {
                throw new Exception("No syscall with name: " + syscallName);
            }
        }
    }
}
