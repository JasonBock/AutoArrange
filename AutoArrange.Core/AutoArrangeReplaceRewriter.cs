using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoArrange
{
	public sealed class AutoArrangeReplaceRewriter
		: CSharpSyntaxRewriter
	{
		private int count;
		private readonly List<SyntaxNode> nodes;

		public AutoArrangeReplaceRewriter(
			AutoArrangeCaptureWalker walker)
		{
			walker.Constructors.Sort(
				(a, b) => a.Identifier.ValueText.CompareTo(
					b.Identifier.ValueText));
			walker.Enums.Sort(
				(a, b) => a.Identifier.ValueText.CompareTo(
					b.Identifier.ValueText));
			walker.Events.Sort(
				(a, b) => a.Declaration.Variables[0].Identifier.ValueText.CompareTo(
					b.Declaration.Variables[0].Identifier.ValueText));
			walker.Fields.Sort(
				(a, b) => a.Declaration.Variables[0]
					.Identifier.ValueText.CompareTo(
						b.Declaration.Variables[0]
							.Identifier.ValueText));
			walker.Methods.Sort(
				(a, b) => a.Identifier.ValueText.CompareTo(
					b.Identifier.ValueText));
			walker.Properties.Sort(
				(a, b) => a.Identifier.ValueText.CompareTo(
					b.Identifier.ValueText));
			walker.Types.Sort(
				(a, b) => a.Target.Identifier.ValueText.CompareTo(
					b.Target.Identifier.ValueText));

			this.nodes = new List<SyntaxNode>();
			this.nodes.AddRange(walker.Events);
			this.nodes.AddRange(walker.Fields);
			this.nodes.AddRange(walker.Constructors);
			this.nodes.AddRange(walker.Methods);
			this.nodes.AddRange(walker.Properties);
			this.nodes.AddRange(walker.Enums);

			this.nodes.AddRange(
				from typeRewriter in walker.Types
				select new AutoArrangeReplaceRewriter(typeRewriter)
					.VisitTypeDeclaration(typeRewriter.Target)
						as TypeDeclarationSyntax);
		}

		private SyntaxNode Replace(SyntaxNode node)
		{
			SyntaxNode result = null;

			if (this.count < this.nodes.Count)
			{
				result = this.nodes[this.count];
				this.count++;
			}
			else
			{
				throw new NotSupportedException();
			}

			return result;
		}

		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) => 
			this.Replace(node);

		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) =>
			this.Replace(node);

		public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node) => 
			this.Replace(node);

		public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) =>
			this.Replace(node);

		public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node) =>
			this.Replace(node);

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) =>
			this.Replace(node);

		public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) =>
			this.Replace(node);

		public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node) =>
			this.Replace(node);

		public TypeDeclarationSyntax VisitTypeDeclaration(
			TypeDeclarationSyntax node)
		{
			if (node is ClassDeclarationSyntax classNode)
			{
				return base.VisitClassDeclaration(classNode)
					as ClassDeclarationSyntax;
			}
			else
			{
				return base.VisitStructDeclaration(
					node as StructDeclarationSyntax)
					as StructDeclarationSyntax;
			}
		}
	}
}