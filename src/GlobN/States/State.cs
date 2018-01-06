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
            if (Context.P > 0) Context.P--;
            if (Context.V > 0) Context.V--;
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
                    nextState = DefaultState.Instance;
                    break;

                case '*':
                    if (CharAt(position: -1) == '*')
                    {
                        Context.P--;
                        nextState = DirectoryWildcardState.Instance;
                    }
                    else
                    {
                        nextState = WildcardState.Instance;
                    }
                    break;

                case '?':
                    nextState = CharacterWildcard.Instance;
                    break;
            }

            nextState.Initialize(Context);
            return nextState;
        }

        internal bool TryFastForwardingTo(char v)
        {
            bool charactersAreNotEqual;
            do
            {
                Context.V--;
                charactersAreNotEqual = EquateCharacters(Context.Value[Context.V], v) == false;
            } while (charactersAreNotEqual && Context.V != 0);

            return charactersAreNotEqual == false;
        }

        internal char CharAt(int position, bool useValue = false)
        {
            string text = useValue ? Context.Value : Context.Pattern;
            int index = (useValue ? Context.V : Context.P) + position;

            if (index >= 0 && index <= (text.Length - 1))
            {
                return text[index];
            }

            return NULL_CHAR;
        }
    }
}