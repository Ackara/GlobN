namespace Acklann.GlobN.Evaluators
{
    internal interface IEvaluator
    {
        void Step(in Glob context);

        void Change(in Glob context, in char p);

        void Initialize(in Glob context, in char p);

        Outcome Evaluate(in Glob context, in char p, in char v);
    }
}