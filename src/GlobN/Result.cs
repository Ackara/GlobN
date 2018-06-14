namespace Acklann.GlobN
{
    [System.Diagnostics.DebuggerDisplay("{ToDebuggerDisplay()}")]
    internal struct Result
    {
        public Result(bool continueMatching, bool? patternMatchingComplete)
        {
            ContinuePatternMatching = continueMatching;
            PatternIsMatch = patternMatchingComplete;
        }

        public static readonly Result PatternMatchComplete = new Result(true, true);
        public static readonly Result PatterMatchFailed = new Result(false, false);
        public static readonly Result MoveForward = new Result(true, null);

        public readonly bool ContinuePatternMatching;
        public readonly bool? PatternIsMatch;

        public string ToDebuggerDisplay()
        {
            if (ContinuePatternMatching) return "continuing";
            else if (PatternIsMatch ?? false) return "match-found";
            else return "undetermined";
        }
    }
}