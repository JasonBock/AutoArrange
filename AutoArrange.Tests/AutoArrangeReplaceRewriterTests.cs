using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoArrange.Tests
{
	[TestClass]
	public sealed class AutoArrangeReplaceRewriterTests
	{
		[TestMethod]
		public void VerifyRewriter()
		{
			var code = @"
using System;

public class MyClass
{
	public class MyNestedClassB { }
	public class MyNestedClassA { }
	public enum MyEnumB { A };
	public enum MyEnumA { A };
	public string MyPropertyB { get; set; }
	public string MyPropertyA { get; set; }
	public void MyMethodB() { }
	public void MyMethodA() { }
	public MyClass() { };
	public MyClass(int value) { };
	public string MyFieldB;
	public string MyFieldA;
	public event EventHandler MyEventB;
	public event EventHandler MyEventA;
}";

			var expectedCode = @"
public class MyClass
{
	public event EventHandler MyEventA;
	public event EventHandler MyEventB;
	public string MyFieldA;
	public string MyFieldB;
	public MyClass() { };
	public MyClass(int value) { };
	public void MyMethodA() { }
	public void MyMethodB() { }
	public string MyPropertyA { get; set; }
	public string MyPropertyB { get; set; }
	public enum MyEnumA { A };
	public enum MyEnumB { A };
	public class MyNestedClassA { }
	public class MyNestedClassB { }
}";

			var tree = SyntaxFactory.ParseSyntaxTree(code).GetRoot();
			var classNode = tree.FindNode(new TextSpan(30, 7)) as TypeDeclarationSyntax;
			var walker = new AutoArrangeCaptureWalker(classNode);

			var rewriter = new AutoArrangeReplaceRewriter(walker).VisitTypeDeclaration(classNode);
			var text = rewriter.GetText();
			Assert.AreEqual(expectedCode, text.ToString());
		}
	}
}
