using System;
using System.Collections.Generic;
using System.Text;

namespace spc_desktop_notifications
{
    internal static class ConfigManager
    {

        public static readonly string cfgPath = Path.Combine(AppContext.BaseDirectory, "settings.cfg");

        private static readonly string cfgDefaultText = "" +
            "# spc-desktop-notifications config file\n" +
            "# report issues to https://github.com/SamCunion/spc-desktop-notifications/issues\n" +
            "\n" +
            "# mode - determines the scope of your alerts. Options are Point, State, or Global.\n" +
            "mode=Global\n" +
            "\n" +
            "# location - provide the latitude and longitude of the location you want Point alerts for. Does not matter if mode is set to State or Global.\n" +
            "location=39.5,-98.35\n" +
            "\n" +
            "# state - provide the two-letter state ID for the state you want State alerts for. Does not matter if mode is set to Point or Global.\n" +
            "state=KS\n" +
            "\n" +
            "# warnings - the types of warnings you wish to be notified about. Options are:\n" +
            "#'TORE' (Tornado Emergency),\n" +
            "#'PDSTOR' (PDS Tornado Warning),\n" +
            "#'TORO' (Observed Tornado Warning),\n" +
            "#'TORR' (Radar Indicated Tornado Warning),\n" +
            "#'EDSSTW' (Extremely Dangerous Situation Severe Thunderstorm Warning)\n" +
            "#'DSTW' (Destructuve Severe Thunderstorm Warning),\n" +
            "#'CSTW' (Considerable Severe Thunderstorm Warning),\n" +
            "#'STW' (Severe Thunderstorm Warning)\n" +
            "#'FFE' (Flash Flood Emergency),\n" +
            "#'FF' (Flash Flood Warning),\n" +
            "warnings=TORE,PDSTOR,TORO,TORR,EDSSTW,DSTW,CSTW,STW,FFE,FF";

        public static void Init()
        {
            //check if the config file exists, if not, create it and fill it with default values
            if (!File.Exists(cfgPath))
            {
                Console.WriteLine("Config file not found. Creating a default config file.");
                File.WriteAllText(cfgPath, cfgDefaultText);
            }
        }

        public static string GetSetting(string name)
        {
            return "";
        }


    }
}
