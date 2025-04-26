namespace MaciScript
{
    public abstract class SysCall
    {
        public abstract int ID { get; }
        public string Name { get; private set; }

        protected static Dictionary<string, SysCall> Syscalls { get; set; } = [];

        public SysCall(string name)
        {
            Syscalls.Add(name, this);
            Name = name;
        }

        public abstract void Call(ref MaciRuntimeData runtimeData);
    }
}
