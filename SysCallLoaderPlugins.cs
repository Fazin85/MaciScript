using System.Collections.Frozen;

namespace MaciScript
{
    public class SysCallLoaderPlugins : ISysCallLoader
    {
        public FrozenDictionary<string, SysCallPlugin> Plugins => plugins.ToFrozenDictionary();

        private readonly Dictionary<string, SysCallPlugin> plugins = [];

        public SysCallLoaderPlugins()
        {
            RegisterPlugin(new CoreSysCallPluginLoader(new MaciMemoryAllocatorList()));
        }

        public void RegisterPlugin(IMaciScriptSysCallPluginLoader pluginLoader)
        {
            SysCallPlugin plugin = pluginLoader.Load();

            plugins.Add(plugin.Name, plugin);
        }

        public IEnumerable<SysCall> Load()
        {
            List<SysCall> sysCallList = [];

            foreach (SysCallPlugin plugin in plugins.Values)
            {
                sysCallList.AddRange(plugin.SysCalls);
            }

            return sysCallList;
        }
    }
}
