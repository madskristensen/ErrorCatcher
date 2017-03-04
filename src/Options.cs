using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;

namespace ErrorCatcher
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Show Errors")]
        [DefaultValue(true)]
        public bool ShowErrors { get; set; } = true;

        [Category("General")]
        [DisplayName("Show Warnings")]
        [DefaultValue(true)]
        public bool ShowWarnings { get; set; } = true;

        [Category("General")]
        [DisplayName("Show Messages")]
        [DefaultValue(true)]
        public bool ShowMessages { get; set; } = true;

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();
            Saved?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler Saved;
    }
}
