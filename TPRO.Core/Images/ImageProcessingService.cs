using System.Drawing;
using System.Drawing.Imaging;

namespace TPRO.Core.Images;

public class ImageProcessingService
{
    public byte[] ProcessImage(byte[] imageBytes, Func<Bitmap, Bitmap> processAction)
    {
        using var ms = new MemoryStream(imageBytes);
        using Image img = Image.FromStream(ms);
        using Bitmap originalBitmap = new Bitmap(img);
        Bitmap processedBitmap = processAction(originalBitmap);

        using var memoryStream = new MemoryStream();
        processedBitmap.Save(memoryStream, ImageFormat.Png);
        return memoryStream.ToArray();
    }
}