using System;

namespace Acklann.GlobN
{
    [Flags]
    public enum Status
    {
        Literal = 0,
        Wildcard = 1,
        DirectoryWildcard = 2,

        Failed = 32,
        Illegal = 64,
        MatchFound = 128,
    }
}