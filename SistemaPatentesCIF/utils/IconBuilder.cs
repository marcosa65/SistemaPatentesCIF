using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SistemaPatentesCIF.utils
{
    public static class IconBuilder
    {
        public static void CreateMultiResIcon(string inputPngPath, string outputIcoPath)
        {
            using var original = new Bitmap(inputPngPath);
            using var transparentBmp = RemoveOuterBackground(original, tolerance: 28);

            int[] sizes = { 16, 32, 48, 64, 128, 256 };
            var pngFrames = new List<byte[]>();

            foreach (int size in sizes)
            {
                using var resized = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(transparentBmp, 0, 0, size, size);
                }

                using var ms = new MemoryStream();
                resized.Save(ms, ImageFormat.Png);
                pngFrames.Add(ms.ToArray());
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputIcoPath)!);
            using var fs = new FileStream(outputIcoPath, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            // Cabecera ICO (6 bytes)
            bw.Write((short)0);                 // Reservado
            bw.Write((short)1);                 // Tipo: 1 = ICO
            bw.Write((short)sizes.Length);      // Cantidad de resoluciones

            int offset = 6 + (16 * sizes.Length);

            // Entradas de directorio (16 bytes por resolución)
            for (int i = 0; i < sizes.Length; i++)
            {
                byte dim = sizes[i] == 256 ? (byte)0 : (byte)sizes[i];
                bw.Write(dim);                  // Ancho (0 = 256)
                bw.Write(dim);                  // Alto (0 = 256)
                bw.Write((byte)0);              // Paleta de colores
                bw.Write((byte)0);              // Reservado
                bw.Write((short)1);             // Planos de color
                bw.Write((short)32);            // Bits por píxel (ARGB)
                bw.Write(pngFrames[i].Length);  // Tamaño del frame PNG
                bw.Write(offset);               // Posición en el archivo

                offset += pngFrames[i].Length;
            }

            // Datos PNG de cada resolución
            foreach (var frame in pngFrames)
            {
                bw.Write(frame);
            }
        }

        private static Bitmap RemoveOuterBackground(Bitmap source, int tolerance)
        {
            var bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            int w = bmp.Width;
            int h = bmp.Height;
            Color bg = bmp.GetPixel(0, 0);
            var visited = new bool[w, h];
            var queue = new Queue<Point>();

            queue.Enqueue(new Point(0, 0));
            queue.Enqueue(new Point(w - 1, 0));
            queue.Enqueue(new Point(0, h - 1));
            queue.Enqueue(new Point(w - 1, h - 1));

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p.X < 0 || p.X >= w || p.Y < 0 || p.Y >= h || visited[p.X, p.Y])
                    continue;

                visited[p.X, p.Y] = true;
                Color c = bmp.GetPixel(p.X, p.Y);

                int diff = Math.Abs(c.R - bg.R) + Math.Abs(c.G - bg.G) + Math.Abs(c.B - bg.B);
                if (diff <= tolerance)
                {
                    bmp.SetPixel(p.X, p.Y, Color.Transparent);
                    queue.Enqueue(new Point(p.X + 1, p.Y));
                    queue.Enqueue(new Point(p.X - 1, p.Y));
                    queue.Enqueue(new Point(p.X, p.Y + 1));
                    queue.Enqueue(new Point(p.X, p.Y - 1));
                }
            }

            return bmp;
        }
    }
}