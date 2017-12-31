using System;

namespace Acklann.GlobN
{
    [Flags]
    internal enum TokenKind
    {
        Literal = 1,
        Wildcard = 2,
        CharacterWildcard = 4,
        DirectoryWildcard = 8,
        CharacterSet = 16
    }
}