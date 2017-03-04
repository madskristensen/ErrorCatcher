using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ErrorCatcher
{

    class Adornment : StackPanel, IDisposable
    {
        private ITextView _view;
        private ItemControl _error, _warning, _info;
        private static DTE2 _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        public Adornment(IWpfTextView view)
        {
            _view = view;

            Visibility = Visibility.Hidden;
            Orientation = Orientation.Vertical;
            Opacity = 0.6;
            Cursor = Cursors.Hand;

            IAdornmentLayer adornmentLayer = view.GetAdornmentLayer(AdornmentLayer.LayerName);

            if (adornmentLayer.IsEmpty)
                adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, this, null);
        }

        protected override void OnInitialized(EventArgs e)
        {
            _error = new ItemControl(KnownMonikers.StatusError, __VSERRORCATEGORY.EC_ERROR);
            _warning = new ItemControl(KnownMonikers.StatusWarning, __VSERRORCATEGORY.EC_WARNING);
            _info = new ItemControl(KnownMonikers.StatusInformation, __VSERRORCATEGORY.EC_MESSAGE);

            Children.Add(_error);
            Children.Add(_warning);
            Children.Add(_info);

            MouseLeftButtonUp += (snd, evt) => { _dte.ExecuteCommand("View.ErrorList"); evt.Handled = true; };
            MouseRightButtonUp += (snd, evt) => { EnterEditMode(true); evt.Handled = true; };
            MouseLeave += (snd, evt) => { EnterEditMode(false); };

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                SetAdornmentLocation(_view, EventArgs.Empty);

                _view.ViewportHeightChanged += SetAdornmentLocation;
                _view.ViewportWidthChanged += SetAdornmentLocation;
            }));
        }

        private void EnterEditMode(bool editable)
        {
            _error.EditMode(editable);
            _warning.EditMode(editable);
            _info.EditMode(editable);
        }

        public void Update(ErrorResult result)
        {
            if (_error == null)
                return;

            ThreadHelper.Generic.BeginInvoke(() =>
            {
                _error.Update(result.Errors);
                _warning.Update(result.Warnings);
                _info.Update(result.Info);
            });
        }

        private void SetAdornmentLocation(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            Canvas.SetLeft(this, view.ViewportRight - 60);
            Canvas.SetTop(this, _view.ViewportTop + 20);
            Visibility = Visibility.Visible;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.ViewportHeightChanged -= SetAdornmentLocation;
                _view.ViewportWidthChanged -= SetAdornmentLocation;
            }

            _error.Dispose();
            _warning.Dispose();
            _info.Dispose();
        }
    }
}
