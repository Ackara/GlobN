namespace Acklann.GlobN.Evaluators
{
    internal interface IEvaluator
    {
        void Step(in Glob context);

        IEvaluator Change(in Glob contet, char p);

        bool? Evaluate(in Glob context, char p, char v);
    }
}