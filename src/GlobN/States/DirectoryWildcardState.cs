namespace Acklann.GlobN.States
{
    internal class DirectoryWildcardState : State
    {
        private State _surrogate;

        private bool _satisfied;
        private int _resetPoint, _segmentsTraversed = 0, _segmentsToBeTraversed = 1;

        public bool TraversedAllSegments
        {
            get { return _segmentsTraversed >= _segmentsToBeTraversed; }
        }

        public override void Initialize(Glob context)
        {
            Context = context;
            _satisfied = false;
            _segmentsToBeTraversed = 1;
            _resetPoint = _segmentsTraversed = 0;
            if ((CharAt(position: -1).IsDirectorySeparator() && CharAt(position: +2).IsDirectorySeparator()) == false)
            {
                context.PatternIsIllegal = true;
                _surrogate = new DefaultState();
                _surrogate.Initialize(Context);
                string error = context.FormatError($"The directory-wildcard (**) must be between directory separators (example: root/**/folder)");
                if (context.ThrowIfInvalid) throw new Exceptions.IllegalGlobException(error);
            }
            else
            {
                FastForward();
                _surrogate = GetState(Context.Pattern[Context.P]);
            }
        }

        public override void Change(char p)
        {
            if (_satisfied) base.Change(p);
            else _surrogate = GetState(p);
        }

        public override Result Evaluate(char p, char v)
        {
            if (v.IsDirectorySeparator()) _segmentsTraversed++;
            bool segmentCharsAreEqual = _surrogate.EquateCharacters(p, v);
            _satisfied = !_satisfied ? (segmentCharsAreEqual && v.IsDirectorySeparator() && TraversedAllSegments) : _satisfied;
            
            if (Context.PatternIsIllegal || (AtEndOfValue && !AtEndOfPattern)) return Result.PatterMatchFailed;
            else if (_satisfied && (AtEndOfPattern || p == ':')) return Result.PatternMatchComplete;
            else if (segmentCharsAreEqual) return Result.MoveForward;
            else
            {
                Context.P = _resetPoint;
                if (v.IsDirectorySeparator() == false)
                    if (TryJumpToNext('\\'))
                    {
                        _segmentsTraversed++;
                    }
                    else
                    {
                        return Result.PatterMatchFailed;
                    }
            }

            return Result.MoveForward;
        }

        public override void Step()
        {
            _surrogate.Step();
        }

        public void FastForward()
        {
            char next, cur, prev;
            do
            {
                Context.P--;
                next = CharAt(position: -1);
                prev = CharAt(position: +1);
                cur = Context.Pattern[Context.P];
                if ((next.IsDirectorySeparator() || next == NULL_CHAR) && cur == '*' && prev.IsDirectorySeparator())
                {
                    _segmentsToBeTraversed++;
                }
            } while (!AtEndOfPattern && (cur == '*' || cur.IsDirectorySeparator()));
            Context.P = prev == '*' ? Context.P + 1 : Context.P;
            _resetPoint = Context.P + 1;
        }

        public bool TryJumpToNext(char v)
        {
            bool charactersAreNotEqual;
            do
            {
                Context.V--;
                charactersAreNotEqual = EquateCharacters(Context.Value[Context.V], v) == false;
            } while (charactersAreNotEqual && Context.V != 0);

            return charactersAreNotEqual == false;
        }
    }
}