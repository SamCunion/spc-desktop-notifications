using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System.Net.Http.Headers;
using System.Text.Json;

namespace spc_desktop_notifications
{
    internal class SpcDesktopNotifications
    {

        private HttpClient client;

        //Initiates the app
        public SpcDesktopNotifications()
        {

            //initiates the HTTP client for the service
            client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/SamCunion/spc-desktop-notifications , https://github.com/SamCunion/spc-desktop-notifications/issues)"));
            client.BaseAddress = new Uri("https://api.weather.gov");

            //register for notifications
            AppNotificationManager.Default.Register();

            //initiate the config manager
            ConfigManager.Init();

            SendNotification("This is a title", "This is the content");

            UpdateWarnings();

        }

        private bool SendNotification(string title, string content)
        {
            var notification = new AppNotificationBuilder()
                .AddArgument("action", "viewItem")
                .AddText(title)
                .AddText(content)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);

            return true;
        }

        private async void UpdateWarnings()
        {
            //try and get the current SPC alerts, if an error was encountered just return.
            try
            {
                using HttpResponseMessage response = await client.GetAsync("/alerts/active?event=Tornado%20Warning,Severe%20Thunderstorm%20Warning");
                var msg = response.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException e)
            {
                return;
            }


            
        }
    }
}
