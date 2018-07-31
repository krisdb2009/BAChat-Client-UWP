using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using WebSocket4Net;
using Windows.UI.Core;

namespace BAChat
{
    public sealed partial class MainPage : Page
    {
        public ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        public ContentDialog messageDialog = new messageContentDialog();

        public static string loginSessionID = "";

        public string HTTPApiBaseURL = "https://api.belowaverage.org/v1/";

        public bool mDialogIsOpen = false;

        public static ApplicationView currentApplicationView = ApplicationView.GetForCurrentView();

        public DispatcherTimer loginTimer = new DispatcherTimer();

        public ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public WebSocket webSocket;

        public MainPage()
        {
            InitializeComponent();
            currentApplicationView.SetPreferredMinSize(new Size(450, 300));
            ApplicationView.PreferredLaunchViewSize = new Size(450, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            Window.Current.SetTitleBar(TitleBar);
            loginTimer.Tick += LoginTimer_Tick;
            loginTimer.Interval = TimeSpan.FromMilliseconds(100);
            messageDialog.CloseButtonClick += MessageDialog_CloseButtonClick;
            if (LocalSettings.Values.ContainsKey("isLoggedIn") && (bool)LocalSettings.Values["isLoggedIn"])
            {
                login();
            }
            else
            {
                LoginServerMenu.Visibility = Visibility.Visible;
            }
            if (LocalSettings.Values.ContainsKey("theme") && (int)LocalSettings.Values["theme"] == 0)
            {
                RequestedTheme = ElementTheme.Light;
            }
            else
            {
                RequestedTheme = ElementTheme.Dark;
            }
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

        public async void hideDialog()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (mDialogIsOpen)
                {
                    messageDialog.Hide();
                    mDialogIsOpen = false;
                }
            });
        }

        private void MessageDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            hideDialog();
        }

        public async void showDialog(string title, string message, bool showOK = true)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                messageDialog.RequestedTheme = RequestedTheme;
                messageDialog.Title = title;
                messageDialog.Content = message;
                if (showOK)
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
            });
        }

        public async void setTitle(string title = "")
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                currentApplicationView.Title = title;
                if (title.Equals(""))
                {
                    TitleBarText.Text = "BA Chat";
                }
                else
                {
                    TitleBarText.Text = title + " - BA Chat";
                }
            });
        }

        public void login()
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
            string token = "";
            try
            {
                token = await LoginWebView.InvokeScriptAsync("authToken", null);
            } catch (Exception) { }
            if (token.Length == 32)
            {
                loginTimer.Stop();
                loginSessionID = token;
                LoginWebView.Visibility = Visibility.Collapsed;
                LocalSettings.Values["isLoggedIn"] = true;
                showDialog("Connecting...", "Connecting to server...", false);
                webSocket = await WebSocketClient.Connect("localhost", false, 55475);
                webSocket.MessageReceived += WSClient_MessageReceived;
            }
        }

        private void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (Protocol.Receive.Login(e.Message, out string token))
            {
                showDialog("Authenticating...", "Logging on to server...", false);
                if (token != null)
                {
                    loginSessionID = token;
                }
                Protocol.Send.Login(loginSessionID);
                //Other Stuff Here
            }
            else if (Protocol.Receive.Init(e.Message, out string initUsername, out string channel))
            {
                if (channel == null)
                {
                    setTitle(initUsername);
                }
                else
                {
                    setTitle(channel + " - " + initUsername);
                }
                hideDialog();
            }
            else if (Protocol.Receive.Join(e.Message))
            {
                //Close out of the current chat
            }
            else if (Protocol.Receive.Chat(e.Message, out string chatUsername, out string message))
            {

            }
        }

        public async void logout()
        {
            await webSocket.CloseAsync();
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
            LocalSettings.Values["isLoggedIn"] = false;
            setTitle();
            LoginServerMenu.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainNavigationView.SelectedItem = null;
            if (MainNavigationView.IsPaneOpen || loginSessionID == "")
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
            MainNavigationView.SelectedItem = null;
            if (args.InvokedItem.Equals("Toggle Theme"))
            {
                if (RequestedTheme == ElementTheme.Dark)
                {
                    RequestedTheme = ElementTheme.Light;
                    titleBar.ButtonForegroundColor = Colors.Black;
                    LocalSettings.Values["theme"] = 0;
                }
                else
                {
                    RequestedTheme = ElementTheme.Dark;
                    titleBar.ButtonForegroundColor = Colors.White;
                    LocalSettings.Values["theme"] = 1;
                }
            }
            if (args.InvokedItem.Equals("Logout"))
            {
                if (LoginWebView.Visibility == Visibility.Collapsed)
                {
                        logout();
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

        private void loginToBA_Click(object sender, RoutedEventArgs e)
        {
            LoginServerMenu.Visibility = Visibility.Collapsed;
            login();
        }

        private void customServerLogin_Click(object sender, RoutedEventArgs e)
        {
            CustomServerMenu.Visibility = Visibility.Visible;
        }

        private void custServBack_Click(object sender, RoutedEventArgs e)
        {
            CustomServerMenu.Visibility = Visibility.Collapsed;
        }
    }
}
