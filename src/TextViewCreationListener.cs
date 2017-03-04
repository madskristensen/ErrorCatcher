using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace ErrorCatcher
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    public class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        public ITextDocumentFactoryService DocumentService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (!DocumentService.TryGetTextDocument(textView.TextBuffer, out var doc) || ErrorProcessor.Instance == null)
                return;

            textView.Properties.AddProperty("filePath", doc.FilePath);
            textView.Closed += TextView_Closed;

            var adornment = textView.Properties.GetOrCreateSingletonProperty(() => new Adornment(textView));
            ErrorProcessor.Instance.Register(doc.FilePath, (result) => adornment.Update(result));
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;

            if (view == null)
                return;

            if (view.Properties.TryGetProperty(typeof(Adornment), out Adornment adornment))
            {
                adornment.Dispose();
            }

            if (view.Properties.TryGetProperty("filePath", out string filePath))
            {
                ErrorProcessor.Instance.Unregister(filePath);
            }
        }
    }
}
