using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public string loginSessionID = "";

        public string HTTPApiBaseURL = "https://api.belowaverage.org/v1/";

        public async Task<string> HTTP_post_data(string URL, string EncodedData)
        {
            WebRequest request = WebRequest.Create(URL);
            request.Method = "POST";
            string postData = EncodedData;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = await request.GetResponseAsync();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
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
        private async void MainNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
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
                if (LoginWebView.Visibility == Visibility.Collapsed) {
                    if(loginSessionID.Equals(""))
                    {
                        LoginWebView.NavigateToString("<h1 style=\"position:absolute;top:calc(50% - 20px);text-align:center;width:100%;font-family:Segoe UI;\">Waiting for login request...</h1>");
                        LoginWebView.Visibility = Visibility.Visible;
                        string messageID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=");
                        LoginWebView.Navigate(new System.Uri("https://api.belowaverage.org/login/#" + messageID));
                        loginSessionID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=" + messageID);
                        if(loginSessionID.Length == 32) //A key was returned possibly.
                        {
                            LoginWebView.Visibility = Visibility.Collapsed;
                            ChatInputBox.Text = await HTTP_post_data(HTTPApiBaseURL + "chat/", "AUTH=" + loginSessionID);
                        }
                    }
                    else
                    {
                        loginSessionID = "";
                        //Possibly alert the user.
                    }
                    
                }
                else
                {
                    LoginWebView.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
