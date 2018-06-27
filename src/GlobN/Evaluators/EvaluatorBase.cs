namespace Acklann.GlobN.Evaluators
{
    internal abstract class EvaluatorBase : IEvaluator
    {
        public const char NULL_CHAR = '\0';

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
                        context.E = DefaultEvaluator.Id; /* default: 0 */
                        if (context.State == null) context.State = new DefaultEvaluator();
                        break;

                    case '*':
                        if (context.CharAt(-1) == '*')
                        {
                            context.E = DirectoryWildcardEvaluator.Id;
                            if (context.State == null) context.State = new DirectoryWildcardEvaluator();
                        }
                        else
                        {
                            context.E = WildcardEvaluator.Id;
                            if (context.State == null) context.State = new WildcardEvaluator();
                        }
                        break;

                    case '?':
                        context.E = CharacterWildCardEvaluator.Id;
                        if (context.State == null) context.State = new CharacterWildCardEvaluator();
                        break;
                }
        }

        internal virtual bool EquateCharacters(in Glob context, char p, char v)
        {
            if (p == '/' && v == '\\') return true;
            else if (p == '\\' && v == '/') return true;
            else return p == v;
        }
    }
}