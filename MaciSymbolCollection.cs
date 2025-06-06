﻿using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciSymbolCollection
    {
        public required string FilePath;
        public required string[] Imports;
        public required MaciFunction[] Functions;
        public required MaciLabel[] Labels;
        public required string[] Strings;
        public required FrozenDictionary<string, int> FunctionNameToIndex;
        public required FrozenDictionary<string, int> StringToIndex;
        public required FrozenDictionary<string, int> LabelNameToIndex;
        public required Dictionary<int, string> StringLines;
    }
}
