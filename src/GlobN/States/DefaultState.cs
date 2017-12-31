namespace Acklann.GlobN.States
{
    internal class DefaultState : State
    {
        public DefaultState(Glob context) : base(context)
        {
        }

        public override Result Evaluate(char p, char v)
        {
            bool continuePatternMatching = EquateCharacters(p, v);

            if (Context.PatternIsIllegal) return Result.PatterMatchFailed;
            else if (continuePatternMatching && AtEndOfPattern) return Result.PatternMatchComplete;
            else if (AtEndOfValue && !AtEndOfPattern) return Result.PatterMatchFailed;
            else return new Result(continuePatternMatching, null);
        }
    }
}