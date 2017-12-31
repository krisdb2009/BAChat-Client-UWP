using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

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

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(450, 300));
            ApplicationView.PreferredLaunchViewSize = new Size(450, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            Window.Current.SetTitleBar(TitleBar);
            RequestedTheme = ElementTheme.Dark;
        }

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

        public void append_line(string time, string username, string text)
        {
            Paragraph paragraph = new Paragraph();
            Run run_time = new Run();
            Run run_username = new Run();
            Run run_text = new Run();
            run_time.Text = "<"+time+" ";
            paragraph.Inlines.Add(run_time);
            run_username.FontWeight = Windows.UI.Text.FontWeights.Bold;
            run_username.Text = username;
            paragraph.Inlines.Add(run_username);
            run_text.FontWeight = Windows.UI.Text.FontWeights.Normal;
            run_text.Text = "> "+text;
            paragraph.Inlines.Add(run_text);
            ChatBox.Blocks.Add(paragraph);
        }

        public string gen_message_page(string message)
        {
            return "<style>body {background-color:rgba(0,0,0,.5);}</style><h2 style=\"color:white;position:absolute;left:0px;top:calc(50% - 20px);text-align:center;width:100%;font-family:Segoe UI;\">"+message+"</h2>";
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
                        
                        LoginWebView.NavigateToString(gen_message_page("Requesting login session..."));
                        LoginWebView.Visibility = Visibility.Visible;
                        string messageID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=");
                        LoginWebView.Navigate(new System.Uri("https://api.belowaverage.org/login/#" + messageID));
                        loginSessionID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=" + messageID);
                        if (loginSessionID.Length == 32) //A key was returned possibly.
                        {
                            LoginWebView.NavigateToString(gen_message_page("Testing session validity..."));
                            append_line(DateTime.Now.ToString("h:mm:ss tt"), "server", await HTTP_post_data(HTTPApiBaseURL + "chat/", "AUTH=" + loginSessionID));
                            LoginWebView.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        LoginWebView.NavigateToString(gen_message_page("Sending logoff message..."));
                        LoginWebView.Visibility = Visibility.Visible;
                        await HTTP_post_data(HTTPApiBaseURL + "message/", "AUTH=" + loginSessionID + "&logout");
                        LoginWebView.NavigateToString(gen_message_page("Checking that the session has been terminated..."));
                        await HTTP_post_data(HTTPApiBaseURL + "message/", "AUTH=" + loginSessionID); //Check for 403
                        LoginWebView.Visibility = Visibility.Collapsed;
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
