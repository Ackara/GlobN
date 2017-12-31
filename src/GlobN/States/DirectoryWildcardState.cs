using System;

namespace Acklann.GlobN.States
{
    internal class DirectoryWildcardState : WildcardBase
    {
        public DirectoryWildcardState(Glob context) : base(context)
        {
            if (CharAt(position: -2).IsDirectorySeparator() == false || CharAt(position: +1).IsDirectorySeparator() == false)
            {
                System.Diagnostics.Debug.WriteLine($"The directory wildcard (**) was used incorrectly. The directory wildcard can only be used in place of a folder.");
                context.PatternIsIllegal = true;
            }
            else
            {
                Type surrogateType = null;
                ExitChar = NULL_CHAR;
                do
                {
                    /// Looking and accounting for any other wild-card(s) that may appear sequentially after this one,
                    /// as well as determining the surrogate's type.

                    ExitChar = CharAt(-3);
                    _resetPoint = context.P - 3;
                    context.P = context.P - 3;

                    surrogateType = GetNextState(ExitChar);
                    if (surrogateType == this.GetType()) _segmentsToBeTraversed++;
                } while (surrogateType == this.GetType() && _resetPoint > 0);

                if (ExitChar != NULL_CHAR) _segmentsToBeTraversed++;
                _surrogate = (State)Activator.CreateInstance(surrogateType, context);
                if (_surrogate is WildcardBase wildcard) ExitChar = wildcard.ExitChar;
            }
        }

        private State _surrogate;
        
        private int _resetPoint;
        private int _segmentsTraversed = 0;
        private int _segmentsToBeTraversed = 1;

        internal bool TraversedRequiredSegments
        {
            get { return _segmentsTraversed >= _segmentsToBeTraversed; }
        }

        public override Result Evaluate(char p, char v)
        {
            if (Context.PatternIsIllegal) return Result.PatterMatchFailed;

            bool segmentCharsAreEqual = _surrogate.EquateCharacters(p, v);
            bool atEndOfSegment = v.IsDirectorySeparator();
            if (atEndOfSegment) _segmentsTraversed++;

            if (_resetPoint < 0) return Result.PatternMatchComplete; // No more characters and I've ensured that segment will comes after.
            else if (segmentCharsAreEqual && AtEndOfPattern && TraversedRequiredSegments) return Result.PatternMatchComplete;
            else if (AtEndOfValue && !AtEndOfPattern) return Result.PatterMatchFailed;
            else
            {
                if (segmentCharsAreEqual == false)
                {
                    /// Fast-Forwarding the Value's index (V) to the next segment because its clear that the current segments
                    /// will not match. Also if there are no more segments to jump to then the pattern is a failure.

                    if (TryMovingToChar('/') == false) return Result.PatterMatchFailed;
                    Context.P = _resetPoint + 1;
                    _segmentsTraversed++;
                }
                else if (AtEndOfPattern && TraversedRequiredSegments == false)
                {
                    Context.P = _resetPoint + 1;
                }
                else if (segmentCharsAreEqual && atEndOfSegment && TraversedRequiredSegments)
                {
                    Context.State = (State)Activator.CreateInstance(GetNextState(ExitChar), Context);
                }

                return Result.Continue;
            }
        }

        public override void Step()
        {
            _surrogate.Step();
        }

        public override void Change(char p)
        {
            Type nextState = GetNextState(p);
            if (nextState == this.GetType())
            {
                _segmentsToBeTraversed++;
            }
            else if (nextState != _surrogate.GetType())
            {
                _surrogate = (State)Activator.CreateInstance(nextState, Context);
            }
        }

        internal override Type GetNextState(char p)
        {
            Type nextState = base.GetNextState(p);
            if (nextState == typeof(WildcardState) && CharAt(position: -1).IsDirectorySeparator())
            {
                Context.P++;
                _resetPoint++;
                return typeof(DirectoryWildcardState);
            }
            else return nextState;
        }
        
    }
}