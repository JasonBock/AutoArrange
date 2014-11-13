using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoArrange
{
	[ExportCodeRefactoringProvider(AutoArrangeCodeRefactoringProvider.RefactoringId, LanguageNames.CSharp)]
	[Shared]
	public sealed class AutoArrangeCodeRefactoringProvider 
		: CodeRefactoringProvider
	{
		public const string RefactoringId = "AutoArrangeCodeRefactoringProvider";

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			var node = root.FindNode(context.Span);

			var typeDeclaration = node as TypeDeclarationSyntax;

			if (typeDeclaration != null)
			{
				var newDocument = await AutoArrangeCodeRefactoringProvider.AutoArrangeMembersInType(
					context.Document, typeDeclaration, context.CancellationToken);

				if (newDocument != context.Document)
				{
					//return new[] { CodeAction.Create("Auto-arrange members in type", newDocument) };
					context.RegisterRefactoring(
						CodeAction.Create("Auto-arrange members in type", newDocument));
				}
			}

			return;

			//// TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

			//var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			//// Find the node at the selection.
			//var node = root.FindNode(context.Span);

			//// Only offer a refactoring if the selected node is a type declaration node.
			//var typeDecl = node as TypeDeclarationSyntax;
			//if (typeDecl == null)
			//{
			//	return;
			//}

			//// For any type declaration node, create a code action to reverse the identifier text.
			//var action = CodeAction.Create("Reverse type name", c => ReverseTypeNameAsync(context.Document, typeDecl, c));

			//// Register this code action.
			//context.RegisterRefactoring(action);
		}

		private static async Task<Document> AutoArrangeMembersInType(Document document,
			TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
		{
			var captureWalker = new AutoArrangeCaptureWalker(typeDeclaration);

			if (cancellationToken.IsCancellationRequested)
			{
				return document;
			}

			var result = new AutoArrangeReplaceRewriter(
				captureWalker).VisitTypeDeclaration(typeDeclaration);

			if (cancellationToken.IsCancellationRequested)
			{
				return document;
			}

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newTree = root.ReplaceNodes(new[] { typeDeclaration },
				(a, b) => result);
			return document.WithSyntaxRoot(newTree);
		}


		//private async Task<Solution> ReverseTypeNameAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
		//{
		//	// Produce a reversed version of the type declaration's identifier token.
		//	var identifierToken = typeDecl.Identifier;
		//	var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());

		//	// Get the symbol representing the type to be renamed.
		//	var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
		//	var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

		//	// Produce a new solution that has all references to that type renamed, including the declaration.
		//	var originalSolution = document.Project.Solution;
		//	var optionSet = originalSolution.Workspace.Options;
		//	var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

		//	// Return the new solution with the now-uppercase type name.
		//	return newSolution;
		//}
	}
}