namespace Acklann.GlobN.Evaluation
{
    internal class LiteralEvaluator
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
            bool charMatched;

            switch (context.State)
            {
                case Status.Literal:
                    charMatched = Equals(context.PatternAt(), context.ValueAt());
                    break;

                case Status.Wildcard:
                    break;

                case Status.DirectoryWildcard:
                    break;
            }
        }

        public static bool Equals(char p, char v)
        {
            if ((p == '/' && v == '\\') || (p == '\\' && v == '/')) return true;
            else return char.ToUpperInvariant(p) == char.ToUpperInvariant(v);
        }

        internal static void Move(Context context)
        {
            context.Step();
        }
    }
}