namespace Acklann.GlobN.Evaluators
{
    internal class WildcardEvaluator : DefaultEvaluator
    {
        public new static readonly int Id = 1;

        public override void Change(in Glob context, in char p)
        {
            if (p.IsWildcard() || p.IsDirectorySeparator()) base.Change(context, p);
        }

        public override void Initialize(in Glob context, in char p)
        {
            if (p == '*')
            {
                char nextChar = context.CharAt(-1);
                context.PatternIsIllegal = nextChar.IsWildcard();

                if (nextChar.IsDirectorySeparator())
                {
                    context.P--;
                    context.JumptoNextSegment();
                    base.Change(context, nextChar);
                }
                else
                {
                    _resetIdx = context.P;
                    context.P--;
                }
            }
        }

        internal override bool EquateCharacters(in Glob context, char p, char v)
        {
            if (p == '*') return true;
            else if (v.IsDirectorySeparator() && p.IsDirectorySeparator() == false) return false;
            else if (base.EquateCharacters(context, p, v) == false) context.P = _resetIdx;

            return true;
        }

        #region Private Members

        private int _resetIdx;

        #endregion Private Members
    }
}