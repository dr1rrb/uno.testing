using System.Reflection;
using Uno.UI.Runtime.Skia.Wpf;
using WpfApp = System.Windows.Application;

namespace MyUnoTestApp.WPF;
public partial class App : WpfApp
{
	public App()
	{
		

		if (Environment.GetEnvironmentVariable("UNO_TESTING_SOURCE") is { Length: > 0 } testSource)
		{
			Assembly.LoadFile(testSource);
		}

		var host = new WpfHost(Dispatcher, () => new AppHead());
		host.Run();
	}
}
