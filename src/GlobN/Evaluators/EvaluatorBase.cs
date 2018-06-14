namespace Acklann.GlobN.Evaluators
{
    internal abstract class EvaluatorBase : IEvaluator
    {
        public const char NULL_CHAR = '\0';

        public abstract bool? Evaluate(in Glob context, char p, char v);

        /* ***** */

        public virtual IEvaluator Change(in Glob context, char p)
        {
            return GetEvaluator(context, p);
        }

        public virtual void Step(in Glob context)
        {
            context.P--;
            context.V--;
        }

        protected bool AtEndOfValue(in Glob context)
        {
            return context.V == 0;
        }

        protected bool AtEndOfPattern(in Glob context)
        {
            { return context.P == 0; }
        }

        protected virtual bool EquateCharacters(char p, char v)
        {
            if (p == '/' && v == '\\') return true;
            else if (p == '\\' && v == '/') return true;
            else return p == v;
        }

        protected char CharAt(in Glob context, int position)
        {
            int index = context.P + position;
            return (index >= 0 && index <= (context.Pattern.Length - 1)) ? context.Pattern[index] : NULL_CHAR;
        }

        protected IEvaluator GetEvaluator(in Glob context, char p)
        {
            IEvaluator next;
            switch (p)
            {
                default:
                    next = DefaultEvaluator.Instance;
                    break;

                case '*':
                    next = WildcardEvaluator.Instance;
                    break;

                case '?':
                    next = CharacterWildCardEvaluator.Instance;
                    break;
            }

            return next;
        }
    }
}