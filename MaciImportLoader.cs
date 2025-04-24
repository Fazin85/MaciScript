namespace MaciScript
{
    public static class MaciImportLoader
    {
        public static bool TryLoad(string line, IEnumerable<string> sourceFilePaths, List<string> imports)
        {
            if (line.StartsWith("import"))
            {
                int importIndex = line.IndexOf("import") + "import".Length;
                string filePath = line[importIndex..].Trim();

                if (!sourceFilePaths.Contains(filePath))
                {
                    throw new Exception($"No source file with path: {filePath}");
                }

                imports.Add(filePath);

                return true;
            }

            return false;
        }
    }
}
