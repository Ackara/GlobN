namespace Acklann.GlobN.States
{
    internal class CharacterWildcard : DefaultState
    {
        public override bool EquateCharacters(char p, char v) => true;
    }
}