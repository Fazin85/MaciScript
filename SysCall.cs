namespace MaciScript
{
    public abstract class SysCall
    {
        public abstract int ID { get; }

        protected static Dictionary<string, SysCall> syscalls = [];

        public SysCall(string name)
        {
            syscalls.Add(name, this);
        }

        public abstract void Call(ref MaciRuntimeData runtimeData);
    }
}
