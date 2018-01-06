namespace Acklann.GlobN.States
{
    internal class CharacterWildcard : DefaultState
    {
        protected CharacterWildcard()
        {
        }

        public new static CharacterWildcard Instance
        {
            get { return Nested._instance; }
        }

        public override bool EquateCharacters(char p, char v)
        {
            return true;
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly CharacterWildcard _instance = new CharacterWildcard();
        }
    }
}