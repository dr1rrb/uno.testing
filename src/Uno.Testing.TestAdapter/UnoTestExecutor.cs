using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Uno.Testing.TestAdapter
{
	[ExtensionUri("executor://UnoExecutor")]
	public class UnoTestExecutor : ITestExecutor2
	{
		public void Cancel()
		{
		}

		public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
		{
			// cf : IFrameworkHandle and https://github.com/microsoft/vstest-docs/blob/main/RFCs/0029-Debugging-External-Test-Processes.md
			// also : ITestRuntimeProvider https://github.com/microsoft/vstest-docs/blob/main/RFCs/0025-Test-Host-Runtime-Provider.md

			Debugger.Launch();
			Debugger.Break();

			var currentDirectory = Environment.CurrentDirectory;
			var appPath = Path.Combine(currentDirectory, @"..\..\..\..\MyUnoTestApp\MyUnoTestApp.Skia.WPF\bin\Debug\net8.0-windows\MyUnoTestApp.Skia.Wpf.exe");
			var resultPath = Path.Combine(currentDirectory, @"testResult.json");

			var settings = runContext.RunSettings.SettingsXml;
			frameworkHandle.SendMessage(TestMessageLevel.Error, "Bla.Bla.Test");
			//frameworkHandle.LaunchProcessWithDebuggerAttached(@"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\UnoTestApp.exe", ".", "", null);
			frameworkHandle.LaunchProcessWithDebuggerAttached(
				//@"C:\Users\David\source\repos\UnoTestLibrary\MyUnoTestApp\MyUnoTestApp.Skia.WPF\bin\Debug\net8.0-windows\MyUnoTestApp.Skia.Wpf.exe",
				appPath,
				".",
				"",
				new Dictionary<string, string>
				{
					{ "UNO_TESTING_SOURCE", tests.First().Source },
					{ "UNO_RUNTIME_TESTS_RUN_TESTS", "{}" },
					{ "UNO_RUNTIME_TESTS_OUTPUT_PATH", resultPath },
				});
		}

		public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
		{
			Debugger.Launch();
			Debugger.Break();

			var settings = runContext.RunSettings.SettingsXml;
			frameworkHandle.SendMessage(TestMessageLevel.Error, "Bla.Bla.Test");
			// if (runContext.IsBeingDebugged)
			{
				frameworkHandle.LaunchProcessWithDebuggerAttached(@"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\UnoTestApp.exe", ".", "", null);
			}
		}

		/// <inheritdoc />
		public bool ShouldAttachToTestHost(IEnumerable<string> sources, IRunContext runContext)
		{
			Debugger.Launch();
			Debugger.Break();

			return true;
		}

		/// <inheritdoc />
		public bool ShouldAttachToTestHost(IEnumerable<TestCase> tests, IRunContext runContext)
		{
			Debugger.Launch();
			Debugger.Break();

			return true;
		}
	}
}
