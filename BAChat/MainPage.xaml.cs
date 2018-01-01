using System;
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
            showDialog("Login Session", "Generating Login Page...", false);
            string messageID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=");
            hideDialog();
            LoginWebView.Navigate(new Uri("https://api.belowaverage.org/login/#" + messageID));
            LoginWebView.Visibility = Visibility.Visible;
            try
            {
                loginSessionID = await HTTP_post_data(HTTPApiBaseURL + "message/", "id=" + messageID);
                LoginWebView.Visibility = Visibility.Collapsed;
                LoginWebView.NavigateToString("");
            }
            catch (WebException e)
            {
                if (LoginWebView.Visibility == Visibility.Visible)
                {
                    showDialog("Login Session Error", e.Message);
                    LoginWebView.Visibility = Visibility.Collapsed;
                    LoginWebView.NavigateToString("");
                }
            }
            if (loginSessionID.Length == 32) //A key was returned possibly.
            {
                showDialog("Login Session", "Retrieving login details...", false);
                setTitle(await HTTP_post_data(HTTPApiBaseURL + "whoami/", "AUTH=" + loginSessionID));
                hideDialog();
            }
        }

        public async void logout()
        {
            showDialog("Login Session", "Sending logoff command to server...", false);
            await HTTP_post_data(HTTPApiBaseURL + "chat/", "AUTH=" + loginSessionID + "&logout");
            showDialog("Login Session", "Confirming logoff...", false);
            await HTTP_post_data(HTTPApiBaseURL + "chat/", "AUTH=" + loginSessionID); //Check for 403
            loginSessionID = "";
            setTitle();
            showDialog("Logout Succeeded", "You have been successfully logged out.");
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
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
