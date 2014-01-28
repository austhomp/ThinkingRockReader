namespace TRDataReader.Extensions
{
    public static class StringExtensions
    {
        public static string EmptyIfNull(this string input)
        {
            return input ?? string.Empty;
        }
    }
}