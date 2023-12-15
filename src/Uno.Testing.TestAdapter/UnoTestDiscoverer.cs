using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;

namespace Uno.Testing.TestAdapter
{
	[FileExtension(".dll")]
	[DefaultExecutorUri("executor://UnoExecutor")]
	[Category("managed")]
	public class UnoTestDiscoverer : ITestDiscoverer
	{
		public UnoTestDiscoverer()
		{
			//_inner = new MSTestDiscoverer();
		}

		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
		{
			//Debugger.Launch();
			//Debugger.Break();

			logger.SendMessage(TestMessageLevel.Error, "DiscoverTests");

			discoverySink.SendTestCase(new TestCase("Bla.Bla.Test", new Uri("executor://UnoExecutor"), sources.First())
			{
				DisplayName = "The.Test.FriendlyName",
				CodeFilePath = @"C:\Users\David\source\repos\UnoTestLibrary\UnoTestLibrary\Class1.cs",
			});
		}
	}
}
