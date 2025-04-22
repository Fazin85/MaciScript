using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciFunctionLoader
    {
        public FrozenDictionary<string, int> FunctionNameToIndex => functionNameToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> functionNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly MaciRuntimeData runtimeData;

        public MaciFunctionLoader(MaciRuntimeData runtimeData)
        {
            this.runtimeData = runtimeData;
        }

        public bool TryLoad(string line, int instructionIndex, out MaciFunction function)
        {
            function = default;

            if (line.StartsWith("function") && line.EndsWith(":"))
            {
                // Extract just the function name between "function" and ":"
                string funcName = line[8..^1].Trim();

                function = new()
                {
                    Address = instructionIndex,
                    Name = funcName
                };

                runtimeData.Functions.Add(function);
                functionNameToIndex[funcName] = runtimeData.Functions.Count - 1;

                return true;
            }

            return false;
        }
    }
}
