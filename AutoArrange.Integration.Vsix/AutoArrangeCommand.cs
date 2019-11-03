using AutoArrange.Core.Extensions;
using System;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AutoArrange.Integration.Vsix
{
	internal sealed class AutoArrangeCommand
	{
		public const int CommandId = 0x0100;
		public static readonly Guid CommandSet = new Guid("c03bae4f-fe26-48d8-88ed-60427706dd4d");
		private readonly AsyncPackage package;

		private AutoArrangeCommand(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.ExecuteAsync, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			AutoArrangeCommand.Instance = new AutoArrangeCommand(package, commandService);
		}

		private async void ExecuteAsync(object sender, EventArgs e)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			var dte = await this.package.GetServiceAsync(typeof(DTE)) as DTE;
			
			if (dte.ActiveSolutionProjects is Array projects && projects.Length != 0)
			{
				var project = projects.GetValue(0) as Project;

				var model = await this.package.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
				var workspace = model.GetService<VisualStudioWorkspace>();

				var currentProject = workspace.CurrentSolution.Projects
					.SingleOrDefault(_ => _.FilePath == project.FileName);

				if(currentProject != null)
				{
					foreach(var document in currentProject.Documents)
					{
						var root = await document.GetSyntaxRootAsync(default);
						var newDocument = await document.AutoArrangeAsync(default);
						var newRoot = await newDocument.GetSyntaxRootAsync(default);

						if (newRoot != root)
						{
							var newSolution = document.Project.Solution
								.WithDocumentSyntaxRoot(document.Id, newRoot);
							workspace.TryApplyChanges(newSolution);

							foreach(var dteDocumentObject in dte.Documents)
							{
								var dteDocument = (Document)dteDocumentObject;

								if (dteDocument.FullName == document.FilePath)
								{
									dteDocument.Save();
									break;
								}
							}
						}
					}
				}
			}
		}

		public static AutoArrangeCommand Instance
		{
			get;
			private set;
		}
	}
}