using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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

            var image = @"C:\temp\demoimage.jpg";
            try
            {
                IList<DetectedFace> faceList;
                using (var stream = File.OpenRead(image))
                {
                    faceList = await client.Face.DetectWithStreamAsync(stream);
                }

                using (var bitmap = new Bitmap(image))
                using (var stream = new MemoryStream())
                {
                    var graphics = Graphics.FromImage(bitmap);
                    foreach (var face in faceList)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
