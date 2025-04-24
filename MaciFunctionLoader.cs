using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciFunctionLoader
    {
        public FrozenDictionary<string, int> FunctionNameToIndex => functionNameToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> functionNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        public bool TryLoad(MaciCompilationData compilationData, string line, int instructionIndex, List<MaciFunction> functions)
        {
            if (line.StartsWith("function") && line.EndsWith(':'))
            {
                // Extract just the function name between "function" and ":"
                string funcName = line[8..^1].Trim();

                var function = new MaciFunction()
                {
                    Address = instructionIndex,
                    Name = funcName
                };

                functions.Add(function);
                functionNameToIndex[funcName] = compilationData.FunctionOffset + functions.Count - 1;

                return true;
            }

            return false;
        }
    }
}
