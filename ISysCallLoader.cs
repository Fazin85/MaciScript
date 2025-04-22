namespace MaciScript
{
    public interface ISysCallLoader
    {
        IEnumerable<SysCall> Load();
    }
}
