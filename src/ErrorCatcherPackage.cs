using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Tasks = System.Threading.Tasks;

namespace ErrorCatcher
{
    [Guid("a6ea2ef8-a48a-4ebd-89d5-16b1ba16f5e3")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)] // Load when any document opens
    [ProvideOptionPage(typeof(Options), Vsix.Name, "General", 0, 0, true, new string[0])]
    public sealed class ErrorCatcherPackage : AsyncPackage
    {
        public static Options Options
        {
            get;
            private set;
        }

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var errorList = await GetServiceAsync(typeof(SVsErrorList)) as IErrorList;
            
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            Options = (Options)GetDialogPage(typeof(Options));

            ErrorProcessor.Initialize(errorList);
        }
    }
}
