using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciStringLoader
    {
        public FrozenDictionary<string, int> StringToIndex => stringToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> stringToIndex = new(StringComparer.OrdinalIgnoreCase);

        public void TryLoad(int lineIndex, string line, List<string> strings, Dictionary<int, string> stringLines)
        {
            if (line.StartsWith("ldstr"))
            {
                var strText = Util.ExtractNestedQuotes(line);

                if (strText == null)
                {
                    return;
                }

                strings.Add(strText);
                stringToIndex[strText] = strings.Count - 1;
                stringLines.Add(lineIndex, line);
            }
        }
    }
}
