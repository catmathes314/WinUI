// MainWindow.xaml.cs

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace WinUICalculator
{
    public sealed partial class MainWindow : Window
    {
        public CalculatorViewModel ViewModel { get; } = new();

        public MainWindow()
        {
            this.InitializeComponent();
            RootGrid.DataContext = ViewModel;

            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(450, 650));
        }
    }
}
