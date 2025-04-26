using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciStringLoader(IEnumerable<string> stringInstructions)
    {
        public FrozenDictionary<string, int> StringToIndex => stringToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> stringToIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly IEnumerable<string> stringInstructions = stringInstructions;

        public void TryLoad(ref MaciCompilationData compilationData, int lineIndex, string line, List<string> strings, Dictionary<int, string> stringLines)
        {
            foreach (var instruction in stringInstructions)
            {
                if (line.StartsWith(instruction))
                {
                    var strText = Util.ExtractNestedQuotes(line);

                    if (strText == null)
                    {
                        return;
                    }

                    int idx = strings.IndexOf(strText);

                    if (idx == -1)
                    {
                        strings.Add(strText);
                        stringToIndex[strText] = compilationData.StringOffset + strings.Count - 1;
                    }
                    else
                    {
                        stringToIndex[strText] = idx;
                    }

                    stringLines.Add(lineIndex, line);
                }
            }
        }
    }
}
