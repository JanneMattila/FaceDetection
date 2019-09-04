using ImageMagick;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaceBlur
{
    public class Program
    {
        /// <summary>
        /// Key from Face API resource (32 characters long).
        /// </summary>
        private const string SubscriptionKey = "123456abcdef123456abcdef123456ab";

        /// <summary>
        /// Endpoint address from Face API resource. 
        /// https://yourresourcename.cognitiveservices.azure.com
        /// Or in case of containers:
        /// http://localhost:5000
        /// </summary>
        private const string FaceEndpoint = "http://localhost:5000";

        public static async Task Main(string[] args)
        {
            var client = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey), new DelegatingHandler[] { })
            {
                Endpoint = FaceEndpoint
            };

            var fill = 1;
            var input = @"C:\temp\demoimage.jpg";
            var output = @"C:\temp\demoimage_output.jpg";

            try
            {
                IList<DetectedFace> faces;
                using (var stream = File.OpenRead(input))
                {
                    faces = await client.Face.DetectWithStreamAsync(stream);
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

        private static void SolidFill(IList<DetectedFace> faces, string input, string output)
        {
            var brush = new SolidBrush(Color.DarkGray);

            using (var bitmap = new Bitmap(input))
            using (var stream = File.OpenWrite(output))
            {
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
        }

        private static void BlurFill(IList<DetectedFace> faces, string input, string output)
        {
            using (var image = new MagickImage(input))
            {
                foreach (var face in faces)
                {
                    using (var part = image.Clone())
                    {
                        part.Crop(new MagickGeometry(
                            face.FaceRectangle.Left, face.FaceRectangle.Top,
                            face.FaceRectangle.Width, face.FaceRectangle.Height));
                        part.RePage();
                        part.Blur(0, 5);
                        image.Composite(part, face.FaceRectangle.Left, face.FaceRectangle.Top, CompositeOperator.Atop);
                    }
                }
                image.Write(output, MagickFormat.Png);
            }
        }
    }
}
