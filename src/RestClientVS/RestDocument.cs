using System.Linq;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public class RestDocument : Document, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private bool _isDisposed;

        public string FileName { get; }
        public string ProjectName { get; private set; }

        public RestDocument(ITextBuffer buffer)
            : base(buffer.CurrentSnapshot.Lines.Select(line => line.GetTextIncludingLineBreak()).ToArray())
        {
            _buffer = buffer;
            _buffer.Changed += BufferChanged;

            FileName = buffer.GetFileName();

#pragma warning disable VSTHRD104 // Offer async methods
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                Project project = await VS.Solutions.GetActiveProjectAsync();
                ProjectName = project?.Name;
            });
#pragma warning restore VSTHRD104 // Offer async methods
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            UpdateLines(_buffer.CurrentSnapshot.Lines.Select(line => line.GetTextIncludingLineBreak()).ToArray());
            ParseAsync().FireAndForget();
        }

        public static RestDocument FromTextbuffer(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new RestDocument(buffer));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _buffer.Changed -= BufferChanged;
            }

            _isDisposed = true;
        }
    }
}
