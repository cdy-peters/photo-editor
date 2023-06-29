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
using Windows.Devices.Sensors;
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

        private enum Filter
        {
            None,
            Grayscale,
            Sepia,
            Invert
        }
        Filter filter = Filter.None;

        float brightnessVal = 0;
        float exposureVal = 0;
        float contrastVal = 0;
        float highlightsVal = 0;
        float shadowsVal = 0;
        float saturationVal = 0;
        float temperatureVal = 0;
        float tintVal = 0;
        float hueVal = 0;
        float sharpenVal = 0;
        float blurVal = 0;

        private void CreateEffect()
        {
            effect = bitmap;

            switch (filter)
            {
                case Filter.Grayscale:
                    effect = CreateGrayscaleEffect(effect);
                    break;
                case Filter.Sepia:
                    effect = CreateSepiaEffect(effect);
                    break;
                case Filter.Invert:
                    effect = CreateInvertEffect(effect);
                    break;
                default:
                    break;
            }

            if (brightnessVal != 0)
                effect = CreateBrightnessEffect(effect);
            if (exposureVal != 0)
                effect = CreateExposureEffect(effect);
            if (contrastVal != 0)
                effect = CreateContrastEffect(effect);
            if (highlightsVal != 0 || shadowsVal != 0)
                effect = CreateHighlightsAndShadowsEffect(effect);
            if (saturationVal != 0)
                effect = CreateSaturationEffect(effect);
            if (temperatureVal != 0 || tintVal != 0)
                effect = CreateTemperatureAndTintEffect(effect);
            if (hueVal != 0)
                effect = CreateHueRotationEffect(effect);
            if (sharpenVal != 0)
                effect = CreateSharpenEffect(effect);
            if (blurVal != 0)
                effect = CreateBlurEffect(effect);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch currentToggleSwitch = (ToggleSwitch)sender;

            if (currentToggleSwitch.IsOn)
            {
                // Turn off the last applied toggle switch
                switch (filter)
                {
                    case Filter.None:
                        break;
                    case Filter.Grayscale:
                        grayscaleToggleSwitch.IsOn = false;
                        break;
                    case Filter.Sepia:
                        sepiaToggleSwitch.IsOn = false;
                        break;
                    case Filter.Invert:
                        invertToggleSwitch.IsOn = false;
                        break;
                    default:
                        break;
                }

                // Update the filter based on the current toggle switch
                if (sender is ToggleSwitch toggleSwitch)
                {
                    if (toggleSwitch == grayscaleToggleSwitch)
                        filter = Filter.Grayscale;
                    else if (toggleSwitch == sepiaToggleSwitch)
                        filter = Filter.Sepia;
                    else if (toggleSwitch == invertToggleSwitch)
                        filter = Filter.Invert;
                }
            }
            else
                filter = Filter.None;

            CreateEffect();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                if (slider == brightnessSlider)
                    brightnessVal = (float)brightnessSlider.Value;
                else if (slider == exposureSlider)
                    exposureVal = (float)exposureSlider.Value;
                else if (slider == contrastSlider)
                    contrastVal = (float)contrastSlider.Value;
                else if (slider == highlightsSlider)
                    highlightsVal = (float)highlightsSlider.Value;
                else if (slider == shadowsSlider)
                    shadowsVal = (float)shadowsSlider.Value;
                else if (slider == saturationSlider)
                    saturationVal = (float)saturationSlider.Value;
                else if (slider == temperatureSlider)
                    temperatureVal = (float)temperatureSlider.Value;
                else if (slider == tintSlider)
                    tintVal = (float)tintSlider.Value;
                else if (slider == hueSlider)
                    hueVal = (float)hueSlider.Value;
                else if (slider == sharpenSlider)
                    sharpenVal = (float)sharpenSlider.Value;
                else if (slider == blurSlider)
                    blurVal = (float)blurSlider.Value;

                CreateEffect();
            }
            else
                throw new InvalidOperationException("Slider_ValueChanged expects a Slider object as the sender.");
        }

        private static ICanvasImage CreateGrayscaleEffect(ICanvasImage bitmap)
        {
            var effect = new GrayscaleEffect
            {
                Source = bitmap
            };
            return effect;
        }

        private static ICanvasImage CreateSepiaEffect(ICanvasImage bitmap)
        {
            var effect = new SepiaEffect
            {
                Source = bitmap
            };
            return effect;
        }

        private static ICanvasImage CreateInvertEffect(ICanvasImage bitmap)
        {
            var effect = new InvertEffect
            {
                Source = bitmap
            };
            return effect;
        }

        private ICanvasImage CreateBrightnessEffect(ICanvasImage bitmap)
        {
            var effect = new BrightnessEffect
            {
                Source = bitmap
            };

            brightnessVal /= 100;
            if (brightnessVal > 0)
                effect.BlackPoint = new Vector2(0, brightnessVal);
            else
                effect.BlackPoint = new Vector2(Math.Abs(brightnessVal), 0);

            return effect;
        }

        private ICanvasImage CreateExposureEffect(ICanvasImage bitmap)
        {
            var effect = new ExposureEffect
            {
                Source = bitmap,
                Exposure = exposureVal / 50
            };
            return effect;
        }

        private ICanvasImage CreateContrastEffect(ICanvasImage bitmap)
        {
            var effect = new ContrastEffect
            {
                Source = bitmap,
                Contrast = contrastVal / 100
            };
            return effect;
        }

        private ICanvasImage CreateHighlightsAndShadowsEffect(ICanvasImage bitmap)
        {
            var effect = new HighlightsAndShadowsEffect
            {
                Source = bitmap,
                Highlights = highlightsVal / 100,
                Shadows = shadowsVal / 100
            };
            return effect;
        }

        private ICanvasImage CreateSaturationEffect(ICanvasImage bitmap)
        {
            var effect = new SaturationEffect
            {
                Source = bitmap,
                Saturation = saturationVal / 200 + 0.5f
            };
            return effect;
        }

        private ICanvasImage CreateTemperatureAndTintEffect(ICanvasImage bitmap)
        {
            var effect = new TemperatureAndTintEffect
            {
                Source = bitmap,
                Temperature = temperatureVal / 100,
                Tint = tintVal / 100
            };
            return effect;
        }

        private ICanvasImage CreateHueRotationEffect(ICanvasImage bitmap)
        {
            var effect = new HueRotationEffect
            {
                Source = bitmap,
                Angle = (float)(((hueVal + 360) % 360) * Math.PI / 180)
            };

            return effect;
        }

        private ICanvasImage CreateSharpenEffect(ICanvasImage bitmap)
        {
            var effect = new SharpenEffect
            {
                Source = bitmap,
                Amount = sharpenVal / 10
            };
            return effect;
        }

        private ICanvasImage CreateBlurEffect(ICanvasImage bitmap)
        {
            var effect = new GaussianBlurEffect
            {
                Source = bitmap,
                BlurAmount = blurVal / 10,
                BorderMode = EffectBorderMode.Hard
            };
            return effect;
        }
    }
}
