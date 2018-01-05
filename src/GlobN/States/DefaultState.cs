namespace Acklann.GlobN.States
{
    internal class DefaultState : State
    {
        public DefaultState(Glob context) : base(context)
        {
        }

        public override void Initialize(Glob context)
        {
            Context = context;
        }

        public override Result Evaluate(char p, char v)
        {
            bool charactersAreEqual = EquateCharacters(p, v);

            if (Context.PatternIsIllegal) return Result.PatterMatchFailed;
            else if (AtEndOfValue && !AtEndOfPattern) return Result.PatterMatchFailed;
            else if (charactersAreEqual && (AtEndOfPattern || p == ':')) return Result.PatternMatchComplete;
            else return new Result(charactersAreEqual, null);
        }
    }
}