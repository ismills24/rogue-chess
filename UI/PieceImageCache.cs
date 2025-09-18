using Svg;

namespace RogueChess.UI;

public static class PieceImageCache
{
    private static readonly Dictionary<string, Svg.SvgDocument> _svgDocs = new();

    public static Bitmap? RenderSvg(string key, int size)
    {
        if (!_svgDocs.TryGetValue(key, out var doc))
        {
            var svgPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "..",
                "..",
                "assets",
                "svg",
                $"{key}.svg"
            );
            if (!File.Exists(svgPath))
                return null;

            doc = Svg.SvgDocument.Open(svgPath);
            _svgDocs[key] = doc;
        }

        return doc.Draw(size, size);
    }
}
