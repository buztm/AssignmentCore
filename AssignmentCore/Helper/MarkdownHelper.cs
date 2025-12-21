using Ganss.Xss;
using Markdig;

namespace AssignmentCore.Helpers
{
    public static class MarkdownHelper
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        private static readonly HtmlSanitizer Sanitizer = new HtmlSanitizer();

        public static string ToSafeHtml(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return "";

            var html = Markdown.ToHtml(markdown, Pipeline);

            return Sanitizer.Sanitize(html);
        }
    }
}
