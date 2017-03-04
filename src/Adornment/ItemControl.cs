using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ErrorCatcher
{
    public class ItemControl : StackPanel, IDisposable
    {
        private TextBlock _text;
        private CheckBox _checkbox;
        public ImageMoniker _icon;
        private __VSERRORCATEGORY _category;
        private bool _isInEditMode;

        public ItemControl(ImageMoniker icon, __VSERRORCATEGORY category)
        {
            _icon = icon;
            _category = category;

            Orientation = Orientation.Horizontal;
            VerticalAlignment = VerticalAlignment.Center;
            Visibility = Visibility.Collapsed;

            Options.Saved += OptionsSaved;
        }

        private void OptionsSaved(object sender, EventArgs e)
        {
            if (!_isInEditMode)
            {
                bool isChecked = IsChecked();
                _checkbox.IsChecked = isChecked;
                Visibility = isChecked && _text.Text != "0" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            _checkbox = new CheckBox();
            _checkbox.IsChecked = IsChecked();
            _checkbox.Visibility = Visibility.Hidden;
            _checkbox.Padding = new Thickness(0, 0, 4, 0);
            _checkbox.Checked += CheckedChanged;
            _checkbox.Unchecked += CheckedChanged;
            Children.Add(_checkbox);

            var img = new Image();
            img.Source = ToBitmap(_icon, 14);
            img.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);

            var border = new Border();
            border.BorderBrush = Brushes.Transparent;
            border.BorderThickness = new Thickness(1);
            border.Child = img;
            Children.Add(border);

            _text = new TextBlock();
            _text.Width = 30;
            _text.Padding = new Thickness(4, 0, 0, 0);
            _text.SetResourceReference(Control.ForegroundProperty, VsBrushes.CaptionTextKey);
            _text.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Aliased);
            _text.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal);
            Children.Add(_text);
        }

        public void Update(int count)
        {
            _text.Text = count.ToString();
            Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public void EditMode(bool editable)
        {
            _isInEditMode = editable;
            _checkbox.Visibility = editable ? Visibility.Visible : Visibility.Hidden;

            if (editable)
            {
                Visibility = Visibility.Visible;

            }
            else if (!IsChecked() || _text.Text == "0")
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = _checkbox.IsChecked.HasValue && _checkbox.IsChecked.Value;

            switch (_category)
            {
                case __VSERRORCATEGORY.EC_ERROR:
                    ErrorCatcherPackage.Options.ShowErrors = isChecked;
                    break;
                case __VSERRORCATEGORY.EC_WARNING:
                    ErrorCatcherPackage.Options.ShowWarnings = isChecked;
                    break;
                case __VSERRORCATEGORY.EC_MESSAGE:
                    ErrorCatcherPackage.Options.ShowMessages = isChecked;
                    break;
            }

            ErrorCatcherPackage.Options.SaveSettingsToStorage();
        }

        private bool IsChecked()
        {
            switch (_category)
            {
                case __VSERRORCATEGORY.EC_ERROR:
                    return ErrorCatcherPackage.Options.ShowErrors;
                case __VSERRORCATEGORY.EC_WARNING:
                    return ErrorCatcherPackage.Options.ShowWarnings;
                case __VSERRORCATEGORY.EC_MESSAGE:
                    return ErrorCatcherPackage.Options.ShowMessages;
            }

            return false;
        }

        private static BitmapSource ToBitmap(ImageMoniker moniker, int size)
        {
            var shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell5;
            uint backgroundColor = VsColors.GetThemedColorRgba(shell, EnvironmentColors.BrandedUIBackgroundBrushKey);

            var imageAttributes = new ImageAttributes
            {
                Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags | unchecked((uint)_ImageAttributesFlags.IAF_Background),
                ImageType = (uint)_UIImageType.IT_Bitmap,
                Format = (uint)_UIDataFormat.DF_WPF,
                Dpi = 96,
                LogicalHeight = size,
                LogicalWidth = size,
                Background = backgroundColor,
                StructSize = Marshal.SizeOf(typeof(ImageAttributes))
            };

            var service = (IVsImageService2)Package.GetGlobalService(typeof(SVsImageService));
            IVsUIObject result = service.GetImage(moniker, imageAttributes);
            result.get_Data(out object data);

            return data as BitmapSource;
        }

        public void Dispose()
        {
            Options.Saved -= OptionsSaved;
            if (_checkbox != null)
            {
                _checkbox.Checked -= CheckedChanged;
                _checkbox.Unchecked -= CheckedChanged;
            }

            _checkbox = null;
            _text = null;

        }
    }
}
