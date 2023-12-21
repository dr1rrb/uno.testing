using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Uno.Testing.EmbeddedTestHost;
using ElementFactoryGetArgs = Microsoft.UI.Xaml.ElementFactoryGetArgs;

namespace MyUnoTestApp;

public sealed partial class MainPage : Page
{
	private EmbeddedTestHost _client;

	public MainPage()
	{
		this.InitializeComponent();

		Loaded += (snd, e) =>
		{
			//try
			//{

			//	File.Create(Path.Combine(Environment.CurrentDirectory, "log.txt")).Dispose();
			//}
			//catch
			//{
				
			//}

			_info.Text = string.Join(Environment.NewLine, GetArgs()) + $"\r\nTest file exists: {File.Exists("/storage/emulated/uno-tesing/MyUnoTestApp.Tests.dll")} \r\nCurrent dir:{Environment.CurrentDirectory}\r\nLog file: {File.Exists(Path.Combine(Environment.CurrentDirectory, "log.txt"))}";
		};
	}

	private async void Button_Click(object sender, RoutedEventArgs e)
	{
		//System.Diagnostics.Debugger.Launch();
		//System.Diagnostics.Debugger.Break();

		if (_client is not null)
		{
			_client.Dispose();
			return;
		}

		try
		{
#if __ANDROID__
			var logPath = Path.Combine(ContextHelper.Current.FilesDir?.AbsolutePath, "log.txt"); ;
#else
			var logPath = Path.Combine(Environment.CurrentDirectory, "log.txt");
#endif

			Assembly.LoadFile("/data/data/com.companyname.MyUnoTestApp/files/Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll");
			Assembly.LoadFile("/data/data/com.companyname.MyUnoTestApp/files/Microsoft.VisualStudio.TestPlatform.TestFramework.dll");

			_client = Uno.Testing.EmbeddedTestHost.EmbeddedTestHost.Enable(GetArgs(), logPath);
			if (_client is null)
			{
				_output.Text = "Test host not enabled";
				return;
			}

			_client.Exited += (snd, e) =>
			{
				_client = null;
				DispatcherQueue.TryEnqueue(() =>
				{
					_output.Text = "Test host exited";
					((Button)sender).Content = "Start test host";
				});
			};

			((Button)sender).Content = "Stop test host";
			_output.Text = @$"Test host initializing";

			await _client.Initialized();

			_output.Text = @$"Test host enabled
Local-env: {Environment.GetEnvironmentVariable("VSTEST_UWP_DEPLOY_LOCAL_PATH")}
Local-act: {_client.LocalPath}
Remote-env: {Environment.GetEnvironmentVariable("VSTEST_UWP_DEPLOY_REMOTE_PATH")}
Remote-act: {_client.RemotePath}
{_client.Tests}";
		}
		catch (Exception ex)
		{
			_output.Text = ex.ToString();
		}
	}

	private string[] GetArgs()
	{
#if __ANDROID__
		return new[] { "--uno-test-endpoint:" + BaseActivity.Current.Intent?.GetStringExtra("--uno-test-endpoint")?.Replace("127.0.0.1", "10.0.2.2") ?? "" };
		//return new[] { "--uno-test-endpoint:10.0.2.2:12345" };
#else
		return Environment.GetCommandLineArgs();
#endif
	}
}
