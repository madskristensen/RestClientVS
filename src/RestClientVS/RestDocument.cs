using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
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

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await Task.Yield();
                Project project = await VS.Solutions.GetActiveProjectAsync();
                ProjectName = project?.Name;
            }).FireAndForget();
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            UpdateLines(_buffer.CurrentSnapshot.Lines.Select(line => line.GetTextIncludingLineBreak()).ToArray());
            ParseAsync().FireAndForget();
        }

        private async Task ParseAsync()
        {
            await TaskScheduler.Default;
            Parse();
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
