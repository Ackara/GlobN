namespace Acklann.GlobN.States
{
    internal class WildcardState : WildcardBase
    {
        public WildcardState(Glob context) : base(context)
        {
            _wildcardNotSatisfied = true;
            ExitChar = CharAt(position: -1);

            if (ExitChar.IsWildcard())
            {
                System.Diagnostics.Debug.WriteLine($"Cannot use a wildcard (*) immediateley after another.");
                context.PatternIsIllegal = true;
            }
        }

        private bool _wildcardNotSatisfied;

        public override Result Evaluate(char p, char v)
        {
            if (Context.PatternIsIllegal) return Result.PatterMatchFailed;
            else if (base.EquateCharacters(ExitChar, v) == true)
            {
                Context.P--;
                Context.State.Change(ExitChar);
                return Context.State.Evaluate(ExitChar, v);
            }
            else if (AtEndOfValue && !AtEndOfPattern) return Result.PatterMatchFailed;
            else if (AtEndOfValue) return Result.PatternMatchComplete;
            else return Result.Continue;
        }

        public override bool EquateCharacters(char p, char v)
        {
            if (v == ExitChar)
            {
                _wildcardNotSatisfied = false;
                Context.P--;
                return true;
            }
            else if (_wildcardNotSatisfied && p == '*') return true;
            else return base.EquateCharacters(p, v);
        }

        public override void Step()
        {
            if (_wildcardNotSatisfied) Context.V--;
            else base.Step();
        }
    }
}