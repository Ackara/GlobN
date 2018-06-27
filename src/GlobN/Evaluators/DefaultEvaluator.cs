namespace Acklann.GlobN.Evaluators
{
    internal class DefaultEvaluator : EvaluatorBase
    {
        public static readonly int Id = 0;

        public override Outcome Evaluate(in Glob context, in char p, in char v)
        {
            bool charsAreEqual = EquateCharacters(context, p, v);
            if (v.IsDirectorySeparator() && p.IsDirectorySeparator()) context.ShouldIgnoreNextSegment = false;

            if (context.PatternIsIllegal || (charsAreEqual == false && context.ShouldIgnoreNextSegment == false) || (context.AtEndOfValue && !context.AtEndOfPattern)) return Outcome.MatchFailed;
            else if (charsAreEqual && context.AtEndOfPattern) return Outcome.MatchFound;
            else if (charsAreEqual == false && context.ShouldIgnoreNextSegment)
            {
                if (v.IsDirectorySeparator() == false) context.JumptoNextSegment();
                context.P = context.R;
                return Outcome.Continue;
            }

            return Outcome.Continue;
        }

        public override void Initialize(in Glob context, in char p)
        {
        }
    }
}