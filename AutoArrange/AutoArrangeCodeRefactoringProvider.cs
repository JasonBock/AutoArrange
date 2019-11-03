using System.Composition;
using System.Threading.Tasks;
using AutoArrange.Core.Extensions;
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
		public const string RefactoringId = nameof(AutoArrangeCodeRefactoringProvider);

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			context.CancellationToken.ThrowIfCancellationRequested();

			var node = root.FindNode(context.Span);

			if (node is TypeDeclarationSyntax typeDeclaration)
			{
				var newDocument = await context.Document.AutoArrangeAsync(typeDeclaration, context.CancellationToken);

				if (newDocument != context.Document)
				{
					context.RegisterRefactoring(
						CodeAction.Create("Auto-arrange members in type",
							_ => Task.FromResult<Document>(newDocument)));
				}
			}
		}
	}
}