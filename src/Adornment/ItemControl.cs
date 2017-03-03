using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ErrorCatcher
{
    public class ItemControl : StackPanel
    {
        private TextBlock _text;

        public ItemControl(ImageMoniker moniker)
        {
            Icon = moniker;
            Orientation = Orientation.Horizontal;
            Visibility = Visibility.Collapsed;
        }

        public ImageMoniker Icon { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            var img = new Image();
            img.Source = ToBitmap(Icon, 14);
            img.Margin = new Thickness(0, 0, 4, 3);
            img.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
            Children.Add(img);

            _text = new TextBlock();
            _text.Width = 30;
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

    }
}
