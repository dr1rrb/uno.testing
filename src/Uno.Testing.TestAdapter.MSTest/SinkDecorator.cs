using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Uno.Testing.TestAdapter.MSTest;

internal class SinkDecorator(ITestCaseDiscoverySink inner) : ITestCaseDiscoverySink
{
	/// <inheritdoc />
	public void SendTestCase(TestCase discoveredTest)
	{
		discoveredTest.ExecutorUri = new Uri("executor://UnoExecutor");

		inner.SendTestCase(discoveredTest);
	}
}