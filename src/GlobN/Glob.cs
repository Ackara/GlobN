using Acklann.GlobN.States;
using System;

namespace Acklann.GlobN
{
    public class Glob
    {
        #region Operators

        public static implicit operator Glob(string pattern) => new Glob(pattern);

        #endregion Operators

        public Glob() : this(string.Empty)
        {
        }

        public Glob(string pattern)
        {
            _pattern = pattern;
            State = new DefaultState(this);
        }

        internal State State;
        internal bool PatternIsIllegal;

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

        public bool IsMatch(string absolutePath)
        {
            if (string.IsNullOrEmpty(_pattern)
                || _pattern == "*"
                || _pattern == absolutePath) return true;
            else if (string.IsNullOrEmpty(absolutePath) || PatternIsIllegal) return false;

            // Initializing the glob's state
            Value = absolutePath;
            V = (Value.Length - 1);

            Pattern = GetNormalizedPattern(Value);
            P = (Pattern.Length - 1);

            // Evaluating if the pattern matches the value/path.
            do
            {
                State.Change(Pattern[P]);
                Result result = State.Evaluate(Pattern[P], Value[V]);

                if (result.PatternIsMatch != null) return result.PatternIsMatch ?? false;
                else if (result.ContinuePatternMatching == false) return false;
                else State.Step();
            } while (true);
        }

        public string ExpandPath(string filePath)
        {
            return filePath;
        }

        #region Private Members

        private static readonly char _volumeSeparatorChar = System.IO.Path.VolumeSeparatorChar;
        private string _pattern;
        private int _p, _v;

        private string GetNormalizedPattern(string value)
        {
            string pattern = Environment.ExpandEnvironmentVariables(_pattern)
                .TrimStart('.', '\\', '/');

            if (pattern.EndsWith("\\") || pattern.EndsWith("/"))
            {
                pattern = $"{pattern}**\\*";
            }

            if (pattern.IndexOf(_volumeSeparatorChar) == -1)
            {
                pattern = '\\' + pattern;
            }

            return pattern;
        }

        #endregion Private Members
    }
}