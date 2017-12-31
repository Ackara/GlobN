namespace Acklann.GlobN.States
{
    internal abstract class WildcardBase : State
    {
        public WildcardBase(Glob context) : base(context)
        {
        }

        public char ExitChar;
    }
}