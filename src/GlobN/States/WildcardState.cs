namespace Acklann.GlobN.States
{
    internal class WildcardState : DefaultState
    {
        public new static WildcardState Instance
        {
            get { return Nested._instance; }
        }

        public override void Initialize(Glob context)
        {
            Context = context;
            _notSatisfied = true;
            _exitChar = CharAt(position: -1);

            if (_exitChar == '*' || _exitChar == '?')
            {
                context.PatternIsIllegal = true;
                string error = context.FormatError($"The '{_exitChar}:{context.P - 1}' character cannot preceed the wildcard character ('*' {context.P}) ");
                if (context.ThrowIfInvalid) throw new Exceptions.IllegalGlobException(error);
            }
        }

        public override bool EquateCharacters(char p, char v)
        {
            if (_notSatisfied && base.EquateCharacters(_exitChar, v) == true)
            {
                _notSatisfied = false;
                Context.P--;
                return true;
            }
            else if (_notSatisfied && v.IsDirectorySeparator())
            {
                _notSatisfied = false;
                return false;
            }
            else if (_notSatisfied) return true;
            else return base.EquateCharacters(p, v);
        }

        public override void Step()
        {
            if (_notSatisfied) Context.V--;
            else base.Step();
        }

        #region Private Members

        private char _exitChar;
        private bool _notSatisfied;

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly WildcardState _instance = new WildcardState();
        }

        #endregion Private Members
    }
}