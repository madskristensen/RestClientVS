using System.Linq;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public class RestDocument : Document
    {
        private readonly ITextBuffer _buffer;

        public RestDocument(ITextBuffer buffer)
            : base(buffer.CurrentSnapshot.Lines.Select(line => line.GetTextIncludingLineBreak()).ToArray())
        {
            _buffer = buffer;
            _buffer.Changed += BufferChanged;
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
    }
}
