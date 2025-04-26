namespace MaciScript
{
    public abstract class SysCall
    {
        public abstract int ID { get; }

        protected static Dictionary<string, SysCall> Syscalls { get; set; } = [];

        public SysCall(string name)
        {
            Syscalls.Add(name, this);
        }

        public abstract void Call(ref MaciRuntimeData runtimeData);
    }
}
