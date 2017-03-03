using EnvDTE;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ErrorCatcher
{

    class Adornment : StackPanel
    {
        private ITextView _view;
        private IErrorList _errorList;
        private ItemControl _error, _warning, _info;
        private string _fileName;
        private DTE _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;

        public Adornment(IWpfTextView view, IErrorList errorList, string fileName)
        {
            _view = view;
            _errorList = errorList;
            _fileName = fileName;

            Visibility = Visibility.Hidden;
            Orientation = Orientation.Vertical;
            Opacity = 0.6;
            Cursor = Cursors.Hand;
            ToolTip = "Click to show Error List";

            IAdornmentLayer adornmentLayer = view.GetAdornmentLayer(AdornmentLayer.LayerName);

            if (adornmentLayer.IsEmpty)
                adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, this, null);
        }

        protected override void OnInitialized(EventArgs e)
        {
            _error = new ItemControl(KnownMonikers.StatusError);
            _warning = new ItemControl(KnownMonikers.StatusWarning);
            _info = new ItemControl(KnownMonikers.StatusInformation);

            Children.Add(_error);
            Children.Add(_warning);
            Children.Add(_info);

            MouseLeftButtonUp += (snd, evt) => { _dte.ExecuteCommand("View.ErrorList"); };

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                SetAdornmentLocation(_view, EventArgs.Empty);

                _view.ViewportHeightChanged += SetAdornmentLocation;
                _view.ViewportWidthChanged += SetAdornmentLocation;
            }));
        }

        public void Update()
        {
            if (_error == null)
                return;

            int error = 0, warning = 0, info = 0;

            foreach (var entry in _errorList.TableControl.Entries)
            {
                if (!entry.TryGetValue(StandardTableKeyNames.DocumentName, out string fileName) || fileName != _fileName)
                    break;

                if (!entry.TryGetValue(StandardTableKeyNames.ErrorSeverity, out __VSERRORCATEGORY severity))
                    break;

                switch (severity)
                {
                    case __VSERRORCATEGORY.EC_ERROR:
                        error += 1;
                        break;
                    case __VSERRORCATEGORY.EC_WARNING:
                        warning += 1;
                        break;
                    case __VSERRORCATEGORY.EC_MESSAGE:
                        info += 1;
                        break;
                }
            }

            _error.Update(error);
            _warning.Update(warning);
            _info.Update(info);
        }

        private void SetAdornmentLocation(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            Canvas.SetLeft(this, view.ViewportRight - ActualWidth - 40);
            Canvas.SetTop(this, 10);
            Visibility = Visibility.Visible;
        }

    }
}
