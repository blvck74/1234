using System.Windows;
using System.Windows.Controls;
using Markdig;

namespace WpfLb1
{
    public partial class MarkdownViewer : UserControl
    {
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownViewer),
                new PropertyMetadata(string.Empty, OnMarkdownChanged));

        public string Markdown
        {
            get { return (string)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        public MarkdownViewer()
        {
            InitializeComponent();
        }

        private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer viewer)
            {
                viewer.RenderMarkdown();
            }
        }

        private void RenderMarkdown()
        {
            if (string.IsNullOrEmpty(Markdown))
            {
                Viewer.Document = new System.Windows.Documents.FlowDocument();
                return;
            }

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var xaml = Markdig.Wpf.Markdown.ToXaml(Markdown, pipeline);
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xaml)))
            {
                var doc = System.Windows.Markup.XamlReader.Load(stream) as System.Windows.Documents.FlowDocument;
                Viewer.Document = doc;
            }
        }
    }
}