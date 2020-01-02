namespace Acklann.GlobN.Evaluation
{
    internal class Evaluator
    {
        private const char wildcard = '*';

        public static Context ChangeState(Context context)
        {
            switch (context.PatternAt())
            {
                case wildcard:
                    if (context.PatternAt(1) == wildcard)
                    {
                        context.State = Status.DirectoryWildcard;
                    }
                    else
                    {
                        context.State = Status.Wildcard;
                    }
                    break;

                default:
                    context.State = Status.Literal;
                    break;
            }

            return context;
        }

        public static void Evaluate(Context context)
        {
            switch (context.State)
            {
                case Status.Literal:

                    break;
                case Status.Wildcard:

                    break;
                case Status.DirectoryWildcard:

                    break;
            }
        }
    }
}