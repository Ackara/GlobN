namespace Acklann.GlobN.Evaluators
{
    internal class CharacterWildCardEvaluator : DefaultEvaluator
    {
        public new static readonly int Id = 2;

        internal override bool EquateCharacters(in Glob context, char p, char v) => (v.IsDirectorySeparator() == false);
    }
}