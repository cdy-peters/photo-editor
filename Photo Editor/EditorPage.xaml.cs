using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            CanvasDrawingSession ds = args.DrawingSession;
            ds.DrawImage(bitmap);

            sender.Invalidate();
        }
    }
}
