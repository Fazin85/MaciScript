using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciParseInput(FrozenDictionary<string, int> functionNameToIndex,
    FrozenDictionary<string, int> labelNameToIndex,
    FrozenDictionary<string, int> stringToIndex,
    Dictionary<int, string> stringLines,
    string line,
    int lineNumber)
    {
        public FrozenDictionary<string, int> FunctionNameToIndex = functionNameToIndex;
        public FrozenDictionary<string, int> LabelNameToIndex = labelNameToIndex;
        public FrozenDictionary<string, int> StringToIndex = stringToIndex;
        public Dictionary<int, string> StringLines = stringLines;
        public string Line = line;
        public int LineNumber = lineNumber;
    }
}
