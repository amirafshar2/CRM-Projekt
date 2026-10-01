// -----------------------------------------------------------------------------
// Bilder laden und speichern
//  - Laden ohne Dateisperre (Image.FromFile sperrt die Datei bis zum Schließen
//    des Programms – dann kann ein Bild nicht mehr ersetzt werden)
//  - Fehlende/defekte Dateien führen nicht mehr zum Absturz, es wird das
//    Platzhalterbild angezeigt
//  - Beim Speichern wird das Bild verkleinert und als JPEG komprimiert,
//    damit die Programm-Ordner klein bleiben (aus mehreren MB werden ~20–60 KB)
// -----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace çağdaşcivata
{
    public static class ImageHelper
    {
        /// <summary>Dateifilter für den Öffnen-Dialog.</summary>
        static readonly System.Collections.Generic.Dictionary<string, Tuple<DateTime, Image>> Cache =
            new System.Collections.Generic.Dictionary<string, Tuple<DateTime, Image>>(StringComparer.OrdinalIgnoreCase);

        public const string Filter = "Bilder (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

        /// <summary>Lädt ein Bild ohne die Datei zu sperren. Bei Fehlern wird <paramref name="fallback"/> zurückgegeben.</summary>
        public static Image Load(string path, Image fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return fallback;
                if (!File.Exists(path)) return fallback;
                // Zwischenspeicher: dieselbe Datei wird nur einmal gelesen (Produktlisten laden schneller)
                DateTime stamp = File.GetLastWriteTimeUtc(path);
                Tuple<DateTime, Image> hit;
                if (Cache.TryGetValue(path, out hit) && hit.Item1 == stamp) return hit.Item2;

                byte[] data = File.ReadAllBytes(path);
                if (data.Length == 0) return fallback; // früher wurden leere Bilddateien angelegt
                using (var ms = new MemoryStream(data))
                using (var img = Image.FromStream(ms))
                {
                    Image copy = new Bitmap(img); // Kopie -> Stream darf geschlossen werden
                    Cache[path] = Tuple.Create(stamp, copy);
                    return copy;
                }
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Speichert ein Bild verkleinert (längste Seite max. <paramref name="maxSize"/> px)
        /// als JPEG in "Programmordner\<paramref name="folder"/>\<paramref name="name"/>.jpg".
        /// Gibt den vollständigen Pfad zurück oder "" wenn kein Bild gespeichert werden konnte.
        /// </summary>
        public static string SaveCompressed(string sourceFile, string folder, string name, int maxSize = 600, long quality = 80)
        {
            if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile)) return "";
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
                Directory.CreateDirectory(dir);
                string target = Path.Combine(dir, SafeName(name) + ".jpg");

                using (Image src = LoadUncached(sourceFile))
                {
                    if (src == null) return "";
                    double scale = Math.Min(1.0, (double)maxSize / Math.Max(src.Width, src.Height));
                    int w = Math.Max(1, (int)Math.Round(src.Width * scale));
                    int h = Math.Max(1, (int)Math.Round(src.Height * scale));

                    using (var bmp = new Bitmap(w, h))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.White); // transparente PNGs bekommen weißen Hintergrund
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(src, 0, 0, w, h);
                        }

                        ImageCodecInfo jpeg = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        using (var p = new EncoderParameters(1))
                        {
                            p.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                            if (File.Exists(target)) File.Delete(target);
                            bmp.Save(target, jpeg, p);
                        }
                    }
                }
                return target;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Das Bild konnte nicht gespeichert werden.\n" + e.Message, "Hinweis");
                return "";
            }
        }

        static Image LoadUncached(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            if (data.Length == 0) return null;
            using (var ms = new MemoryStream(data))
            using (var img = Image.FromStream(ms))
                return new Bitmap(img);
        }

        static string SafeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "bild";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Replace(' ', '_');
        }
    }
}
