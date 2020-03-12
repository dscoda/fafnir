using System;
using System.Collections.Generic;
using System.Text;

namespace Fafnir.LogParsers
{
    interface IHLDSLogParser
    {
		public abstract bool IsMatch(string input);

		public abstract void Execute(string input);
	}
}
