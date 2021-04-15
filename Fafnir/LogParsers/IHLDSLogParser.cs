namespace Fafnir.LogParsers
{
    interface IHLDSLogParser
    {
		public abstract bool IsMatch(string input);

		public abstract void Execute(string input);
	}
}
