namespace MaciScript
{
    public class SysCallPlugin
    {
        public readonly string Name;
        public readonly IEnumerable<SysCall> SysCalls;

        public SysCallPlugin(IEnumerable<SysCall> sysCalls, string name)
        {
            Name = name;
            SysCalls = sysCalls;
        }
    }
}
