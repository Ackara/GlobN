namespace Acklann.GlobN.Evaluators
{
    internal abstract class EvaluatorBase : IEvaluator
    {
        public abstract void Initialize(in Glob context, in char p);

        public abstract Outcome Evaluate(in Glob context, in char p, in char v);

        /* ***** */

        public virtual void Step(in Glob context)
        {
            context.P--;
            context.V--;
        }

        public virtual void Change(in Glob context, in char p)
        {
            if (context != null)
                switch (p)
                {
                    default:
                        context.E = 0; /* default: 0 */
                        if (context.State == null) context.State = new DefaultEvaluator();
                        break;

                    case '*':
                        if (context.CharAt(-1) == '*')
                        {
                            context.E = 1;
                            if (context.State == null) context.State = new DirectoryWildcardEvaluator();
                        }
                        else
                        {
                            context.E = 2;
                            if (context.State == null) context.State = new WildcardEvaluator();
                        }
                        break;

                    case '?':
                        context.E = 3;
                        if (context.State == null) context.State = new CharacterWildCardEvaluator();
                        break;
                }
        }

        internal virtual bool EquateCharacters(in Glob context, in char p, in char v)
        {
            if (p == '/' && v == '\\') return true;
            else if (p == '\\' && v == '/') return true;
            else return p == v;
        }
    }
}