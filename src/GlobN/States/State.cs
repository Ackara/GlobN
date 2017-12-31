using System;

namespace Acklann.GlobN.States
{
    internal abstract class State
    {
        public State(Glob context)
        {
            Context = context;
        }

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
            Type nextState = GetNextState(p);
            if (nextState != Context.State.GetType())
            {
                Context.State = (State)Activator.CreateInstance(nextState, Context);
            }
        }

        public abstract Result Evaluate(char p, char v);

        public virtual bool EquateCharacters(char p, char v)
        {
            if (p == '/') p = '\\';
            if (v == '/') v = '\\';

            return p == v;
        }

        // ----- HELPER METHODS -----

        internal virtual Type GetNextState(char p)
        {
            Type nextState;
            switch (p)
            {
                default:
                    nextState = typeof(DefaultState);
                    break;

                case '*':
                    if (CharAt(position: -1) == '*')
                    {
                        nextState = typeof(DirectoryWildcardState);
                    }
                    else
                    {
                        nextState = typeof(WildcardState);
                    }
                    break;
            }
            return nextState;
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

        internal bool TryMovingToChar(char v)
        {
            bool charactersAreNotEqual;
            do
            {
                Context.V--;
                charactersAreNotEqual = EquateCharacters(Context.Value[Context.V], v) == false;
            } while (charactersAreNotEqual && Context.V != 0);

            return charactersAreNotEqual == false;
        }
    }
}