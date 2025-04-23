namespace MaciScript
{
    public static class Util
    {
        public static string? ExtractNestedQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            int firstQuoteIndex = input.IndexOf('"');
            if (firstQuoteIndex < 0)
            {
                return null;
            }

            int secondQuoteIndex = input.IndexOf('"', firstQuoteIndex + 1);
            if (secondQuoteIndex < 0)
            {
                return null;
            }

            // Extract the content between the quotes
            int startIndex = firstQuoteIndex + 1;
            int length = secondQuoteIndex - startIndex;
            return input.Substring(startIndex, length);
        }
    }
}
