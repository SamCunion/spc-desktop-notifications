using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace spc_desktop_notifications
{
    internal class SpcDesktopNotifications
    {

        private HttpClient client;
        private string queryPath = "";
        private List<GeoJsonFeature> previousWarnings = new List<GeoJsonFeature>();
        private System.Windows.Forms.Timer processTimer = new System.Windows.Forms.Timer();

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

            //get the query path
            queryPath = getQueryPath();

            //check every 30 seconds
            processTimer.Interval = 30000; //30 seconds
            processTimer.Tick += new EventHandler(UpdateWarnings);
            processTimer.Start();

            //call UpdateWarnings initially
            UpdateWarnings(null, null);

        }

        //stops all ongoing processes associated with the app.
        public void Stop()
        {
            processTimer.Stop();
            processTimer.Dispose();
            AppNotificationManager.Default.Unregister();
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

        //gets the query path for the spc API, depending on the ConfigManager attributes
        private string getQueryPath()
        {
            string output = "/alerts/active?";

            //change warning scope
            switch (ConfigManager.mode)
            {
                case NotificationMode.STATE:
                    output += "area=" + ConfigManager.state;
                    break;
                case NotificationMode.POINT:
                    output += "point=" + ConfigManager.location.lat + "," + ConfigManager.location.lon;
                    break;
            }

            //filter warning types
            string warning_types = "";
            //does the app need to listen for Flash Flood Warnings?
            if (ConfigManager.warnings.Contains(SevereWarnings.FF) || ConfigManager.warnings.Contains(SevereWarnings.FFE)) 
            {
                warning_types += Uri.EscapeDataString("Flash Flood Warning") + ",";
            }

            //does the app need to listen for Severe Thunderstorm Warnings?
            if (ConfigManager.warnings.Contains(SevereWarnings.EDSSTW) || ConfigManager.warnings.Contains(SevereWarnings.DSTW) || ConfigManager.warnings.Contains(SevereWarnings.CSTW) || ConfigManager.warnings.Contains(SevereWarnings.STW))
            {
                warning_types += Uri.EscapeDataString("Severe Thunderstorm Warning") + ",";
            }

            //does the app need to listen for Tornado Warnings?
            if (ConfigManager.warnings.Contains(SevereWarnings.TORE) || ConfigManager.warnings.Contains(SevereWarnings.PDSTOR) || ConfigManager.warnings.Contains(SevereWarnings.TORO) || ConfigManager.warnings.Contains(SevereWarnings.TORR))
            {
                warning_types += Uri.EscapeDataString("Tornado Warning") + ",";
            }

            if (warning_types == "") //isnt watching for anything, return empty string
            {
                return "";
            }

            //trim trailing comma
            warning_types = warning_types.Remove(warning_types.Length - 1);
            output += "&event=" + warning_types;

            Console.WriteLine("Query URL: " + output);
            return output;
        }

        //fetches the current warnings from the spc, deserialises the response, and compares the valid warning features against the previous set. Then sets the previous set to the current set for next time.
        private async void UpdateWarnings(object? sender, EventArgs? e)
        {
            Console.WriteLine("update warnings called");
            //try and get the current SPC alerts, if an error was encountered just return.
            try
            {
                using HttpResponseMessage response = await client.GetAsync(queryPath);
                var msg = response.EnsureSuccessStatusCode();

                //get the GeoJson feature collection by deserialising the response
                GeoJsonFeature[]? features = DeserialiseWarningJSON(await msg.Content.ReadAsStringAsync());

                if (features == null) //error while deserialising, or no active alerts. could happen when the nws api is down or has an occasional server error
                {
                    Console.WriteLine("Error deserialising JSON or no active warnings.");
                    return;
                }
                Console.WriteLine(features.Length);

            }
            catch(Exception ex)
            {
                Console.WriteLine("HTTP error probably");
                return;
            }
        }

        //takes the raw json string returned from the GET request and deserialises it to a GeoJsonFeature, or null if there was a deserialisation error.
        private GeoJsonFeature[]? DeserialiseWarningJSON(string raw_json)
        {
            try
            {
                GeoJsonCollection? data = JsonSerializer.Deserialize<GeoJsonCollection>(raw_json);
                Console.WriteLine(data.Features);
                if (data != null && data.Features != null)
                {
                    return data.Features;
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    internal class GeoJsonCollection
    {
        [JsonPropertyName("features")]
        public GeoJsonFeature[] Features;
    }

    internal class GeoJsonFeature
    {
        [JsonPropertyName("id")]
        public string ID;

        [JsonPropertyName("properties")]
        public GeoJsonProps Properties;
    }

    internal class GeoJsonProps
    {
        [JsonPropertyName("sent")]
        public string Sent;

        [JsonPropertyName("status")]
        public string Status;

        [JsonPropertyName("messageType")]
        public string MessageType;

        [JsonPropertyName("event")]
        public string Event;

        [JsonPropertyName("headline")]
        public string Headline;

        [JsonPropertyName("description")]
        public string Description;

        [JsonPropertyName("parameters")]
        public WarningParams Parameters;
    }

    internal class WarningParams
    {
        [JsonPropertyName("damageThreat")]
        public string[] DamageThreat;
    }
}
