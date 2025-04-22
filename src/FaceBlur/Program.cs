using Azure;
using Azure.AI.Vision.Face;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace FaceBlur
{
    public class Program
    {
        /// <summary>
        /// Key from Face API resource.
        /// </summary>
        private const string SubscriptionKey = "long-key-here";

        /// <summary>
        /// Endpoint address from Face API resource. 
        /// https://yourresourcename.cognitiveservices.azure.com
        /// Or in case of containers:
        /// http://localhost:5000
        /// </summary>
        private const string FaceEndpoint = "https://yourresourcename.cognitiveservices.azure.com";

        public static async Task Main(string[] args)
        {
            var client = new FaceClient(new Uri(FaceEndpoint), new AzureKeyCredential(SubscriptionKey));

            var fill = 1;
            var input = @"C:\temp\source.jpg";
            var output = @"C:\temp\target.jpg";

            try
            {
                IReadOnlyList<FaceDetectionResult> faces;
                using (var stream = File.OpenRead(input))
                {
                    BinaryData imageData = BinaryData.FromStream(stream);
                    var response = await client.DetectAsync(imageData, FaceDetectionModel.Detection03, FaceRecognitionModel.Recognition04, false);
                    faces = response.Value;
                }

                switch (fill)
                {
                    case 1:
                        SolidFill(faces, input, output);
                        break;
                    case 2:
                        BlurFill(faces, input, output);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void SolidFill(IReadOnlyList<FaceDetectionResult> faces, string input, string output)
        {
            var brush = new SolidBrush(Color.DarkGray);

            using var bitmap = new Bitmap(input);
            using var stream = File.OpenWrite(output);
            var graphics = Graphics.FromImage(bitmap);
            foreach (var face in faces)
            {
                graphics.FillRectangle(brush,
                    face.FaceRectangle.Left, face.FaceRectangle.Top,
                    face.FaceRectangle.Width, face.FaceRectangle.Height);
            }
            graphics.Save();
            bitmap.Save(stream, ImageFormat.Png);
        }

        private static void BlurFill(IReadOnlyList<FaceDetectionResult> faces, string input, string output)
        {
            using var image = new MagickImage(input);
            foreach (var face in faces)
            {
                using var part = image.Clone();
                part.Crop(new MagickGeometry(
                    face.FaceRectangle.Left, face.FaceRectangle.Top,
                    (uint)face.FaceRectangle.Width, (uint)face.FaceRectangle.Height));
                part.ResetPage();
                part.Blur(0, 5);
                image.Composite(part, face.FaceRectangle.Left, face.FaceRectangle.Top, CompositeOperator.Atop);
            }
            image.Write(output, MagickFormat.Png);
        }
    }
}
