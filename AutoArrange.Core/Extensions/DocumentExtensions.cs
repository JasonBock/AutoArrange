using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoArrange.Core.Extensions
{
	public static class DocumentExtensions
	{
		public static async Task<Document> AutoArrangeAsync(this Document @this, CancellationToken cancellationToken)
		{
			if(Path.GetExtension(@this.FilePath) == ".cs")
			{
				var root = await @this.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

				var typeNodes = root.DescendantNodesAndSelf(_ => true)
					.Where(_ => _.IsKind(SyntaxKind.ClassDeclaration) || _.IsKind(SyntaxKind.StructDeclaration))
					.Cast<TypeDeclarationSyntax>();

				var newRoot = root;

				foreach (var typeNode in typeNodes)
				{
					var typeNodeCapture = new AutoArrangeCaptureWalker(typeNode);

					if (cancellationToken.IsCancellationRequested)
					{
						return @this;
					}

					var typeNodeResult = new AutoArrangeReplaceRewriter(
						typeNodeCapture).VisitTypeDeclaration(typeNode);

					if (cancellationToken.IsCancellationRequested)
					{
						return @this;
					}

					newRoot = newRoot.ReplaceNodes(new[] { typeNode },
						(a, b) => typeNodeResult);
				}

				return @this.WithSyntaxRoot(newRoot);
			}
			else
			{
				return @this;
			}
		}

		public static async Task<Document> AutoArrangeAsync(this Document @this,
			TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
		{
			if (Path.GetExtension(@this.FilePath) == ".cs")
			{
				var captureWalker = new AutoArrangeCaptureWalker(typeDeclaration);

				if (cancellationToken.IsCancellationRequested)
				{
					return @this;
				}

				var result = new AutoArrangeReplaceRewriter(
					captureWalker).VisitTypeDeclaration(typeDeclaration);

				if (cancellationToken.IsCancellationRequested)
				{
					return @this;
				}

				var root = await @this.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
				var newTree = root.ReplaceNodes(new[] { typeDeclaration },
					(a, b) => result);
				return @this.WithSyntaxRoot(newTree);
			}
			else
			{
				return @this;
			}
		}
	}
}