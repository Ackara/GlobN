namespace Acklann.GlobN.States
{
    internal class DefaultState : State
    {
        public override void Initialize(Glob context)
        {
            Context = context;
        }

        public override Result Evaluate(in char p, in char v)
        {
            bool charactersAreEqual = EquateCharacters(p, v);

            if (Context.PatternIsIllegal || (AtEndOfValue && !AtEndOfPattern)) return Result.PatterMatchFailed;
            else if (charactersAreEqual && (AtEndOfPattern || p == ':')) return Result.PatternMatchComplete;
            else return new Result(charactersAreEqual, null);
        }
    }
}