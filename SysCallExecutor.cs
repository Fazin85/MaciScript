namespace MaciScript
{
    public class SysCallExecutor
    {
        private readonly Dictionary<int, SysCall> _systemCalls;

        public SysCallExecutor(ISysCallLoader sysCallLoader)
        {
            _systemCalls = [];

            IEnumerable<SysCall> syscalls = sysCallLoader.Load();

            foreach (SysCall sysCall in syscalls)
            {
                if (_systemCalls.ContainsKey(sysCall.ID))
                {
                    throw new Exception("Cannot have syscalls with the same id");
                }

                _systemCalls.Add(sysCall.ID, sysCall);
            }
        }

        public void Execute(ref MaciRuntimeData runtimeData, int syscallId)
        {
            if (_systemCalls.TryGetValue(syscallId, out SysCall? sysCall))
            {
                sysCall.Call(ref runtimeData);
                Console.WriteLine("executed syscall with id: " + syscallId);
            }
            else
            {
                throw new Exception("No syscall with id: " + syscallId);
            }
        }
    }
}
