using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace BAChat
{
    public sealed partial class MainPage : Page
    {
        public ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        public ContentDialog messageDialog = new messageContentDialog();

        public string loginSessionID = "";

        public string HTTPApiBaseURL = "https://api.belowaverage.org/v1/";

        public bool mDialogIsOpen = false;

        public ApplicationView currentApplicationView = ApplicationView.GetForCurrentView();

        public DispatcherTimer loginTimer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            currentApplicationView.SetPreferredMinSize(new Size(450, 300));
            ApplicationView.PreferredLaunchViewSize = new Size(450, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            Window.Current.SetTitleBar(TitleBar);
            RequestedTheme = ElementTheme.Dark;
            loginTimer.Tick += LoginTimer_Tick;
            loginTimer.Interval = new TimeSpan(0, 0, 1);
            messageDialog.CloseButtonClick += MessageDialog_CloseButtonClick;
            login();
        }

        public async Task<string> HTTP_post_data(string URL, string EncodedData)
        {
            WebRequest request = WebRequest.Create(URL);
            request.Method = "POST";
            string postData = EncodedData;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            request.Headers["origin"] = "https://login.belowaverage.org";
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
            run_time.Text = "<" + time + " ";
            paragraph.Inlines.Add(run_time);
            run_username.FontWeight = Windows.UI.Text.FontWeights.Bold;
            run_username.Text = username;
            paragraph.Inlines.Add(run_username);
            run_text.FontWeight = Windows.UI.Text.FontWeights.Normal;
            run_text.Text = "> " + text;
            paragraph.Inlines.Add(run_text);
            ChatBox.Blocks.Add(paragraph);
        }

        public void hideDialog()
        {
            if (mDialogIsOpen)
            {
                messageDialog.Hide();
                mDialogIsOpen = false;
            }
        }

        private void MessageDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            hideDialog();
        }

        public async void showDialog(string title, string message, bool showOK = true)
        {
            messageDialog.RequestedTheme = RequestedTheme;
            messageDialog.Title = title;
            messageDialog.Content = message;
            if(showOK)
            {
                messageDialog.CloseButtonText = "Ok";
            }
            else
            {
                messageDialog.CloseButtonText = "";
            }
            if (!mDialogIsOpen)
            {
                mDialogIsOpen = true;
                await messageDialog.ShowAsync();
            }
        }

        public void setTitle(string title = "")
        {
            currentApplicationView.Title = title;
            if(title.Equals(""))
            {
                TitleBarText.Text = "BA Chat";
            }
            else
            {
                TitleBarText.Text = title + " - BA Chat";
            }
        }

        public async void login()
        {
            LoginWebView.NavigateToString(
                "<iframe style=\"" +
                    "background-color:rgba(255,255,255,.8);" +
                    "position:absolute;" +
                    "top:0px;" +
                    "left:0px;" +
                    "width:100%;" +
                    "height:100%;" +
                    "border:0px;" +
                "\" src=\"https://login.belowaverage.org/\"></iframe>" +
                "<script src=\"https://static.belowaverage.org/js/auth.js\"></script>" +
                "<script> function authToken() { return AUTH.token; } </script>"
            );
            LoginWebView.Visibility = Visibility.Visible;
            loginTimer.Start();
        }

        private async void LoginTimer_Tick(object sender, object e)
        {
            string token = await LoginWebView.InvokeScriptAsync("authToken", null);
            if (token.Length == 32)
            {
                loginTimer.Stop();
                loginSessionID = token;
                LoginWebView.Visibility = Visibility.Collapsed;
            }
        }


        public async void logout()
        {
            showDialog("Login Session", "Sending logoff command to server...", false);
            await HTTP_post_data(HTTPApiBaseURL + "AUTH/", "AUTH=" + loginSessionID + "&logout");
            showDialog("Login Session", "Confirming logoff...", false);
            bool isSuccess = false;
            try
            {
                await HTTP_post_data(HTTPApiBaseURL + "AUTH/", "AUTH=" + loginSessionID);
            }
            catch (WebException e)
            {
                if(e.Status == WebExceptionStatus.ProtocolError)
                {
                    isSuccess = true;
                }
            }
            if (isSuccess)
            {
                showDialog("Logout Succeeded", "You have been successfully logged out.");
            }
            else
            {
                showDialog("Logout Forced", "You have been forcefully logged out client side.");
            }
            loginSessionID = "";
            setTitle();
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
            MainNavigationView.SelectedItem = null;
            if (args.InvokedItem.Equals("Toggle Theme"))
            {
                if (RequestedTheme == ElementTheme.Dark)
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
                if (LoginWebView.Visibility == Visibility.Collapsed)
                {
                    if (loginSessionID.Equals(""))
                    {
                        login();
                    }
                    else
                    {
                        logout();
                    }

                }
                else
                {
                    LoginWebView.Visibility = Visibility.Collapsed;
                    loginTimer.Stop();
                }
            }
            if (args.InvokedItem.Equals("About BA Chat"))
            {
                
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
        }



        
    }
}
