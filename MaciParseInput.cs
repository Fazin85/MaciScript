namespace MaciScript
{
    public class MaciParseInput(int symbolCollectionIndex, List<MaciSymbolsCollection> symbolsCollections, string line, int lineNumber)
    {
        public int SymbolCollectionIndex = symbolCollectionIndex;
        public List<MaciSymbolsCollection> SymbolsCollections = symbolsCollections;
        public string Line = line;
        public int LineNumber = lineNumber;
    }
}
