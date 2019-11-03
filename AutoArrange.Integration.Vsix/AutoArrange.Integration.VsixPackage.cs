using AutoArrange.Core.Extensions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;

namespace AutoArrange.Integration.Vsix
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[Guid(AutoArrangePackage.PackageGuidString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class AutoArrangePackage
		: AsyncPackage
	{
		public const string PackageGuidString = "b90a8d3d-5382-4e7e-ad03-a70c59321cbf";
		private VisualStudioWorkspace workspace;
		private DTE dte;
		private DocumentEventsClass documentEvents;

		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			var model = await this.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
			this.workspace = model.GetService<VisualStudioWorkspace>();

			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			this.dte = await this.GetServiceAsync(typeof(DTE)) as DTE;
			this.documentEvents = this.dte.Events.DocumentEvents as DocumentEventsClass;
			this.documentEvents.DocumentSaved += this.OnDocumentSaved;
		}

		protected override void Dispose(bool disposing)
		{
			this.documentEvents.DocumentSaved -= this.OnDocumentSaved;
			base.Dispose(disposing);
		}

		private async void OnDocumentSaved(EnvDTE.Document dteDocument)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(this.DisposalToken);

			var documentIds = this.workspace.CurrentSolution.GetDocumentIdsWithFilePath(
				dteDocument.FullName);

			if (documentIds != null && documentIds.Length == 1)
			{
				var documentId = documentIds[0];
				var document = this.workspace.CurrentSolution.GetDocument(documentId);

				if (Path.GetExtension(document.FilePath) == ".cs")
				{
					var root = await document.GetSyntaxRootAsync(default);
					var newDocument = await document.AutoArrangeAsync(default);
					var newRoot = await newDocument.GetSyntaxRootAsync(default);

					if (newRoot != root)
					{
						var newSolution = document.Project.Solution
							.WithDocumentSyntaxRoot(document.Id, newRoot);
						this.workspace.TryApplyChanges(newSolution);
						dteDocument.Save();
					}
				}
			}
		}
	}
}