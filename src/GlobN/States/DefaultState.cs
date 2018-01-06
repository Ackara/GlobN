namespace Acklann.GlobN.States
{
    internal class DefaultState : State
    {
        protected DefaultState()
        {
        }

        public static DefaultState Instance
        {
            get { return Nested._instance; }
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

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly DefaultState _instance = new DefaultState();
        }
    }
}