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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Photo_Editor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void PhotoPicker_Click(object sender, RoutedEventArgs e)
        {
            // Create file picker
            FileOpenPicker openPicker = new()
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter =
                {
                    { ".jpg" },
                    { ".jpeg" },
                    { ".png" }
                }
            };

            // Get window handle
            var window = (Application.Current as App)?.Window as MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize file picker
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Open picker
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
                Frame.Navigate(typeof(EditorPage), file);
        }
    }
}
