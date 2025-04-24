using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciLabelLoader
    {
        public FrozenDictionary<string, int> LabelNameToIndex => labelNameToIndex.ToFrozenDictionary();

        private readonly Dictionary<string, int> labelNameToIndex = [];

        public bool TryLoad(ref MaciCompilationData compilationData, List<MaciSymbolCollection> existingSymbolCollections, string line, int instructionIndex, List<MaciLabel> labels)
        {
            if (line.EndsWith(':'))
            {
                string labelName = line[..^1].Trim();

                if (existingSymbolCollections.Any(x => x.Labels.Any(y => y.Name == labelName)) || labels.Any(x => x.Name == labelName))
                {
                    throw new Exception($"Cannot have multiple labels with the same name: {labelName}");
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
