using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using Tasks = System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using System.Linq;

namespace ErrorCatcher
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid("a6ea2ef8-a48a-4ebd-89d5-16b1ba16f5e3")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class ErrorCatcherPackage : AsyncPackage
    {
        private static Dictionary<string, Action<int, int, int>> _dic = new Dictionary<string, Action<int, int, int>>();
        private IWpfTableControl _table;

        public static ErrorCatcherPackage Instance { get; private set; }

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var errorList = await GetServiceAsync(typeof(SVsErrorList)) as IErrorList;
            _table = errorList.TableControl;
            errorList.TableControl.EntriesChanged += EntriesChanged;

            Instance = this;
        }

        public void Register(string fileName, Action<int, int, int> action)
        {
            _dic[fileName] = action;
            var entries = _table.Entries.ToArray();

            Tasks.Task.Run(() =>
            {
                UpdateFile(entries, fileName, out int error, out int warning, out int info);
                _dic[fileName].Invoke(error, warning, info);
            });
        }

        public void Unregister(string fileName)
        {
            if (_dic.ContainsKey(fileName))
                _dic.Remove(fileName);
        }

        private void EntriesChanged(object sender, EventArgs e)
        {
            var entries = _table.Entries.ToArray();

            Tasks.Task.Run(() =>
            {
                Update(entries);
            });
        }

        private void Update(ITableEntryHandle[] entries)
        {
            foreach (string file in _dic.Keys)
            {
                UpdateFile(entries, file, out int error, out int warning, out int info);

                _dic[file].Invoke(error, warning, info);
            }
        }

        private static void UpdateFile(ITableEntryHandle[] entries, string file, out int error, out int warning, out int info)
        {
            error = 0;
            warning = 0;
            info = 0;

            foreach (var entry in entries)
            {
                if (!entry.TryGetValue(StandardTableKeyNames.DocumentName, out string fileName) || fileName != file)
                    break;

                if (!entry.TryGetValue(StandardTableKeyNames.ErrorSeverity, out __VSERRORCATEGORY severity))
                    severity = __VSERRORCATEGORY.EC_MESSAGE;

                switch (severity)
                {
                    case __VSERRORCATEGORY.EC_ERROR:
                        error += 1;
                        break;
                    case __VSERRORCATEGORY.EC_WARNING:
                        warning += 1;
                        break;
                    default:
                        info += 1;
                        break;
                }
            }
        }
    }
}
