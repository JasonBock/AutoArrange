using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoArrange
{
	public sealed class AutoArrangeCaptureWalker
		: CSharpSyntaxWalker
	{
		public AutoArrangeCaptureWalker(TypeDeclarationSyntax node)
			: base()
		{
			this.Constructors =
				new List<ConstructorDeclarationSyntax>();
			this.Enums =
				new List<EnumDeclarationSyntax>();
			this.Events =
				new List<EventFieldDeclarationSyntax>();
			this.Fields =
				new List<FieldDeclarationSyntax>();
			this.Methods =
				new List<MethodDeclarationSyntax>();
			this.Properties =
				new List<PropertyDeclarationSyntax>();
			this.Types =
				new List<AutoArrangeCaptureWalker>();
			this.Target = node;

			if (node is ClassDeclarationSyntax classNode)
			{
				base.VisitClassDeclaration(classNode);
			}
			else
			{
				base.VisitStructDeclaration(
					node as StructDeclarationSyntax);
			}
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			var capture = new AutoArrangeCaptureWalker(node);
			this.Types.Add(capture);
		}

		public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => 
			this.Constructors.Add(node);

		public override void VisitEnumDeclaration(EnumDeclarationSyntax node) => 
			this.Enums.Add(node);

		public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => 
			this.Events.Add(node);

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node) => 
			this.Fields.Add(node);

		public override void VisitMethodDeclaration(MethodDeclarationSyntax node) => 
			this.Methods.Add(node);

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) => 
			this.Properties.Add(node);

		public override void VisitStructDeclaration(StructDeclarationSyntax node)
		{
			var capture = new AutoArrangeCaptureWalker(node);
			this.Types.Add(capture);
		}

		public List<ConstructorDeclarationSyntax> Constructors { get; private set; }
		public List<EnumDeclarationSyntax> Enums { get; private set; }
		public List<EventFieldDeclarationSyntax> Events { get; private set; }
		public List<FieldDeclarationSyntax> Fields { get; private set; }
		public List<PropertyDeclarationSyntax> Properties { get; private set; }
		public List<MethodDeclarationSyntax> Methods { get; private set; }
		public TypeDeclarationSyntax Target { get; private set; }
		public List<AutoArrangeCaptureWalker> Types { get; private set; }
	}
}