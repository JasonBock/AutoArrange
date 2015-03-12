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
					context.RegisterRefactoring(
						CodeAction.Create("Auto-arrange members in type",
							_ => Task.FromResult<Document>(newDocument)));
				}
			}

			return;
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
	}
}