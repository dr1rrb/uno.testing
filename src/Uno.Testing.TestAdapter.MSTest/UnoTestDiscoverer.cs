using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.ComponentModel;
using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;

namespace Uno.Testing.TestAdapter.MSTest;

[FileExtension(".dll")]
[DefaultExecutorUri("executor://UnoExecutor")]
[Category("managed")]
public class UnoTestDiscoverer : ITestDiscoverer
{
	private readonly MSTestDiscoverer _inner = new();

	public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
		=> _inner.DiscoverTests(sources, discoveryContext, logger, new SinkDecorator(discoverySink));
}