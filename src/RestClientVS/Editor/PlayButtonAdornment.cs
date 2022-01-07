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
    [ContentType(LanguageFactory.LanguageName)]
    [TagType(typeof(IntraTextAdornmentTag))]
    internal sealed class RestIntratextAdornmentTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new PlayButtonAdornment(buffer)) as ITagger<T>;
    }

    internal class PlayButtonAdornment : ITagger<IntraTextAdornmentTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly RestDocument _document;

        public PlayButtonAdornment(ITextBuffer buffer)
        {
            _buffer = buffer;
            _document = buffer.GetRestDocument();
        }

        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _document.IsParsing)
            {
                yield return null;
            }

            foreach (Request request in _document.Requests.Where(r => r.Url.IsValid))
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
                Padding = new Thickness(0),
                Margin = new Thickness(4, -2, 0, 0),
            };

            element.MouseLeftButtonUp += Element_MouseLeftButtonUp;

            return element;
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VS.Commands.ExecuteAsync(PackageGuids.RestClientVS, PackageIds.SendRequest).FireAndForget();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
