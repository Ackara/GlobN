namespace Acklann.GlobN.Evaluators
{
    internal class DefaultEvaluator : EvaluatorBase
    {
        protected DefaultEvaluator()
        {
        }

        public override bool? Evaluate(in Glob context, char p, char v)
        {
            bool charactersAreEqual = EquateCharacters(p, v);

            if (context.PatternIsIllegal) return false;
            else if (charactersAreEqual == false) return false;
            else if (AtEndOfValue(context) && !AtEndOfPattern(context)) return false;
            else if (charactersAreEqual && AtEndOfPattern(context)) return true;
            else return null;
        }

        #region Singleton

        public static DefaultEvaluator Instance
        {
            get { return Nested._instance; }
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly DefaultEvaluator _instance = new DefaultEvaluator();
        }

        #endregion Singleton
    }
}