using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        private IErrorList _errorList;
        private Adornment _adornment;

        [Import]
        public ITextDocumentFactoryService DocumentService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (!DocumentService.TryGetTextDocument(textView.TextBuffer, out var doc))
                return;

            if (ServiceProvider.GetService(typeof(SVsErrorList)) is IErrorList errorList)
            {
                _errorList = errorList;
                _adornment = textView.Properties.GetOrCreateSingletonProperty(() => new Adornment(textView, _errorList, doc.FilePath));

                _errorList.TableControl.EntriesChanged += OnErrorsUpdated;
                textView.Closed += TextView_Closed;
            }
        }

        private void OnErrorsUpdated(object sender, Microsoft.VisualStudio.Shell.TableControl.EntriesChangedEventArgs e)
        {
            if (_adornment != null)
                _adornment.Update();
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            if (_errorList != null)
                _errorList.TableControl.EntriesChanged -= OnErrorsUpdated;
        }
    }
}
