using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;

namespace TextViewer
{
    public class ImageParser : IDisposable
    {
        [SupportedOSPlatform("windows")]

        private Bitmap? bmp = null;

        public ImageParser(Image image)
        {
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                bmp = (Bitmap)image;
            }
        }

        public ImageParser(string filePath)
        {
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                if (filePath == null)
                {

                    Debug.WriteLine("WriteImageToConsole error: image path is null");
                    return;
                }

                filePath = Path.GetFullPath(filePath);

                if (File.Exists(filePath) == false)
                {
                    Debug.WriteLine($"WriteImageToConsole error: file does not exist: {filePath}");
                    return;
                }

                bmp = (Bitmap)Image.FromFile(filePath);
            }
        }

        public void WriteImageToConsole(int startx, int starty, int width, int height, bool scaleToFit)
        {
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                if (bmp == null)
                {
                    Debug.WriteLine($"WriteImageToConsole error: bitmap is null");
                    return;
                }

                int finalWidth = bmp.Width;
                int finalHeight = bmp.Height;


                if (scaleToFit)
                {
                    float ratioX = (float)width / bmp.Width;
                    float ratioY = (float)height / bmp.Height;
                    float ratioFinal = Math.Min(ratioX, ratioY);
                    finalWidth = (int)(bmp.Width * ratioFinal);
                    finalHeight = (int)(bmp.Height * ratioFinal);
                    Debug.WriteLine($"Img {bmp.Width}x{bmp.Height}, frame {width}x{height} Ratios: x {ratioX} y {ratioY}, using {ratioFinal}");
                    Debug.WriteLine($"final {finalWidth} {finalHeight}");
                }

                using (Bitmap bitmap = new Bitmap(bmp, finalWidth, finalHeight))
                {
                    for (int y = 0; y < height - 1 && y < bitmap.Height; y++)
                    {
                        Console.SetCursorPosition(startx, starty + y);
                        for (int x = 0; x < width - 1 && x < bitmap.Width; x++)
                        {
                            Console.Write(ColorChar('█', bitmap.GetPixel(x, y)));
                        }
                        //Console.WriteLine();
                    }
                    Console.ResetColor();
                }
            }
        }

        public static string ColorChar(char symbol, Color color)
        {
            return $"\x1B[38;2;{color.R};{color.G};{color.B}m{symbol}";
        }

        public void Dispose()
        {
            if (OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }
        }
    }
}
