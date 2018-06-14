namespace Acklann.GlobN.Evaluators
{
    internal class WildcardEvaluator : DefaultEvaluator
    {
        public override bool? Evaluate(in Glob context, char p, char v)
        {
            char next = CharAt(context, -1);
            bool charactersAreEqual = base.EquateCharacters(next, v);

            return base.Evaluate(context, CharAt(context, -1), v);
        }

        public override void Step(in Glob context)
        {


            base.Step(context);
        }

        #region Singleton

        public new static WildcardEvaluator Instance
        {
            get { return Nested._instance; }
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly WildcardEvaluator _instance = new WildcardEvaluator();
        }

        #endregion Singleton

        #region Private Members

        private int _index;
        private bool _statisfied;

        #endregion Private Members
    }
}