using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoArrange
{
	[ExportCodeRefactoringProvider("AutoArrangeCodeRefactoringProvider", LanguageNames.CSharp)]
	public sealed class AutoArrangeCodeRefactoringProvider
		: ICodeRefactoringProvider
	{
		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(
			Document document, TextSpan span, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (cancellationToken.IsCancellationRequested)
			{
				return Enumerable.Empty<CodeAction>();
			}

			var node = root.FindNode(span);

			var typeDeclaration = node as TypeDeclarationSyntax;

			if (typeDeclaration != null)
			{
				var newDocument = await AutoArrangeCodeRefactoringProvider.AutoArrangeMembersInType(
					document, typeDeclaration, cancellationToken);

				if (newDocument != document)
				{
					return new[] { CodeAction.Create("Auto-arrange members in type", newDocument) };
				}
			}

			return Enumerable.Empty<CodeAction>();
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