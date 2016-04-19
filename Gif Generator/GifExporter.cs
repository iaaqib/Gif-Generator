using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using Lumia.InteropServices.WindowsRuntime;
using Windows.Storage;
using Lumia.Imaging.Compositing;
using Windows.UI.Popups;
namespace Gif_Generator
{
    class GifExporter
    {
        
        public static async Task Export(IReadOnlyList<IImageProvider> images, Rect? animatedArea, int Duration)
        {
           
            int Count = 0;
            ImageProviderInfo infoImageOne = await images[0].GetInfoAsync();
            // Getting Height and Width of First picked image as a reference point
            int w1 = (int)infoImageOne.ImageSize.Width;
            int h1 = (int)infoImageOne.ImageSize.Height;
            IReadOnlyList<IImageProvider> gifRendererSources;
            for (int i = 1; i < images.Count; i++) {
                ImageProviderInfo infoImageTwo = await images[i].GetInfoAsync();
                int w2 = (int)infoImageTwo.ImageSize.Width;
                int h2 = (int)infoImageTwo.ImageSize.Height;
                
                if (w1 != w2 && h1 != h2)
                {
                  Count++;

                }
            }
            if (Count == 1 || Count > 1) {

                 MessageBox.Show("Please Select Images of Same Dimensions");

            }
            else {

                gifRendererSources = images;



                //if (animatedArea.HasValue)
                //{
                // //    gifRendererSources = CreateFramedAnimation(images, animatedArea.Value, w, h);
                //}
                //else
                //{
                //      gifRendererSources = images;
                //}

                using (GifRenderer gifRenderer = new GifRenderer())
                {
                    try {
                       
                        gifRenderer.Duration = Duration;
                        gifRenderer.NumberOfAnimationLoops = 100;
                        gifRenderer.Sources = gifRendererSources;

                        var buffer = await gifRenderer.RenderAsync();

                        var filename = "GifCreator." + (await GetFileNameRunningNumber()) + ".gif";
                        var storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                        using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await stream.WriteAsync(buffer);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.ToString());
                    }
                }

                MessageBox.Show("Gif has been saved successfully in Gallery.");
               

            }

        }

        private static IReadOnlyList<IImageProvider> CreateFramedAnimation(IReadOnlyList<IImageProvider> images, Rect animationBounds, int w, int h)
        {
            List<IImageProvider> framedAnimation = new List<IImageProvider>();

            WriteableBitmap maskBitmap = new WriteableBitmap(w, h);

            var backgroundRectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Black),
                Width = w,
                Height = h,
            };

            maskBitmap.Render(backgroundRectangle, new TranslateTransform());

            var foregroundRectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.White),
                Width = animationBounds.Width,
                Height = animationBounds.Height,
            };

            TranslateTransform foregroundTranslate = new TranslateTransform
            {
                X = animationBounds.X,
                Y = animationBounds.Y
            };
            maskBitmap.Render(foregroundRectangle, foregroundTranslate);
            maskBitmap.Invalidate();

            foreach (IImageProvider frame in images)
            {
                FilterEffect filterEffect = new FilterEffect(images[0]);

                BlendFilter blendFilter = new BlendFilter(frame, BlendFunction.Normal, 1.0)
                {
                    MaskSource = new BitmapImageSource(maskBitmap.AsBitmap())
                };

                filterEffect.Filters = new List<IFilter>() { blendFilter };
                framedAnimation.Add(filterEffect);
            }

            return framedAnimation;
        }

        private static async Task<int> GetFileNameRunningNumber()
        {
            var files = await KnownFolders.PicturesLibrary.GetFilesAsync();
            int max = 0;
            foreach (StorageFile storageFile in files)
            {
                const string pattern = "GifCreator\\.\\d+\\.gif";
                if (System.Text.RegularExpressions.Regex.IsMatch(storageFile.Name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    max = Math.Max(max, Convert.ToInt32(storageFile.Name.Split('.')[1]));
                }
            }

            return max + 1;
        }

    }
}
