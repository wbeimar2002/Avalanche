namespace Avalanche.Shared.Domain.Models.Media
{
    public class TileViewportModel
    {
        public int Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TileViewportModel()
        {
        }

        public TileViewportModel(int layer, int x, int y, int width, int height)
        {
            Layer = layer;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

    }
}
