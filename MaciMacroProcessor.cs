using System.Text.RegularExpressions;

namespace MaciScript
{
    public class MaciMacroProcessor
    {
        private readonly Dictionary<string, MaciMacro> macros = [];

        // Regex patterns for macro definition and invocation
        public static readonly Regex MacroDefPattern = new Regex(
            @"macro\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{([\s\S]*?)\}",
            RegexOptions.Compiled);

        public static readonly Regex MacroCallPattern = new Regex(
            @"([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)",
            RegexOptions.Compiled);

        // First pass: Collect all macro definitions from all files
        public void CollectMacroDefinitions(MaciInputFileData[] inputFileData)
        {
            foreach (var file in inputFileData)
            {
                string fileName = file.FilePath;
                string code = file.FileContent;

                foreach (Match match in MacroDefPattern.Matches(code))
                {
                    string name = match.Groups[1].Value;
                    string parametersStr = match.Groups[2].Value;
                    string body = match.Groups[3].Value;

                    List<string> parameters = parametersStr
                        .Split(',')
                        .Select(p => p.Trim())
                        .ToList();

                    List<string> bodyLines = body
                        .Split('\n')
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();

                    if (macros.TryGetValue(name, out MaciMacro? macro))
                    {
                        throw new InvalidOperationException($"Duplicate macro definition: {name} in file {fileName}. Already defined in {macro.SourceFile}");
                    }

                    macros[name] = new MaciMacro(name, parameters, bodyLines, fileName);
                }
            }
        }

        // Second pass: Process each file, expanding macros
        public string[] ExpandMacros(MaciInputFileData[] inputFileData)
        {
            var processedFiles = new string[inputFileData.Length];

            for (int i = 0; i < inputFileData.Length; i++)
            {
                string fileName = inputFileData[i].FilePath;
                string code = inputFileData[i].FileContent;

                // Remove macro definitions from the code
                code = MacroDefPattern.Replace(code, "");

                // Split into lines and process each line
                List<string> lines = code.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                List<string> processedLines = [];

                foreach (var line in lines)
                {
                    if (MacroCallPattern.IsMatch(line))
                    {
                        var match = MacroCallPattern.Match(line);
                        string macroName = match.Groups[1].Value;

                        if (macros.TryGetValue(macroName, out MaciMacro? macro))
                        {
                            // Parse arguments
                            string argsString = match.Groups[2].Value;
                            List<string> args = ParseArguments(argsString);

                            // Expand the macro
                            List<string> expandedLines = macro.Expand(args, macros, []);
                            processedLines.AddRange(expandedLines);
                        }
                        else
                        {
                            // Not a macro call, just a regular instruction with parameters
                            processedLines.Add(line);
                        }
                    }
                    else
                    {
                        // Regular line, no macro call
                        processedLines.Add(line);
                    }
                }

                processedFiles[i] = FileLinesToString(processedLines);
            }

            return processedFiles;
        }

        private static string FileLinesToString(List<string> lines)
        {
            string str = "";

            foreach (var s in lines)
            {
                str += s + '\n';
            }

            return str;
        }

        // Parse arguments from a comma-separated string, handling nested commas in parentheses
        public static List<string> ParseArguments(string argsString)
        {
            List<string> args = [];

            if (string.IsNullOrWhiteSpace(argsString))
                return args;

            int parDepth = 0;
            int lastSplit = 0;

            for (int i = 0; i < argsString.Length; i++)
            {
                char c = argsString[i];

                if (c == '(') parDepth++;
                else if (c == ')') parDepth--;
                else if (c == ',' && parDepth == 0)
                {
                    // Found a top-level comma, split here
                    args.Add(argsString.Substring(lastSplit, i - lastSplit).Trim());
                    lastSplit = i + 1;
                }
            }

            // Add the last argument
            if (lastSplit < argsString.Length)
            {
                args.Add(argsString.Substring(lastSplit).Trim());
            }

            return args;
        }

        // Helper method to get all processed macros (useful for debugging)
        public Dictionary<string, MaciMacro> GetAllMacros()
        {
            return new Dictionary<string, MaciMacro>(macros);
        }
    }
}

