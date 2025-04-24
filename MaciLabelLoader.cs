using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciLabelLoader
    {
        public FrozenDictionary<string, int> LabelNameToIndex => labelNameToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> labelNameToIndex = [];

        public bool TryLoad(MaciCompilationData compilationData, string line, int instructionIndex, List<MaciLabel> labels)
        {
            if (line.EndsWith(':'))
            {
                string labelName = line[..^1].Trim();
                foreach (var l in labels)
                {
                    if (labelName == l.Name)
                    {
                        throw new Exception("Cannot have duplicate labels: " + labelName);
                    }
                }

                var label = new MaciLabel
                {
                    Address = instructionIndex,
                    Name = labelName
                };

                // Store label info
                labels.Add(label);
                labelNameToIndex[label.Name] = compilationData.LabelOffset + labels.Count - 1;

                return true;
            }
            return false;
        }
    }
}
