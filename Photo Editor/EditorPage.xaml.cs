using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Photo_Editor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditorPage : Page
    {
        private StorageFile imageFile;
        private CanvasBitmap bitmap;
        private ICanvasImage effect;

        public EditorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args.Parameter != null && args.Parameter is StorageFile imageFile)
                this.imageFile = imageFile;
        }

        private void CanvasControl_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CanvasControl_CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CanvasControl_CreateResourcesAsync(CanvasControl sender)
        {
            using IRandomAccessStream stream = await imageFile.OpenReadAsync();
            bitmap = await CanvasBitmap.LoadAsync(sender, stream);

            CreateEffect();
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            CanvasDrawingSession ds = args.DrawingSession;

            /// Scale image
            double canvasGridHeight = canvasGrid.ActualHeight;
            double canvasGridWidth = canvasGrid.ActualWidth;
            double imageHeight = bitmap.Size.Height;
            double imageWidth = bitmap.Size.Width;
            double scaleFactor = 0;

            if (canvasGridHeight < (imageHeight / 0.9))
            {
                double scaledHeight = canvasGridHeight * 0.9;
                scaleFactor = scaledHeight / imageHeight;
            }

            if (canvasGridWidth < (imageWidth / 0.9))
            {
                double scaledWidth = canvasGridWidth * 0.9;

                if (scaleFactor == 0)
                    scaleFactor = scaledWidth / imageWidth;
                else
                    scaleFactor = Math.Min(scaleFactor, scaledWidth / imageWidth);
            }

            if (scaleFactor > 0)
            {
                sender.Width = imageWidth * scaleFactor;
                sender.Height = imageHeight * scaleFactor;
                ds.Transform = Matrix3x2.CreateScale((float)scaleFactor);
            }
            else
            {
                sender.Height = imageHeight;
                sender.Width = imageWidth;
            }

            if (effect == null)
                ds.DrawImage(bitmap);
            else
                ds.DrawImage(effect);

            sender.Invalidate();
        }

        double brightnessVal = 0;
        double exposureVal = 0;
        double contrastVal = 0;
        double highlightsVal = 0;
        double shadowsVal = 0;

        private void CreateEffect()
        {
            effect = bitmap;

            if (brightnessVal != 0)
                effect = CreateBrightnessEffect(effect);
            if (exposureVal != 0)
                effect = CreateExposureEffect(effect);
            if (contrastVal != 0)
                effect = CreateContrastEffect(effect);
            if (highlightsVal != 0 || shadowsVal != 0)
                effect = CreateHighlightsAndShadowsEffect(effect);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                if (slider == brightnessSlider)
                    brightnessVal = brightnessSlider.Value;
                else if (slider == exposureSlider)
                    exposureVal = exposureSlider.Value;
                else if (slider == contrastSlider)
                    contrastVal = contrastSlider.Value;
                else if (slider == highlightsSlider)
                    highlightsVal = highlightsSlider.Value;
                else if (slider == shadowsSlider)
                    shadowsVal = shadowsSlider.Value;

                CreateEffect();
            }
            else
                throw new InvalidOperationException("Slider_ValueChanged expects a Slider object as the sender.");
        }

        private ICanvasImage CreateBrightnessEffect(ICanvasImage bitmap)
        {
            var effect = new BrightnessEffect
            {
                Source = bitmap
            };

            if (brightnessVal > 0)
                effect.BlackPoint = new Vector2(0, (float)(brightnessVal / 100));
            else
                effect.BlackPoint = new Vector2((float)(Math.Abs(brightnessVal) / 100), 0);

            return effect;
        }

        private ICanvasImage CreateExposureEffect(ICanvasImage bitmap)
        {
            var effect = new ExposureEffect
            {
                Source = bitmap,
                Exposure = (float)(exposureVal / 50)
            };
            return effect;
        }

        private ICanvasImage CreateContrastEffect(ICanvasImage bitmap)
        {
            var effect = new ContrastEffect
            {
                Source = bitmap,
                Contrast = (float)(contrastVal / 100)
            };
            return effect;
        }

        private ICanvasImage CreateHighlightsAndShadowsEffect(ICanvasImage bitmap)
        {
            var effect = new HighlightsAndShadowsEffect
            {
                Source = bitmap,
                Highlights = (float)(highlightsVal / 100),
                Shadows = (float)(shadowsVal / 100)
            };
            return effect;
        }
    }
}
