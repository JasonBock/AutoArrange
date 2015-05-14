using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AutoArrange.Tests
{
	[TestClass]
	public sealed class AutoArrangeCaptureWalkerTests
	{
		[TestMethod]
		public void VerifyWalker()
		{
			var code = File.ReadAllText(@"Targets\VerifyWalker.cs");
			var tree = SyntaxFactory.ParseSyntaxTree(code).GetRoot();
			var node = tree.FindNode(new TextSpan(30, 7)) as TypeDeclarationSyntax;

			var walker = new AutoArrangeCaptureWalker(node);
			Assert.AreSame(node, walker.Target);
			Assert.AreEqual(1, walker.Constructors.Count, nameof(walker.Constructors));
			Assert.AreEqual(1, walker.Enums.Count, nameof(walker.Enums));
			Assert.AreEqual(1, walker.Events.Count, nameof(walker.Events));
			Assert.AreEqual(1, walker.Fields.Count, nameof(walker.Fields));
			Assert.AreEqual(1, walker.Methods.Count, nameof(walker.Methods));
			Assert.AreEqual(1, walker.Properties.Count, nameof(walker.Properties));
			Assert.AreEqual(1, walker.Types.Count, nameof(walker.Types));
		}
	}
}