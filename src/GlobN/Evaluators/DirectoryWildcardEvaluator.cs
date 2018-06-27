namespace Acklann.GlobN.Evaluators
{
    internal class DirectoryWildcardEvaluator : DefaultEvaluator
    {
        public new static readonly int Id = 3;

        public override void Initialize(in Glob context, in char p)
        {
            if (p == '*' && context.CharAt(-1) == '*')
            {
                if (context.CharAt(-2).IsDirectorySeparator() && context.CharAt(+1).IsDirectorySeparator())
                {
                    context.P += -3;
                    context.R = (context.P + 1);
                    context.ShouldIgnoreNextSegment = true;

                    base.Change(context, context.Pattern[context.P]);
                    context.State.Initialize(context, context.Pattern[context.P]);
                }
                else context.PatternIsIllegal = true;
            }
        }
    }
}