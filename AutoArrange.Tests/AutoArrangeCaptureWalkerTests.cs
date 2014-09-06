using AutoArrange;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoArrange.Tests
{
	[TestClass]
	public sealed class AutoArrangeCaptureWalkerTests
	{
		[TestMethod]
		public void VerifyWalker()
		{
			var code = @"
using System;

public class MyClass
{
	public event EventHandler MyEvent;
	public enum MyEnum { A };
	public MyClass() { };
	public string MyProperty { get; set; }
	public void MyMethod() { }
	public class MyNestedClass { }
	public string MyField;
}";
			var tree = SyntaxFactory.ParseSyntaxTree(code).GetRoot();
			var classNode = tree.FindNode(new TextSpan(30, 7)) as TypeDeclarationSyntax;

			var walker = new AutoArrangeCaptureWalker(classNode);
			Assert.AreSame(classNode, walker.Target);
			Assert.AreEqual(1, walker.Constructors.Count, "Constructor count is incorrect.");
			Assert.AreEqual(1, walker.Enums.Count, "Enum count is incorrect.");
			Assert.AreEqual(1, walker.Events.Count, "Event count is incorrect.");
			Assert.AreEqual(1, walker.Fields.Count, "Field count is incorrect.");
			Assert.AreEqual(1, walker.Methods.Count, "Method count is incorrect.");
			Assert.AreEqual(1, walker.Properties.Count, "Property count is incorrect.");
			Assert.AreEqual(1, walker.Types.Count, "Type count is incorrect.");
		}
	}
}
