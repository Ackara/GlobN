namespace Acklann.GlobN.Benchmark
{
    internal struct Header
    {
        public Header(string name, string units = "")
        {
            Name = name; Units = units;
        }

        public string Name, Units;

        public override string ToString() => string.Concat(Name, (string.IsNullOrEmpty(Units) ? string.Empty : $" ({Units})"));
    }
}