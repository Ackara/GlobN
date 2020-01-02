using Acklann.GlobN.Evaluation;

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
        }

        /// <summary>
        /// Indicates whether the specified glob expression finds a match in the specified path.
        /// </summary>
        /// <param name="path">The absolute file path.</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <returns><c>true</c> if the specified path match the pattern; otherwise, <c>false</c>.</returns>
        public static bool IsMatch(in string path, in string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || pattern == "*") return true;
            if (string.IsNullOrEmpty(path)) return false;

            bool matchNotFound;
            var context = new Context(path, pattern);

            do
            {
#if DEBUG
                context.WriteLines();
#endif
                SetState(context);
                Evaluate(context);
                Increment(context);

                if (context.State == Status.Failed || context.State == Status.Illegal)
                    return false;
                else if (!context.AtEndOfValue && context.AtEndOfPattern)
                    return false;
                else if (context.AtEndOfValue && !context.AtEndOfPattern)
                    return false;
                else if (context.AtEndOfPattern)
                    return true;
            } while (true);
        }

        /// <summary>
        /// Determines whether the specified path matches this expression.
        /// </summary>
        /// <param name="path">The absolute file path.</param>
        ///
        /// <returns><c>true</c> if the specified absolute path is match this expression; otherwise, <c>false</c>.</returns>
        public bool IsMatch(in string path) => IsMatch(path, _pattern);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => _pattern;

        internal static void SetState(Context context)
        {
            switch (context.PatternAt())
            {
                case Context.wildcard:
                    if (context.PatternAt(1) == Context.wildcard)
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
        }

        internal static void Evaluate(Context context)
        {
            switch (context.State)
            {
                default:
                case Status.Literal:
                    LiteralEvaluator.Evaluate(context);
                    break;

                case Status.Wildcard:

                    break;

                case Status.DirectoryWildcard:
                    break;
            }
        }

        internal static void Increment(Context context)
        {
            switch (context.State)
            {
                default:
                case Status.Literal:
                    LiteralEvaluator.Move(context);
                    break;

                case Status.Wildcard:
                    break;

                case Status.DirectoryWildcard:
                    break;
            }
        }

        #region IEquatable

        /// <summary>
        /// Determines whether two specified <see cref="Glob"/> have the same value.
        /// </summary>
        /// <param name="a">The first <see cref="Glob"/> to compare, or null.</param>
        /// <param name="b">The second <see cref="Glob"/> to compare, or null.</param>
        /// <returns><c>true</c> if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool Equals(Glob a, Glob b)
        {
            return string.Equals(a._pattern, b._pattern);
        }

        /// <summary>
        /// Determines whether two specified <see cref="Glob"/> have the same value.
        /// </summary>
        /// <param name="a">The first <see cref="Glob"/> to compare, or null.</param>
        /// <param name="b">The second <see cref="Glob"/> to compare, or null.</param>
        /// <returns><c>true</c> if the value of <paramref name="a"/> is different from the value of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Glob a, Glob b) => !Equals(a, b);

        /// <summary>
        /// Determines whether two specified <see cref="Glob"/> have the same value.
        /// </summary>
        /// <param name="a">The first <see cref="Glob"/> to compare, or null.</param>
        /// <param name="b">The second <see cref="Glob"/> to compare, or null.</param>
        /// <returns><c>true</c> if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Glob a, Glob b) => Equals(a, b);

        /// <summary>
        /// Determines whether the specified <see cref="Glob" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Glob"/> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="Glob" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Glob other) => Equals(this, other);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Glob) return Equals(this, (Glob)obj);
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
        /// Performs an explicit conversion from <see cref="Glob"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator string(Glob pattern)
        {
            return pattern._pattern;
        }

        #endregion Operators

        #region Private Members

        private readonly string _pattern;

        #endregion Private Members
    }
}