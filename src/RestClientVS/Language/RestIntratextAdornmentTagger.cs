using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS.Language
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(RestLanguage.LanguageName)]
    [TagType(typeof(IntraTextAdornmentTag))]
    internal sealed class RestIntratextAdornmentTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new RestIntratextAdornmentTagger(buffer)) as ITagger<T>;
    }

    internal class RestIntratextAdornmentTagger : ITagger<IntraTextAdornmentTag>, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private bool _isDisposed;
        private Document _doc;

        public RestIntratextAdornmentTagger(ITextBuffer buffer)
        {
            _buffer = buffer;

            ParseDocumentAsync().FireAndForget();

            if (buffer is ITextBuffer2 buffer2)
            {
                buffer2.ChangedOnBackground += bufferChanged;
            }
        }

        private void bufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ParseDocumentAsync().FireAndForget();
        }

        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _doc == null)
            {
                yield return null;
            }

            foreach (Request request in _doc.Requests.Where(r => r.Url.IsValid))
            {
                if (_buffer.CurrentSnapshot.Length >= request.Start)
                {
                    IntraTextAdornmentTag tag = new(CreateUiControl(), null, PositionAffinity.Successor);
                    ITextSnapshotLine line = _buffer.CurrentSnapshot.GetLineFromPosition(request.Start);
                    SnapshotSpan span = new(line.Snapshot, line.End, 0);

                    yield return new TagSpan<IntraTextAdornmentTag>(span, tag);
                }
            }
        }

        private FrameworkElement CreateUiControl()
        {
            FrameworkElement element = new Label
            {
                Content = "▶️",
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Green,
                Cursor = Cursors.Hand,
            };

            element.MouseLeftButtonUp += Element_MouseLeftButtonUp;

            return element;
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VS.Commands.ExecuteAsync(PackageGuids.RestClientVS, PackageIds.SendRequest).FireAndForget();
        }

        private Task ParseDocumentAsync()
        {
            _doc = _buffer.GetDocument();
            SnapshotSpan span = new(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_buffer is ITextBuffer2 buffer2)
                {
                    buffer2.ChangedOnBackground += bufferChanged;
                }
            }

            _isDisposed = true;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
