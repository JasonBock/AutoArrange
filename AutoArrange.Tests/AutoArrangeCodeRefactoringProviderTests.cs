﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoArrange.Tests
{
	public static class AutoArrangeCodeRefactoringProviderTests
	{
		[Test]
		public static async Task RefactorWhenTargetIsNotTypeDeclaration() => 
			await AutoArrangeCodeRefactoringProviderTests.TestProvider(
				@"Targets\TargetIsNotTypeDeclaration.cs", new TextSpan(30, 7), 0);

		[Test]
		public static async Task RefactorWhenCodeDoesNotNeedRefactoring() => 
			await AutoArrangeCodeRefactoringProviderTests.TestProvider(
				@"Targets\CodeDoesNotNeedRefactoring.cs", new TextSpan(30, 7), 0);

		[Test]
		public static async Task RefactorWhenCodeNeedsRefactoring() => 
			await AutoArrangeCodeRefactoringProviderTests.TestProvider(
				@"Targets\CodeNeedsRefactoring.cs", new TextSpan(30, 7), 1);

		private static async Task TestProvider(string file, TextSpan span, int expectedActionCount)
		{
			var code = File.ReadAllText(file);
			var document = AutoArrangeCodeRefactoringProviderTests.CreateDocument(code);
			var actions = new List<CodeAction>();
			var actionRegistration = new Action<CodeAction>(action => actions.Add(action));
			var context = new CodeRefactoringContext(document, span,
				actionRegistration, new CancellationToken(false));

			var provider = new AutoArrangeCodeRefactoringProvider();
			await provider.ComputeRefactoringsAsync(context);
			Assert.AreEqual(expectedActionCount, actions.Count);
		}

		internal static Document CreateDocument(string code)
		{
			var projectName = "Test";
			var projectId = ProjectId.CreateNewId(projectName);

			var solution = new AdhocWorkspace()
				 .CurrentSolution
				 .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location))
				 .AddMetadataReference(projectId,
					MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location));

			var documentId = DocumentId.CreateNewId(projectId);
			solution = solution.AddDocument(documentId, "Test.cs", SourceText.From(code));

			return solution.GetProject(projectId).Documents.First();
		}
	}
}