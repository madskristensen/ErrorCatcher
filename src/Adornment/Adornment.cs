using EnvDTE;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
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
        private ItemControl _error, _warning, _info;
        private DTE _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;

        public Adornment(IWpfTextView view)
        {
            _view = view;

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

        public void Update(int error, int warning, int info)
        {
            if (_error == null)
                return;

            ThreadHelper.Generic.BeginInvoke(() =>
            {
                _error.Update(error);
                _warning.Update(warning);
                _info.Update(info);
            });
        }

        private void SetAdornmentLocation(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            Canvas.SetLeft(this, view.ViewportRight - 50);
            Canvas.SetTop(this, _view.ViewportTop + 20);
            Visibility = Visibility.Visible;
        }

    }
}
