namespace MaciScript
{
    public class MaciParseInput(int symbolCollectionIndex, List<MaciSymbolCollection> symbolCollections, string line, int lineNumber)
    {
        public int SymbolCollectionIndex = symbolCollectionIndex;
        public List<MaciSymbolCollection> SymbolCollections = symbolCollections;
        public string Line = line;
        public int LineNumber = lineNumber;
    }
}
