﻿namespace Acklann.GlobN
{
    internal struct Result
    {
        public Result(bool continueMatching, bool? patternMatchingComplete)
        {
            ContinuePatternMatching = continueMatching;
            PatternIsMatch = patternMatchingComplete;
        }

        public static readonly Result PatternMatchComplete = new Result(true, true);
        public static readonly Result PatterMatchFailed = new Result(false, false);
        public static readonly Result Continue = new Result(true, null);

        public readonly bool ContinuePatternMatching;
        public readonly bool? PatternIsMatch;
    }
}