namespace Acklann.GlobN
{
    public static class GlobExtensionMethods
    {
        public static bool IsWildcard(this char character)
        {
            return character == '*' || character == '?';
        }

        public static bool IsDirectorySeparator(this char character)
        {
            return character == '\\' || character == '/';
        }
    }
}