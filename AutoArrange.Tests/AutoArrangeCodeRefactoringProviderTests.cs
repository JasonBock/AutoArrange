using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoArrange.Tests
{
	[TestClass]
	public sealed class AutoArrangeCodeRefactoringProviderTests
	{
		[TestMethod]
		public async Task RefactorWhenTargetIsNotTypeDelcaraction()
		{
			var code = @"
using System;

public class MyClass
{
	public void MyMethod() { }
}";

			var document = AutoArrangeCodeRefactoringProviderTests.CreateDocument(code);

			var provider = new AutoArrangeCodeRefactoringProvider();
			var actions = (await provider.GetRefactoringsAsync(document, 
				new TextSpan(55, 8), new CancellationToken(false))).ToList();
			Assert.AreEqual(0, actions.Count);
      }

		[TestMethod]
		public async Task RefactorWhenCodeDoesNotNeedRefactoring()
		{
			var code = @"
using System;

public class MyClass
{
	public void MyMethod() { }
}";

			var document = AutoArrangeCodeRefactoringProviderTests.CreateDocument(code);

			var provider = new AutoArrangeCodeRefactoringProvider();
			var actions = (await provider.GetRefactoringsAsync(document,
				new TextSpan(30, 7), new CancellationToken(false))).ToList();
			Assert.AreEqual(0, actions.Count);
		}

		[TestMethod]
		public async Task RefactorWhenCodeNeedsRefactoring()
		{
			var code = @"
using System;

public class MyClass
{
	public void MyMethodB() { }
	public void MyMethodA() { }
}";

			var document = AutoArrangeCodeRefactoringProviderTests.CreateDocument(code);

			var provider = new AutoArrangeCodeRefactoringProvider();
			var actions = (await provider.GetRefactoringsAsync(document,
				new TextSpan(30, 7), new CancellationToken(false))).ToList();
			Assert.AreEqual(1, actions.Count);
		}

		internal static Document CreateDocument(string code)
		{
			var projectName = "Test";
			var projectId = ProjectId.CreateNewId(projectName);

			var solution = new CustomWorkspace()
				 .CurrentSolution
				 .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
				 .AddMetadataReference(projectId,
					new MetadataFileReference(typeof(object).Assembly.Location, MetadataImageKind.Assembly))
				 .AddMetadataReference(projectId,
					new MetadataFileReference(typeof(Enumerable).Assembly.Location, MetadataImageKind.Assembly))
				 .AddMetadataReference(projectId,
					new MetadataFileReference(typeof(CSharpCompilation).Assembly.Location, MetadataImageKind.Assembly))
				 .AddMetadataReference(projectId,
					new MetadataFileReference(typeof(Compilation).Assembly.Location, MetadataImageKind.Assembly));

			var documentId = DocumentId.CreateNewId(projectId);
			solution = solution.AddDocument(documentId, "Test.cs", SourceText.From(code));

			return solution.GetProject(projectId).Documents.First();
		}
	}
}
