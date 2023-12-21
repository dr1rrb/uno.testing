using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Host;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Uno.Testing.RuntimeProvider
{
	[ExtensionUri("HostProvider://UnoTestHost")]
	[FriendlyName("UnoTestHost")]
	public class UnoRuntimeProvider : ITestRuntimeProvider2
	{
		// set VSTEST_RUNNER_DEBUG=1

		// vstest.console UnoTestProject.dll "--TestAdapterPath:C:\Users\David\source\repos\Uno.TestAdapter\Uno.TestAdapter\bin\Debug\netstandard2.0\Uno.TestAdapter.dll;C:\Users\David\source\repos\Uno.TestAdapter\Uno.RuntimeProvider\bin\Debug\netstandard2.0\Uno.RuntimeProvider.dll" --diag:testconsole.log --TestAdapterLoadingStrategy:Explicit -lt

		// https://github.com/microsoft/vstest/blob/c899e96b95463b75a108533204458797e8023251/src/Microsoft.TestPlatform.TestHostProvider/Hosting/DotnetTestHostManager.cs#L50

		public UnoRuntimeProvider()
		{
			Debugger.Launch();
			Debugger.Break();

			"".ToString();
		}

		/// <inheritdoc />
		public bool CanExecuteCurrentRunConfiguration(string runsettingsXml)
		{
			return true;
		}






		/// <inheritdoc />
		public void Initialize(IMessageLogger logger, string runsettingsXml)
		{
			"".ToString();
		}

		/// <inheritdoc />«
		public void SetCustomLauncher(ITestHostLauncher customLauncher)
		{
			"".ToString();
		}








		/// <inheritdoc />
		public TestHostConnectionInfo GetTestHostConnectionInfo()
		{
			Debugger.Launch();
			Debugger.Break();

			var port = 12345;
			do
			{
				try
				{
					var listener = new TcpListener(System.Net.IPAddress.Loopback, port);
					listener.Start();
					listener.Stop();
					break;
				}
				catch (SocketException)
				{
					port++;
				}
			} while (true);

			return new TestHostConnectionInfo { Endpoint = $"127.0.0.1:{port}", Role = ConnectionRole.Client, Transport = Transport.Sockets };
		}

		/// <inheritdoc />
		public TestProcessStartInfo GetTestHostProcessStartInfo(IEnumerable<string> sources, IDictionary<string, string>? environmentVariables, TestRunnerConnectionInfo connectionInfo)
		{
			environmentVariables ??= new Dictionary<string, string>();
			environmentVariables["UNO_TESTING_HOST_ENDPOINT"] = connectionInfo.ConnectionInfo.Endpoint;

			return new TestProcessStartInfo
			{
				FileName = @"C:\Src\GitHub\dr1rrb\uno.testing\src\MyUnoTestApp\MyUnoTestApp.Skia.WPF\bin\Debug\net8.0-windows\MyUnoTestApp.Skia.Wpf.exe",
				EnvironmentVariables = environmentVariables,
				WorkingDirectory = @"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\",
				Arguments = $"--uno-test-endpoint:127.0.0.1:{connectionInfo.Port}"
			};
		}

		/// <inheritdoc />
		public async Task<bool> LaunchTestHostAsync(TestProcessStartInfo testHostStartInfo, CancellationToken cancellationToken)
		{
			//Task.Run(async () =>
			{
				var pi = new ProcessStartInfo(testHostStartInfo.FileName, testHostStartInfo.Arguments)
				{
					WorkingDirectory = testHostStartInfo.WorkingDirectory,
					UseShellExecute = true,
					//RedirectStandardOutput = true,
					//RedirectStandardError = true,
					//CreateNoWindow = true,
				};
				//if (testHostStartInfo.EnvironmentVariables is { Count: > 0 })
				//{
				//	foreach (var environmentVariable in testHostStartInfo.EnvironmentVariables)
				//	{
				//		pi.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
				//	}
				//}

				var app = new Process() { StartInfo = pi };
				app.Start();

				await Task.Delay(15_000, cancellationToken);

				HostLaunched?.Invoke(this, new HostProviderEventArgs("Host started.") { ProcessId = app.Id });
			}//);

			return true;
		}

		/// <inheritdoc />
		public IEnumerable<string> GetTestPlatformExtensions(IEnumerable<string> sources, IEnumerable<string> extensions)
		{
			//yield break;

			return extensions;
		}

		/// <inheritdoc />
		public IEnumerable<string> GetTestSources(IEnumerable<string> sources)
		{
			return sources;
		}

		/// <inheritdoc />
		public async Task CleanTestHostAsync(CancellationToken cancellationToken)
		{
			Task.Run(async () =>
			{
				await Task.Delay(30_000);
				HostExited?.Invoke(this, new HostProviderEventArgs("Host exited."));
			});
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
