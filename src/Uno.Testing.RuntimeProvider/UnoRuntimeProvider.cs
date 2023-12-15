using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Host;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Uno.Testing.RuntimeProvider
{
	[ExtensionUri("executor://UnoExecutor")]
	public class UnoRuntimeProvider : ITestRuntimeProvider2
	{
		public UnoRuntimeProvider()
		{
			"".ToString();
		}

		/// <inheritdoc />
		public void Initialize(IMessageLogger logger, string runsettingsXml)
		{
			"".ToString();
		}

		/// <inheritdoc />
		public bool CanExecuteCurrentRunConfiguration(string runsettingsXml)
		{
			return true;
		}

		/// <inheritdoc />«
		public void SetCustomLauncher(ITestHostLauncher customLauncher)
		{
			"".ToString();
		}

		/// <inheritdoc />
		public TestHostConnectionInfo GetTestHostConnectionInfo()
		{
			return default;
		}

		/// <inheritdoc />
		public async Task<bool> LaunchTestHostAsync(TestProcessStartInfo testHostStartInfo, CancellationToken cancellationToken)
		{
			return true;
		}

		/// <inheritdoc />
		public TestProcessStartInfo GetTestHostProcessStartInfo(IEnumerable<string> sources, IDictionary<string, string> environmentVariables, TestRunnerConnectionInfo connectionInfo)
		{
			return default;
		}

		/// <inheritdoc />
		public IEnumerable<string> GetTestPlatformExtensions(IEnumerable<string> sources, IEnumerable<string> extensions)
		{
			yield break;
		}

		/// <inheritdoc />
		public IEnumerable<string> GetTestSources(IEnumerable<string> sources)
		{
			return sources;
		}

		/// <inheritdoc />
		public async Task CleanTestHostAsync(CancellationToken cancellationToken)
		{

		}

		/// <inheritdoc />
		public bool Shared => true;

		/// <inheritdoc />
		public event EventHandler<HostProviderEventArgs> HostLaunched;

		/// <inheritdoc />
		public event EventHandler<HostProviderEventArgs> HostExited;

		/// <inheritdoc />
		public bool AttachDebuggerToTestHost()
		{
			return true;
		}
	}
}
