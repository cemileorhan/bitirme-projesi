using SkiaSharp;
using QRCoder;

public static class SkiaSharpQRCodeHelper
{
    public static byte[] GenerateQrCode(string content)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        using (SKBitmap bitmap = Render(qrCodeData))
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                return stream.ToArray();
            }
        }
    }

    private static SKBitmap Render(QRCodeData qrCodeData)
    {
        var pixels = qrCodeData.ModuleMatrix;
        int size = pixels.Count;
        int scale = 10;
        SKBitmap bitmap = new SKBitmap(size * scale, size * scale);

        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);

            using (var paint = new SKPaint { Color = SKColors.Black })
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        if (pixels[x][y])
                        {
                            canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                        }
                    }
                }
            }
        }

        return bitmap;
    }
}
