using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BAChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
        public MainPage()
        {
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            Window.Current.SetTitleBar(TitleBar);
            RequestedTheme = ElementTheme.Dark;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainNavigationView.SelectedItem = null;
            if (MainNavigationView.IsPaneOpen)
            {
                MainNavigationView.IsPaneOpen = false;
            }
            else
            {
                MainNavigationView.IsPaneOpen = true;
            }
        }
        private void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.Equals("Toggle Theme"))
            {
                if(RequestedTheme == ElementTheme.Dark)
                {
                    RequestedTheme = ElementTheme.Light;
                    titleBar.ButtonForegroundColor = Colors.Black;
                }
                else
                {
                    RequestedTheme = ElementTheme.Dark;
                    titleBar.ButtonForegroundColor = Colors.White;
                }
            }
            if (args.InvokedItem.Equals("Login/Out"))
            {
                LoginPageFlyout.ShowAt(MainScreen);
                LoginWebview.Navigate(new System.Uri("https://api.belowaverage.org/login/"));
            }
        }
    }
}
