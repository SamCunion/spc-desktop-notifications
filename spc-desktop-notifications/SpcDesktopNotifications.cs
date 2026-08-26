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
                List<GeoJsonFeature>? features = DeserialiseWarningJSON(await msg.Content.ReadAsStringAsync());

                if (features == null) //error while deserialising, or no active alerts. could happen when the nws api is down or has an occasional server error
                {
                    Console.WriteLine("Error deserialising JSON or no active warnings.");
                    return;
                }

                //filter the warnings, where validWarnings are the warnings that should be ranked for priority
                List<GeoJsonFeature> validWarnings = new List<GeoJsonFeature>();
                foreach (GeoJsonFeature w in features)
                {
                    //dont consider warnings that arent "actual" and of type "alert" (i.e culls test or excersise warnings as well as cancel message types)
                    if (w.Properties.Status == "Actual" && (w.Properties.MessageType == "Alert" || w.Properties.MessageType == "Update"))
                    {
                        //if a previous warning has the same ID as this warning, its already been considered
                        if (previousWarnings.Where(_w => _w.ID == w.ID).ToList().Count == 0)
                        {
                            //warnings that reach this stage are valid
                            w.SetWarningType();
                            validWarnings.Add(w);
                        }
                    }
                }

                //if onlyShowMostSevere option is false, show each valid warnings notification
                if (validWarnings.Count > 0)
                {
                    if (!ConfigManager.onlyShowMostSevere)
                    {
                        foreach (GeoJsonFeature w in validWarnings)
                        {
                            w.ShowNotification();
                        }
                    }
                    else //else, sort the warnings by most severe, and only show the most severe
                    {
                        validWarnings.OrderBy(w =>
                        {
                            return ConfigManager.warningTypeSeverityOrder.IndexOf(w.warningType);
                        }).ToList()[0].ShowNotification();
                    }
                }

                //replace previous warnings with new warnings
                previousWarnings = features;

            }
            catch(Exception ex)
            {
                Console.WriteLine("HTTP error probably");
                return;
            }
        }

        //takes the raw json string returned from the GET request and deserialises it to a GeoJsonFeature, or null if there was a deserialisation error.
        private List<GeoJsonFeature>? DeserialiseWarningJSON(string raw_json)
        {
            try
            {
                GeoJsonCollection? data = JsonSerializer.Deserialize<GeoJsonCollection>(raw_json);
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
        public List<GeoJsonFeature> Features { get; set; }
    }

    internal class GeoJsonFeature
    {

        //Shows a notification for this feature
        public void ShowNotification()
        {
            var notification = new AppNotificationBuilder()
                .AddArgument("action", "viewItem")
                .AddText(commonName)
                .AddText(Properties.Description)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);

            Console.WriteLine("Pushing notification of type " + commonName);
        }

        public SevereWarnings warningType;
        public string commonName = "";

        //sets the warningType value to its SevereWarning type, or null if its not a severe warning
        public SevereWarnings? SetWarningType()
        {
            switch (Properties.Event.ToLower())
            {
                case "severe thunderstorm warning":
                    //check if EDS with wording
                    if (Properties.Description.Contains("EXTREMELY DANGEROUS SITUATION"))
                    {
                        warningType = SevereWarnings.EDSSTW;
                        commonName = "EXTREMELY DESTRUCTIVE SEVERE THUNDERSTORM WARNING";
                    }
                    else if (Properties.Parameters.SevereThunderstormDamageThreat != null && Properties.Parameters.SevereThunderstormDamageThreat[0] == "DESTRUCTIVE") // destructive = destructive
                    {
                        warningType = SevereWarnings.DSTW;
                        commonName = "DESTRUCTIVE SEVERE THUNDERSTORM WARNING";
                    }
                    else if (Properties.Parameters.SevereThunderstormDamageThreat != null && Properties.Parameters.SevereThunderstormDamageThreat[0] == "CONSIDERABLE") //considerable = considerable
                    {
                        warningType = SevereWarnings.CSTW;
                        commonName = "CONSIDERABLE SEVERE THUNDERSTORM WARNING";
                    }
                    else //has to be a regular severe thunderstorm warning
                    {
                        warningType = SevereWarnings.STW;
                        commonName = "SEVERE THUNDERSTORM WARNING";
                    }
                    break;
                case "tornado warning":
                    if (Properties.Parameters.TornadoDamageThreat != null && Properties.Parameters.TornadoDamageThreat[0] == "CATASTROPHIC") //catastrophic = tornado emergency
                    {
                        warningType = SevereWarnings.TORE;
                        commonName = "TORNADO EMERGENCY";
                    }
                    else if (Properties.Parameters.TornadoDamageThreat != null && Properties.Parameters.TornadoDamageThreat[0] == "CONSIDERABLE") //considerable = pds
                    {
                        warningType = SevereWarnings.PDSTOR;
                        commonName = "PARTICULARLY DANGEROUS SITUATION TORNADO WARNING";
                    }
                    else if (Properties.Parameters.TornadoDetection != null && Properties.Parameters.TornadoDetection[0] == "OBSERVED") //observed = observed tornado warning
                    {
                        warningType = SevereWarnings.TORO;
                        commonName = "OBSERVED TORNADO WARNING";
                    }
                    else //has to be a radar indicated tornado warning
                    {
                        warningType = SevereWarnings.TORR;
                        commonName = "RADAR INDICATED TORNADO WARNING";
                    }
                    break;
                case "flash flood warning":
                    if (Properties.Parameters.FlashFloodDamageThreat != null && Properties.Parameters.FlashFloodDamageThreat[0] == "CATASTROPHIC") //catastrophic = flash flood emergency
                    {
                        warningType = SevereWarnings.FFE;
                        commonName = "FLASH FLOOD EMERGENCY";
                    }
                    else if (Properties.Parameters.FlashFloodDamageThreat != null && Properties.Parameters.FlashFloodDamageThreat[0] == "CONSIDERABLE") //considerable = considerable
                    {
                        warningType = SevereWarnings.FFC;
                        commonName = "CONSIDERABLE FLASH FLOOD WARNING";
                    }
                    else //regular flash flood warning
                    {
                        warningType = SevereWarnings.FF;
                        commonName = "FLASH FLOOD WARNING";
                    }
                    break;
            }

            //return the value of warningType
            return warningType;
        }

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("properties")]
        public GeoJsonProps Properties { get; set; }
    }

    internal class GeoJsonProps
    {
        [JsonPropertyName("sent")]
        public string Sent { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("messageType")]
        public string MessageType { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("parameters")]
        public WarningParams Parameters { get; set; }
    }

    internal class WarningParams
    {
        [JsonPropertyName("flashFloodDamageThreat")]
        public List<string>? FlashFloodDamageThreat { get; set; }

        [JsonPropertyName("thunderstormDamageThreat")]
        public List<string>? SevereThunderstormDamageThreat { get; set; }

        [JsonPropertyName("tornadoDamageThreat")]
        public List<string>? TornadoDamageThreat { get; set; }

        [JsonPropertyName("tornadoDetection")]
        public List<string>? TornadoDetection { get; set; }
    }
}
