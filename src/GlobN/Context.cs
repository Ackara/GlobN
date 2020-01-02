namespace Acklann.GlobN
{
    internal class Context
    {
        public Context(string path, string pattern)
        {
            Value = path;
            V = path.Length - 1;

            Pattern = pattern;
            P = pattern.Length - 1;
        }

        public int V, P;
        public string Value, Pattern;
        public Status State;

        public bool AtEndOfPattern
        {
            get => (P <= 0);
        }

        public bool AtEndOfValue
        {
            get => (V <= 0);
        }

        public void Step()
        {
            if (P > 0) P--;
            if (V > 0) V--;
        }

        public char ValueAt(int steps = default)
        {
            if (steps == default)
            {
                return Value[P];
            }

            steps = P - steps;
            return (steps < 0 ? '\0' : Value[steps]);
        }

        public char PatternAt(int steps = default)
        {
            if (steps == default)
            {
                return Pattern[P];
            }

            steps = P - steps;
            return (steps < 0 ? '\0' : Pattern[steps]);
        }
    }
}