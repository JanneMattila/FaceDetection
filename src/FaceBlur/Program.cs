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
        private const string SubscriptionKey = "";
        private const string FaceEndpoint = "";

        public static async Task Main(string[] args)
        {
            var client = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey), new DelegatingHandler[] { })
            {
                Endpoint = FaceEndpoint
            };

            var input = @"C:\temp\demoimage.jpg";
            var output = @"C:\temp\demoimage_output.jpg";
            var brush = new SolidBrush(Color.DarkGray);

            try
            {
                IList<DetectedFace> faceList;
                using (var stream = File.OpenRead(input))
                {
                    faceList = await client.Face.DetectWithStreamAsync(stream);
                }

                using (var bitmap = new Bitmap(input))
                using (var stream = File.OpenWrite(output))
                {
                    var graphics = Graphics.FromImage(bitmap);
                    foreach (var face in faceList)
                    {
                        graphics.FillRectangle(brush, 
                            face.FaceRectangle.Left, face.FaceRectangle.Top,
                            face.FaceRectangle.Width, face.FaceRectangle.Height);
                    }
                    graphics.Save();
                    bitmap.Save(stream, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
