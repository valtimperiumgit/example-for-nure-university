using System.Drawing;
using System.Drawing.Imaging;

namespace TPRO.Core.Images;

public static class ImageChanger
{
    public static Bitmap MakeImageBlackAndWhite(Bitmap original)
    {
        Bitmap newBitmap = new Bitmap(original.Width, original.Height);

        Graphics g = Graphics.FromImage(newBitmap);
        
        ColorMatrix colorMatrix = new ColorMatrix(
            new []
            {
                new [] {.3f, .3f, .3f, 0, 0},
                new [] {.59f, .59f, .59f, 0, 0},
                new [] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

        ImageAttributes attributes = new ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);

        var rectangle = new Rectangle(0, 0, original.Width, original.Height);
        const int srcX = 0;
        const int srcY = 0;
        
        g.DrawImage(original, rectangle, srcX, srcY, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        g.Dispose();

        return newBitmap;
    }
    
    public static Bitmap DetectEdges(Bitmap original)
    {
        Bitmap edgeBitmap = new Bitmap(original.Width, original.Height);

        for (int x = 1; x < original.Width - 1; x++)
        {
            for (int y = 1; y < original.Height - 1; y++)
            {
                Color prevX = original.GetPixel(x - 1, y);
                Color nextX = original.GetPixel(x + 1, y);
                Color prevY = original.GetPixel(x, y - 1);
                Color nextY = original.GetPixel(x, y + 1);

                int diffX = Math.Abs(prevX.R - nextX.R) + Math.Abs(prevX.G - nextX.G) + Math.Abs(prevX.B - nextX.B);
                int diffY = Math.Abs(prevY.R - nextY.R) + Math.Abs(prevY.G - nextY.G) + Math.Abs(prevY.B - nextY.B);

                int diff = (diffX + diffY) / 2;
                diff = Math.Clamp(diff, 0, 255);

                edgeBitmap.SetPixel(x, y, Color.FromArgb(diff, diff, diff));
            }
        }

        return edgeBitmap;
    }
    
    public static Bitmap RotateImage(Bitmap original)
    {
        original.RotateFlip(RotateFlipType.Rotate180FlipNone);
        return original;
    }
}