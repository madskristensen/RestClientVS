using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text.Editor;
using RestClient.Client;
using RestClientVS.Margin;

namespace RestClientVS
{
    public class ResponseMargin : DockPanel, IWpfTextViewMargin
    {
        private readonly ITextView _textView;
        private bool _isDisposed;
        private bool _isLoaded;

        public ResponseMargin(ITextView textview)
        {
            _textView = textview;
        }

        public ResponseControl Control = new();

        public async Task UpdateReponseAsync(RequestResult result)
        {
            if (!_isLoaded)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                CreateRightMarginControls();
                _isLoaded = true;
            }

            await Control.SetResponseTextAsync(result);
        }

        private void CreateRightMarginControls()
        {
            var width = General.Instance.ResponseWindowWidth;

            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(width, GridUnitType.Pixel), MinWidth = 150 });
            grid.RowDefinitions.Add(new RowDefinition());
            Children.Add(grid);

            grid.Children.Add(Control);
            Grid.SetColumn(Control, 2);
            Grid.SetRow(Control, 0);

            GridSplitter splitter = new()
            {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += RightDragCompleted;

            grid.Children.Add(splitter);
            Grid.SetColumn(splitter, 1);
            Grid.SetRow(splitter, 0);

            Action fixWidth = new(() =>
            {
                // previewWindow maxWidth = current total width - textView minWidth
                var newWidth = (_textView.ViewportWidth + grid.ActualWidth) - 150;

                // preveiwWindow maxWidth < previewWindow minWidth
                if (newWidth < 150)
                {
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth != 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 0;
                        grid.ColumnDefinitions[2].MaxWidth = 0;
                    }
                }
                else
                {
                    grid.ColumnDefinitions[2].MaxWidth = newWidth;
                    // Call 'get before 'set for performance
                    if (grid.ColumnDefinitions[2].MinWidth == 0)
                    {
                        grid.ColumnDefinitions[2].MinWidth = 150;
                    }
                }
            });

            // Listen sizeChanged event of both marginGrid and textView
            grid.SizeChanged += (e, s) => fixWidth();
            _textView.ViewportWidthChanged += (e, s) => fixWidth();
        }

        private void RightDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!double.IsNaN(Control.ActualWidth))
            {
                General.Instance.ResponseWindowWidth = (int)Control.ActualWidth;
                General.Instance.Save();
            }
        }

        public FrameworkElement VisualElement => this;
        public double MarginSize => 0;
        public bool Enabled => true;

        public void Dispose()
        {
            if (!_isDisposed)
            {
            }

            _isDisposed = true;
        }

        public ITextViewMargin GetTextViewMargin(string marginName) => this;
    }
}
