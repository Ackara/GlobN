namespace Acklann.GlobN.Evaluators
{
    internal class WildcardEvaluator : DefaultEvaluator
    {
        public override void Change(in Glob context, in char p)
        {
            if (p.IsaWildcard() || p.IsaDirectorySeparator()) base.Change(context, p);
        }

        public override void Initialize(in Glob context, in char p)
        {
            if (p == '*')
            {
                char nextChar = context.CharAt(-1);
                context.PatternIsIllegal = nextChar.IsaWildcard();

                if (nextChar.IsaDirectorySeparator())
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

        internal override bool EquateCharacters(in Glob context, in char p, in char v)
        {
            if (p == '*') return true;
            else if (v.IsaDirectorySeparator() && p.IsaDirectorySeparator() == false) return false;
            else if (base.EquateCharacters(context, p, v) == false) context.P = _resetIdx;

            return true;
        }

        #region Private Members

        private int _resetIdx;

        #endregion Private Members
    }
}