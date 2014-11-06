using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
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
			var actions = new List<CodeAction>();
			var actionRegistration = new Action<CodeAction>(action => actions.Add(action));
			var context = new CodeRefactoringContext(document, new TextSpan(55, 8), 
				actionRegistration, new CancellationToken(false));

			var provider = new AutoArrangeCodeRefactoringProvider();
			await provider.ComputeRefactoringsAsync(context);

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
			var actions = new List<CodeAction>();
			var actionRegistration = new Action<CodeAction>(action => actions.Add(action));
			var context = new CodeRefactoringContext(document, new TextSpan(30, 7),
				actionRegistration, new CancellationToken(false));

			var provider = new AutoArrangeCodeRefactoringProvider();
			await provider.ComputeRefactoringsAsync(context);
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
			var actions = new List<CodeAction>();
			var actionRegistration = new Action<CodeAction>(action => actions.Add(action));
			var context = new CodeRefactoringContext(document, new TextSpan(30, 7),
				actionRegistration, new CancellationToken(false));

			var provider = new AutoArrangeCodeRefactoringProvider();
			await provider.ComputeRefactoringsAsync(context);
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
					MetadataReference.CreateFromAssembly(typeof(object).Assembly))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromAssembly(typeof(CSharpCompilation).Assembly))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromAssembly(typeof(Compilation).Assembly));

			var documentId = DocumentId.CreateNewId(projectId);
			solution = solution.AddDocument(documentId, "Test.cs", SourceText.From(code));

			return solution.GetProject(projectId).Documents.First();
		}
	}
}
