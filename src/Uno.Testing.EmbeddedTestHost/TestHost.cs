using System;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using static System.Net.Mime.MediaTypeNames;

namespace Uno.Testing.EmbeddedTestHost;

internal class UnoTestRequestHandler : TestRequestHandler
{
	public UnoTestRequestHandler(TestHostConnectionInfo connectionInfo, ICommunicationEndpointFactory communicationFactory)
		: base(JsonDataSerializer.Instance, communicationFactory)
	{
		ConnectionInfo = connectionInfo;
	}

	public string LocalPath { get; private set; }
	public string RemotePath { get; private set; }
	public string Tests { get; private set; }

	internal void PatchPathConverter()
	{
		var pathConverter = typeof(TestRequestHandler).GetField("_pathConverter", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this);
		if (pathConverter is null)
		{
			LocalPath = RemotePath = "--no path converter--";
		}
		else
		{
			if (pathConverter.GetType().GetField("_originalPath", BindingFlags.Instance | BindingFlags.NonPublic) is { } orgPathField)
			{
				if (orgPathField.GetValue(pathConverter) is string { Length: > 0 } path)
				{
					LocalPath = path?.ToString()?.Trim('/') + '\\';
					orgPathField.SetValue(pathConverter, LocalPath);
				}
				else
				{
					LocalPath = "--path not set--";
				}
			}
			else
			{
				LocalPath = $"--no original field-- ({pathConverter.GetType().Name})";
			}

			RemotePath = pathConverter.GetType().GetField("_deploymentPath", BindingFlags.Instance | BindingFlags.NonPublic) is { } depPathPField
				? depPathPField.GetValue(pathConverter)?.ToString() ?? "--path not set--"
				: $"--no deployment field-- ({pathConverter.GetType().Name})";

			try
			{
				var directionType = typeof(TestRequestHandler).Assembly.GetType("Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.PathConversionDirection");
				var direction = Enum.Parse(directionType, "Receive");

				var updatePath = pathConverter.GetType().GetMethod("UpdatePath")!;

				var p1 = updatePath.Invoke(pathConverter, new[] { "C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\MyUnoTestApp.Tests.dll", direction });
				//var p2 = ((dynamic)pathConverter).UpdatePath("C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\MyUnoTestApp.Tests.dll", 1);
				var p3 = updatePath.Invoke(pathConverter, new[] { "/C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\MyUnoTestApp.Tests.dll", direction });
				//var p4 = ((dynamic)pathConverter).UpdatePath("/C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\MyUnoTestApp.Tests.dll", 1);
				var p4 = updatePath.Invoke(pathConverter, new[] { "C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll", direction });

				var updatePaths = pathConverter.GetType().GetMethod("UpdatePaths")!;

				Tests = string.Join(Environment.NewLine, new[] { p1, p3 });
			}
			catch (Exception ex)
			{
				Tests = ex.ToString();
			}
		}
	}
}

internal class UnoCommunicationEndpointFactory : ICommunicationEndpointFactory
{
	private readonly CommunicationEndpointFactory _inner = new(); 
	
	internal event EventHandler? OnCreating;

	/// <inheritdoc />
	public ICommunicationEndPoint Create(ConnectionRole role)
	{
		OnCreating?.Invoke(this, EventArgs.Empty);
		return _inner.Create(role);
	}
}

public class EmbeddedTestHost : IDisposable
{
	private readonly TaskCompletionSource _initialized = new();
	private readonly UnoTestRequestHandler _handler;
	private bool _isDisposed = false;

	public event EventHandler? Exited;

	public static EmbeddedTestHost? Enable(string[] args, string logPath)
	{
		File.Create(logPath).Dispose();
		EqtTrace.InitializeVerboseTrace(logPath);

		var endpointArg = args.FirstOrDefault(arg => arg.StartsWith("--uno-test-endpoint:"));
		if (endpointArg is null)
		{
			return null;
		}

		// TODO: other parameters!
		// TODO: For net8.0, instead of parsing parameters, we should just ref the original testhost.dll and invoke UnitTestClient.Start(string[] args)
		var endpoint = endpointArg.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
		if (endpoint is null)
		{
			return null;
		}

		return new(new TestHostConnectionInfo { Role = ConnectionRole.Client, Endpoint = endpoint, Transport = Transport.Sockets });
	}

	public string LocalPath => _handler.LocalPath;
	public string RemotePath => _handler.RemotePath;		

	public string Tests => _handler.Tests;

	private EmbeddedTestHost(TestHostConnectionInfo connection)
	{
		Environment.SetEnvironmentVariable("VSTEST_UWP_DEPLOY_LOCAL_PATH", "C:\\Src\\GitHub\\dr1rrb\\uno.testing\\src\\MyUnoTestApp.Tests\\bin\\Debug\\net8.0-android\\");
		Environment.SetEnvironmentVariable("VSTEST_UWP_DEPLOY_REMOTE_PATH", "/data/data/com.companyname.MyUnoTestApp/files/");
		var factory = new UnoCommunicationEndpointFactory();
		factory.OnCreating += (snd, e) => _handler!.PatchPathConverter();
		_handler = new UnoTestRequestHandler(connection, factory);

		Task.Run(() =>
		{
			try
			{
				_handler.InitializeCommunication();
				_initialized.SetResult();
				_handler.ProcessRequests(new TestHostManagerFactory(false));

				Exited?.Invoke(this, EventArgs.Empty);

				if (!_isDisposed)
				{
					Environment.Exit(0);
				}
			}
			catch (Exception ex)
			{
				_initialized.TrySetException(ex);
			}
		});
	}

	public Task Initialized() => _initialized.Task;

	/// <inheritdoc />
	public void Dispose()
	{
		_isDisposed = true;
		_initialized.TrySetCanceled();
		try
		{
			_handler.Close();
		}
		catch
		{
			// Ignore if _handler has already closed its connection
		}
		_handler.Dispose();
	}
}
