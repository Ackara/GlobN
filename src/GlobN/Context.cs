using System;
using System.Linq;

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

        public const char wildcard = '*';
        public string Value, Pattern;
        public Status State;
        public int V, P;

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

        internal void WriteLines()
        {
            int len = (Value.Length > Pattern.Length ? Value.Length : Pattern.Length);
            string positions = string.Join(" ", Enumerable.Range(0, len).Select(x => $"{x}".PadLeft(2, ' ')));

            int idx(int x) => (Math.Abs(Value.Length - Pattern.Length) + x);

            void foo(int x, string c)
            {
                string[] s = Enumerable.Repeat(" ", len).ToArray();
                s[Math.Abs(Pattern.Length - Value.Length) + x] = c;
                write(string.Join(" ", s.Select(x => x.PadLeft(2, ' '))));
            }


            static void write(string x = default)
            {
                System.Console.WriteLine(x);
                System.Diagnostics.Debug.WriteLine(x);
            }


            write(string.Join(" ", Value.Select(x => $" {x}")));
            write(string.Join(" ", Pattern.Select(x => $" {x}")).PadLeft(positions.Length, '-'));
            write(positions);


            string[] spaces = Enumerable.Repeat(" ", len).ToArray();
            spaces[Math.Abs(Pattern.Length - Value.Length) + P] = "p";
            write(string.Join(" ", spaces.Select(x => x.PadLeft(2, ' '))));

            spaces = Enumerable.Repeat(" ", len).ToArray();
            spaces[V] = "v";
            write(string.Join(" ", spaces.Select(x => x.PadLeft(2, ' '))));

            write("\r\n");
        }
    }
}