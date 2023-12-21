using System.Diagnostics;

namespace MuUnoTestApp.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void JustATestInTheTestAssembly()
		{
			//Debugger.Launch();
			Console.WriteLine(Environment.ProcessPath);
			Console.WriteLine(Environment.CommandLine);

			Debugger.Break();
		}
	}
}
