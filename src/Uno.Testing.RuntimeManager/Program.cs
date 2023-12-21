// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net;
using System.Xml;

Debugger.Launch();
Debugger.Break();

//Console.WriteLine($"Hello, World! : {string.Join("; ", args)}");

var sourceDir = @"C:\Src\GitHub\dr1rrb\uno.testing\src\MyUnoTestApp.Tests\bin\Debug\net8.0-android\";
var packageName = "com.companyname.MyUnoTestApp";
var activity = @$"{packageName}/crc64d5402d0903509cd6.MainActivity"; // TODO: Read AndroidManifest.xml
var tmpDir = @"/storage/emulated/uno-tesing/";

try
{
	var endpoint = args.SkipWhile(a => a != "--endpoint").Skip(1).FirstOrDefault();
	// # WPF
	//var pi = new ProcessStartInfo
	//{
	//	FileName = @"C:\Src\GitHub\dr1rrb\uno.testing\src\MyUnoTestApp\MyUnoTestApp.Skia.WPF\bin\Debug\net8.0-windows\MyUnoTestApp.Skia.Wpf.exe", 
	//	WorkingDirectory = @"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\", 
	//	Arguments = $"--uno-test-endpoint:{endpoint}"
	//};

	// # ANDROID
	// adb install "C:\Src\GitHub\dr1rrb\uno.testing\src\MyUnoTestApp\MyUnoTestApp.Mobile\bin\Debug\net8.0-android\com.companyname.MyUnoTestApp-Signed.apk"
	Adb($"install \"{sourceDir}{packageName}-Signed.apk\"");

	// Copy the test context on the device (into a directory that is accessible to the app)
	var dlls = Directory.GetFiles(sourceDir, "*.dll")
		.Concat(Directory.GetFiles(@$"{sourceDir}_man deps\", "*.dll"));
	foreach (var dll in dlls)
	{
		Adb($"push \"{dll}\" \"{tmpDir}\"");
	}
	Adb($"shell su root cp \"{tmpDir}*.dll\" \"/data/data/{packageName}/files/\"");
	foreach (var dll in dlls)
	{
		Adb($"shell su root chmod 777 \"/data/data/{packageName}/files/{Path.GetFileName(dll)}\"");
	}

	// Start the app
	if (Adb($"shell am start -a android.intent.action.MAIN -n {activity} --es --uno-test-endpoint {endpoint}") is { error: { Length: > 0 } error })
	{
		Console.Error.WriteLine(error);
		return -1;
	}

	//var pi = new ProcessStartInfo
	//{
	//	FileName = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe", 
	//	//WorkingDirectory = @"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\", 
	//	Arguments = ,
	//	UseShellExecute = false,
	//	RedirectStandardOutput = true,
	//	RedirectStandardError = true,
	//};
	//var app = Process.Start(pi);
	//app.WaitForExit(30_000);

	//var result = app.StandardOutput.ReadToEnd();
	//var error = app.StandardError.ReadToEnd();

	//if (error is { Length: > 0 })
	//{
	//	Console.WriteLine(error);
	//	return -1;
	//}


	//var dumpsysInfo = new ProcessStartInfo
	//{
	//	FileName = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe",
	//	Arguments = $"shell dumpsys window windows",
	//	UseShellExecute = false,
	//	RedirectStandardOutput = true,
	//	RedirectStandardError = true,
	//};

	// Wait for the app to exit on the device (we pull for the current 'imeInputTarget' to validate it's still our target activity).
	var failure = 0;
	while (true)
	{
		await Task.Delay(1000);

		var dumpsys = Adb("shell dumpsys window windows", 5_000);
		if (dumpsys.error is not null)
		{
			if (failure++ > 3)
			{
				Console.Error.WriteLine(dumpsys.error);
				return -1;
			}

			continue;
		}

		var inputTarget = dumpsys
			.output
			?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			.Select(line => line.Trim())
			.FirstOrDefault(line => line.StartsWith("imeInputTarget"));

		if (inputTarget is null)
		{
			if (failure++ > 3)
			{
				Console.Error.WriteLine(dumpsys.error);
				return -1;
			}

			continue;
		}
		
		if(!inputTarget.Contains(activity))
		{
			return 0;
		}

		//var dumpsys = Process.Start(dumpsysInfo);
		//if (!dumpsys.WaitForExit(5000))
		//{
		//	try
		//	{
		//		dumpsys.Kill();
		//	}
		//	catch { }

		//	if (failure++ > 3)
		//	{
		//		return -1;
		//	}
		//}
		
		//var inputTarget = dumpsys
		//	.StandardOutput
		//	.ReadToEnd()
		//	.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
		//	.Select(line => line.Trim())
		//	.FirstOrDefault(line => line.StartsWith("imeInputTarget"));

		//if (inputTarget is null || !inputTarget.Contains(activity))
		//{
		//	return 0;
		//}
	}
}
finally
{
	// Copy the log file from the device
	File.WriteAllText($"{sourceDir}\\log.device.txt", Adb($"shell \"run-as {packageName} cat /data/data/{packageName}/files/log.txt\"").output);
}

(string? output, string? error) Adb(string arguments, int timeout = 30_000)
{
	var pi = new ProcessStartInfo
	{
		FileName = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe",
		//WorkingDirectory = @"C:\Users\David\source\repos\UnoTestLibrary\UnoTestApp\bin\Debug\net8.0\", 
		Arguments = arguments,
		UseShellExecute = false,
		RedirectStandardOutput = true,
		RedirectStandardError = true,
	};

	var app = Process.Start(pi);
	if (app is null)
	{
		return (null, "Failed to start adb");
	}
	if (!app.WaitForExit(timeout))
	{
		try
		{
			app.Kill();
		}
		catch { }

		return (null, "Timeout");
	}

	var result = app.StandardOutput.ReadToEnd();
	var error = app.StandardError.ReadToEnd();

	return (result is { Length: 0 } ? null : result, error is { Length: 0 } ? null : error);
}
