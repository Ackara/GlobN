namespace Acklann.GlobN.States
{
    internal abstract class State
    {
        public const char NULL_CHAR = '\0';
        public Glob Context;

        public bool AtEndOfValue
        {
            get { return Context.V == 0; }
        }

        public bool AtEndOfPattern
        {
            get { return Context.P == 0; }
        }

        public virtual void Step()
        {
            Context.P--;
            Context.V--;
        }

        public virtual void Change(char p)
        {
            Context.State = GetState(p);
        }

        public abstract void Initialize(Glob context);

        public abstract Result Evaluate(char p, char v);

        public virtual bool EquateCharacters(char p, char v)
        {
            if (p == '/') p = '\\';
            if (v == '/') v = '\\';

            return p == v;
        }

        // ----- HELPER METHODS -----

        internal State GetState(char p)
        {
            State nextState;
            switch (p)
            {
                default:
                    if (_default == null) _default = new DefaultState();
                    nextState = _default;
                    break;

                case '*':
                    if (CharAt(position: -1) == '*')
                    {
                        Context.P--;
                        if (_directoryWildcard == null) _directoryWildcard = new DirectoryWildcardState();
                        nextState = _directoryWildcard;
                    }
                    else
                    {
                        if (_wildcard == null) _wildcard = new WildcardState();
                        nextState = _wildcard;
                    }
                    break;

                case '?':
                    if (_characterWildcard == null) _characterWildcard = new CharacterWildcard();
                    nextState = _characterWildcard;
                    break;
            }

            nextState.Initialize(Context);
            return nextState;
        }

        internal char CharAt(int position)
        {
            int index = Context.P + position;
            return (index >= 0 && index <= (Context.Pattern.Length - 1)) ? Context.Pattern[index] : NULL_CHAR;
        }

        #region Private Members

        private State _default, _wildcard, _directoryWildcard, _characterWildcard;

        #endregion Private Members
    }
}