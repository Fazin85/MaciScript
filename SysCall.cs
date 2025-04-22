namespace MaciScript
{
    public abstract class SysCall
    {
        public abstract int ID { get; }

        public abstract void Call(MaciRuntimeData runtimeData);
    }
}
