namespace MaciScript
{
    public class MaciCodeUnit
    {
        public MaciFunction[] Functions { get; private set; }
        public MaciLabel[] Labels { get; private set; }
        public MaciInstruction[] Instructions { get; private set; }

        private MaciCodeUnit()
        {

        }

        public static MaciCodeUnit FromString(string source)
        {
            MaciCodeUnit codeUnit = new();
            MaciFunctionLoader functionLoader = new();
            MaciLabelLoader labelLoader = new();

            List<MaciFunction> functions = [];
            List<MaciLabel> labels = [];

            try
            {
                // First pass: collect all labels and function addresses
                int instructionIndex = 0;
                var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                        continue;

                    // Check for function declarations
                    if (functionLoader.TryLoad(line, instructionIndex, functions))
                    {
                        continue;
                    }

                    // Check for regular labels
                    if (labelLoader.TryLoad(line, instructionIndex, labels))
                    {
                        continue;
                    }

                    // Actual instruction that will be executed
                    instructionIndex++;
                }
                codeUnit.Functions = [.. functions];
                codeUnit.Labels = [.. labels];

                List<MaciInstruction> instructions = [];

                // Second pass: parse instructions
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Skip labels, comments, and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.EndsWith(":"))
                        continue;

                    // Parse instruction
                    var instruction = MaciScriptParser.ParseInstruction(functionLoader.FunctionNameToIndex, labelLoader.LabelNameToIndex, line);
                    instructions.Add(instruction);
                }

                codeUnit.Instructions = [.. instructions];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading program: {ex.Message}");
            }

            return codeUnit;
        }
    }
}
