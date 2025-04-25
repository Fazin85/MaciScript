namespace MaciScript
{
    public class MaciMacro(string name, List<string> parameters, List<string> body, string sourceFile)
    {
        public string Name { get; } = name;
        public List<string> Parameters { get; } = parameters;
        public List<string> Body { get; } = body;
        public string SourceFile { get; } = sourceFile;

        // Method to expand a macro call with actual arguments
        public List<string> Expand(List<string> arguments, Dictionary<string, MaciMacro> allMacros, HashSet<string> expansionStack)
        {
            if (arguments.Count != Parameters.Count)
            {
                throw new ArgumentException($"Macro {Name} expects {Parameters.Count} arguments, but got {arguments.Count}");
            }

            // Check for circular references
            if (expansionStack.Contains(Name))
            {
                throw new InvalidOperationException($"Circular macro reference detected: {string.Join(" -> ", expansionStack)} -> {Name}");
            }

            // Add this macro to the expansion stack
            expansionStack.Add(Name);

            var expandedLines = new List<string>();

            // For each line in the macro body
            foreach (var line in Body)
            {
                string expandedLine = line;

                // Replace each parameter with its corresponding argument
                for (int i = 0; i < Parameters.Count; i++)
                {
                    expandedLine = expandedLine.Replace(Parameters[i], arguments[i]);
                }

                // Check if this line contains another macro call
                var macroCallMatch = MaciMacroProcessor.MacroCallPattern.Match(expandedLine);
                if (macroCallMatch.Success)
                {
                    string innerMacroName = macroCallMatch.Groups[1].Value;

                    if (allMacros.TryGetValue(innerMacroName, out MaciMacro? innerMacro))
                    {
                        // Parse inner macro arguments
                        string argsString = macroCallMatch.Groups[2].Value;
                        List<string> innerArgs = MaciMacroProcessor.ParseArguments(argsString);

                        // Expand the inner macro
                        List<string> innerExpandedLines = innerMacro.Expand(innerArgs, allMacros, new HashSet<string>(expansionStack));

                        // Replace the macro call with its expansion
                        expandedLines.AddRange(innerExpandedLines);
                    }
                    else
                    {
                        // Not a macro call, just add the line as is
                        expandedLines.Add(expandedLine);
                    }
                }
                else
                {
                    // No nested macro calls, add the line as is
                    expandedLines.Add(expandedLine);
                }
            }

            // Remove this macro from the expansion stack
            expansionStack.Remove(Name);

            return expandedLines;
        }
    }
}
