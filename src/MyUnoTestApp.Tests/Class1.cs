using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyUnoTestApp.Tests;

[TestClass]
public class Class1
{
	[TestMethod]
	public void MyTestMethod()
	{
		File.AppendAllText("/data/data/com.companyname.MyUnoTestApp/files/MyTestMethod.txt", $"Run on {DateTimeOffset.Now}");

		"".ToString();
	}
}
