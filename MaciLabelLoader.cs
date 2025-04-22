using System.Collections.Frozen;

namespace MaciScript
{
    public class MaciLabelLoader
    {
        public FrozenDictionary<string, int> LabelNameToIndex => labelNameToIndex.ToFrozenDictionary();

        private readonly MaciRuntimeData runtimeData;
        private readonly Dictionary<string, int> labelNameToIndex = [];

        public MaciLabelLoader(MaciRuntimeData runtimeData)
        {
            this.runtimeData = runtimeData;
        }

        public bool TryLoad(string line, int instructionIndex, out MaciLabel label)
        {
            label = default;

            if (line.EndsWith(":"))
            {
                string labelName = line[..^1].Trim();
                foreach (var l in runtimeData.Labels)
                {
                    if (labelName == l.Name)
                    {
                        throw new Exception("Cannot have duplicate labels: " + label);
                    }
                }

                label = new MaciLabel
                {
                    Address = instructionIndex,
                    Name = labelName
                };

                // Store label info
                runtimeData.Labels.Add(label);
                labelNameToIndex[label.Name] = runtimeData.Labels.Count - 1;

                return true;
            }
            return false;
        }
    }
}
