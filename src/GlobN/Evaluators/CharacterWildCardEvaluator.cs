using System;

namespace Acklann.GlobN.Evaluators
{
    internal class CharacterWildCardEvaluator : DefaultEvaluator
    {
        private CharacterWildCardEvaluator() : base()
        {
        }

        public override bool? Evaluate(in Glob context, char p, char v)
        {
            throw new NotImplementedException();
        }

        #region Singleton

        public new static CharacterWildCardEvaluator Instance
        {
            get { return Nested._instance; }
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly CharacterWildCardEvaluator _instance = new CharacterWildCardEvaluator();
        }

        #endregion Singleton
    }
}