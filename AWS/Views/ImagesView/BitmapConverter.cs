using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Views.ImagesView
{
    public class ImagesViewWin
    {
       public static Bitmap LoadEmbeddedImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);
            foreach (var name in assembly.GetManifestResourceNames())
                Console.WriteLine(name);
            if (stream == null)
                throw new FileNotFoundException($"Ресурс не найден: {resourceName}");

            return new Bitmap(stream);
        }

    }
}
