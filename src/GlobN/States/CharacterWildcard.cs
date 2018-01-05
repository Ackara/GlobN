namespace Acklann.GlobN.States
{
    internal class CharacterWildcard : DefaultState
    {
        public CharacterWildcard(Glob context) : base(context)
        {
        }

        public override bool EquateCharacters(char p, char v)
        {
            return true;
        }
    }
}