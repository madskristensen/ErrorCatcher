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
        private static Dictionary<string, Action<ErrorResult>> _dic = new Dictionary<string, Action<ErrorResult>>();
        private IWpfTableControl _table;

        public static ErrorCatcherPackage Instance { get; private set; }

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var errorList = await GetServiceAsync(typeof(SVsErrorList)) as IErrorList;
            _table = errorList.TableControl;
            errorList.TableControl.EntriesChanged += EntriesChanged;

            Instance = this;
        }

        public void Register(string fileName, Action<ErrorResult> action)
        {
            _dic[fileName] = action;
            var entries = _table.Entries.ToArray();

            Tasks.Task.Run(() =>
            {
                Update(entries);
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
            var errors = GetErrors(entries);

            foreach (string file in _dic.Keys)
            {
                var error = errors.FirstOrDefault(e => e.FileName == file) ?? new ErrorResult(file);
                _dic[file].Invoke(error);
            }
        }

        private static IEnumerable<ErrorResult> GetErrors(ITableEntryHandle[] entries)
        {
            var list = new Dictionary<string, ErrorResult>();

            try
            {
                foreach (var entry in entries)
                {
                    if (!entry.TryGetValue(StandardTableKeyNames.DocumentName, out string fileName) || !_dic.ContainsKey(fileName))
                        break;

                    if (!entry.TryGetValue(StandardTableKeyNames.ErrorSeverity, out __VSERRORCATEGORY severity))
                        severity = __VSERRORCATEGORY.EC_MESSAGE;

                    if (!list.ContainsKey(fileName))
                        list.Add(fileName, new ErrorResult(fileName));

                    switch (severity)
                    {
                        case __VSERRORCATEGORY.EC_ERROR:
                            list[fileName].Errors += 1;
                            break;
                        case __VSERRORCATEGORY.EC_WARNING:
                            list[fileName].Warnings += 1;
                            break;
                        default:
                            list[fileName].Info += 1;
                            break;
                    }
                }

                return list.Values;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return null;
            }
        }
    }
}
