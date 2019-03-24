using Acklann.GlobN.Evaluators;
using System.Linq;

namespace Acklann.GlobN
{
    /// <summary>
    /// Represents an immutable glob expression.
    /// </summary>
    /// <seealso cref="System.IEquatable{T}" />
    public class Glob : System.IEquatable<Glob>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Glob" /> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <exception cref="System.ArgumentNullException">pattern</exception>
        public Glob(string pattern)
        {
            _pattern = pattern;
            Evaluators = new IEvaluator[4];
        }

        internal int E, R;
        internal IEvaluator[] Evaluators;
        internal bool PatternIsIllegal, ShouldIgnoreNextSegment;

        internal int P
        {
            get { return _p; }
            set { _p = value < 0 ? 0 : value; }
        }

        internal string Pattern { get; private set; }

        internal int V
        {
            get { return _v; }
            set { _v = value < 0 ? 0 : value; }
        }

        internal string Value { get; private set; }

        internal bool AtEndOfPattern
        {
            get { return P == 0; }
        }

        internal bool AtEndOfValue
        {
            get { return V == 0; }
        }

        internal IEvaluator State
        {
            get { return Evaluators[E]; }
            set { Evaluators[E] = value; }
        }

        /// <summary>
        /// Indicates whether the specified glob expression finds a match in the specified path.
        /// </summary>
        /// <param name="path">The absolute file path.</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <returns><c>true</c> if the specified path match the pattern; otherwise, <c>false</c>.</returns>
        public static bool IsMatch(in string path, in string pattern)
        {
            return new Glob(pattern).IsMatch(path);
        }

        /// <summary>
        /// Determines whether the specified path matches this expression.
        /// </summary>
        /// <param name="path">The absolute file path.</param>
        /// <param name="expandVariables">Determines whether to replace the name of each environment variable embedded in the specified path with the string equivalent of the value of the variable.</param>
        /// <returns><c>true</c> if the specified absolute path is match this expression; otherwise, <c>false</c>.</returns>
        public bool IsMatch(in string path, bool expandVariables = false)
        {
            if (PatternIsIllegal || string.IsNullOrEmpty(path)) return false;
            else if (string.IsNullOrEmpty(_pattern)
                    || _pattern == "*"
                    || _pattern == path) return true;

            // Initializing the glob's state
            bool negate = _pattern[0] == '!';
            if (State == null) Evaluators[0] = new DefaultEvaluator();

            Value = path;
            V = (Value.Length - 1);

            if (string.IsNullOrEmpty(Pattern)) Pattern = GetNormalizedPattern(expandVariables);
            P = (Pattern.Length - 1);

            // Evaluating if the pattern matches the value/path.
            do
            {
                State.Change(this, Pattern[P]);
                State.Initialize(this, Pattern[P]);
                Outcome result = State.Evaluate(this, char.ToLowerInvariant(Pattern[P]), char.ToLowerInvariant(Value[V]));
#if DEBUG
                string placeholder(int index, char c = '●') => string.Concat(Enumerable.Repeat(c, index));

                System.Diagnostics.Debug.WriteLine($"p    | {Pattern.ToString()}");
                System.Diagnostics.Debug.WriteLine($"p({Pattern[P]}) | {placeholder(P)}{Pattern.Substring(P)}");
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine($"v    | {Value}");
                System.Diagnostics.Debug.WriteLine($"v({Value[V]}) | {placeholder(V)}{Value.Substring(V)}");
                System.Diagnostics.Debug.WriteLine($"===== {result} =====");
#endif
                if (PatternIsIllegal) return false;
                else switch (result)
                    {
                        case Outcome.Continue:
                            State.Step(this);
                            break;

                        case Outcome.MatchFound:
                            return !negate;

                        case Outcome.MatchFailed:
                            return !!negate;
                    }
            } while (true);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return _pattern;
        }

        internal char CharAt(int position)
        {
            int index = P + position;
            return (index >= 0 && index <= (Pattern.Length - 1)) ? Pattern[index] : '\0';
        }

        internal void JumptoNextSegment()
        {
            int tmp = V;

            while (tmp > 0 && Value[tmp].IsaDirectorySeparator() == false)
                if (Value[--tmp].IsaDirectorySeparator())
                {
                    V = tmp;
                }
        }

        #region IEquatable

        /// <summary>
        /// Determines whether the specified <see cref="Glob" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Glob"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="Glob" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Glob other)
        {
            return _pattern == other?._pattern;
        }

        /// <summary>
        /// Indicates whether the expression is equal to another expression.
        /// </summary>
        /// <param name="pattern">An string to compare with this expression.</param>
        /// <returns><c>true</c> if the specified <paramref name="pattern"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(string pattern)
        {
            return _pattern == pattern;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Glob) return Equals((Glob)obj);
            else return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _pattern.GetHashCode();
        }

        #endregion IEquatable

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Glob"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Glob(string pattern)
        {
            return new Glob(pattern);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Glob"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(Glob pattern)
        {
            return pattern._pattern;
        }

        /// <summary>
        /// Determines whether two specified <see cref="Glob"/> have the same value.
        /// </summary>
        /// <param name="a">The first <see cref="Glob"/> to compare, or null.</param>
        /// <param name="b">The second <see cref="Glob"/> to compare, or null.</param>
        /// <returns><c>true</c> if the value of <paramref name="a"/> is different from the value of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Glob a, Glob b)
        {
            return (a?._pattern == b?._pattern) == false;
        }

        /// <summary>
        /// Determines whether two specified <see cref="Glob"/> have the same value.
        /// </summary>
        /// <param name="a">The first <see cref="Glob"/> to compare, or null.</param>
        /// <param name="b">The second <see cref="Glob"/> to compare, or null.</param>
        /// <returns><c>true</c> if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Glob a, Glob b)
        {
            return a?._pattern == b?._pattern;
        }

        #endregion Operators

        #region Private Members

        private readonly string _pattern;
        private int _p, _v;

        private string GetNormalizedPattern(bool expandVariables)
        {
            string pattern;
            if (_pattern[0] == '!')
            {
                pattern = expandVariables ? System.Environment.ExpandEnvironmentVariables(_pattern.Substring(1, _pattern.Length - 1)) : _pattern.Substring(1, _pattern.Length - 1);
            }
            else pattern = expandVariables ? System.Environment.ExpandEnvironmentVariables(_pattern) : _pattern;

            /* Trimming all sequence of the characters ('\\', '/', '.', '**') from the start of the pattern (i.e. a specialized TrimStart()). */
            char c = '\0', prev = '\0';
            int i = 0, wildcard = -1, separator = -1, n = pattern.Length;
            for (i = 0; i < n; i++)
            {
                c = pattern[i];

                if (c.IsaDirectorySeparator()) separator = i;
                if (wildcard == -1 && ((prev == '\0' || prev.IsaDirectorySeparator()) && c == '*' && (i + 1 < n) && pattern[i + 1].IsaDirectorySeparator())) wildcard = i;
                if ((c == '.' || c == '*' || c == '\\' || c == '/') == false) break;

                prev = c;
            }
            i = (wildcard == -1) ? separator : wildcard;
            pattern = (i >= 0) ? pattern.Substring(i, (n - i)) : pattern;

            /* --- */
            if (pattern[pattern.Length - 1].IsaDirectorySeparator())
            {
                pattern = $"{pattern}**\\*";
            }

            if (pattern[0].IsaDirectorySeparator() == false && (n > 2 && pattern[1] != ':')/* drive letter not specified */)
            {
                pattern = '\\' + pattern;
            }

            return pattern;
        }

        #endregion Private Members
    }
}