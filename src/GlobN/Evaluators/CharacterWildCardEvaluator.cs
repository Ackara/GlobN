namespace Acklann.GlobN.Evaluators
{
    internal class CharacterWildCardEvaluator : DefaultEvaluator
    {
        internal override bool EquateCharacters(in Glob context, in char p, in char v) => (v.IsaDirectorySeparator() == false);
    }
}