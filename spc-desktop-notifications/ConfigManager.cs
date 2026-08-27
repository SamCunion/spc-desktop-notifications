using System;
using System.Collections.Generic;
using System.Text;

namespace spc_desktop_notifications
{
    internal static class ConfigManager
    {

        public static readonly string cfgPath = Path.Combine(AppContext.BaseDirectory, "settings.cfg");
        public static NotificationMode mode = NotificationMode.GLOBAL;
        public static bool onlyShowMostSevere = false;
        public static bool notifyOnWarningUpdate = true;
        public static GeoPoint location = new GeoPoint(39.5f, -98.35f);
        public static string state = "KS";
        public static List<SevereWarnings> warnings = new List<SevereWarnings>();
        public static readonly List<SevereWarnings> warningTypeSeverityOrder = new List<SevereWarnings>() { SevereWarnings.TORE, SevereWarnings.PDSTOR, SevereWarnings.EDSSTW, SevereWarnings.FFE, SevereWarnings.DSTW, SevereWarnings.TORO, SevereWarnings.FFC, SevereWarnings.CSTW, SevereWarnings.TORR, SevereWarnings.STW, SevereWarnings.FF }; //subjective opinion, probably temporary


        private static readonly string cfgDefaultText = "" +
            "# spc-desktop-notifications config file\n" +
            "# report issues to https://github.com/SamCunion/spc-desktop-notifications/issues\n" +
            "\n" +
            "# mode - determines the scope of your alerts. Options are Point, State, or Global.\n" +
            "mode=Global\n" +
            "\n" +
            "# onlyShowMostSevere - whether only the most severe alert issued in the last 30 seconds shows a notification, or all alerts from the last 30 seconds show a notification.\n" +
            "onlyShowMostSevere=false\n" +
            "\n" +
            "# notifyOnWarningUpdate - whether updates to existing warnings trigger a new notification\n" +
            "notifyOnWarningUpdate=true\n" +
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
            "#'DSTW' (Destructive Severe Thunderstorm Warning),\n" +
            "#'CSTW' (Considerable Severe Thunderstorm Warning),\n" +
            "#'STW' (Severe Thunderstorm Warning)\n" +
            "#'FFE' (Flash Flood Emergency),\n" +
            "#'FFC' (Considerable Flash Flood Warning),\n" +
            "#'FF' (Flash Flood Warning),\n" +
            "warnings=TORE,PDSTOR,TORO,TORR,EDSSTW,DSTW,CSTW,STW,FFE,FFC,FF";

        //Initiates the config manager by creating the default config file if required, and reading the config state from the config file.
        public static void Init()
        {
            //check if the config file exists, if not, create it and fill it with default values
            if (!File.Exists(cfgPath))
            {
                Console.WriteLine("Config file not found. Creating a default config file.");
                File.WriteAllText(cfgPath, cfgDefaultText);
            }

            //update the config state
            Update();
        }

        //updates the ConfigManager attributes with the values read from the config file.
        public static void Update()
        {
            //read the lines in the config file
            foreach (var line in File.ReadAllLines(cfgPath))
            {
                //skip lines that start with "#" or whitespace
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                //split the line at the "=" symbol
                var pair = line.Split("=", 2);

                //if theres more than 2 parts, the syntax is invalid and is skipped.
                if (pair.Length != 2)
                {
                    Console.WriteLine("Error parsing config line: More than 2 sections divided by '=': " + line);
                    continue;
                }

                var option = pair[0].Trim();
                var value = pair[1].Trim();

                //update state by option
                if (option.ToLower() == "mode") //mode
                {
                    if (value.ToLower() == "point")
                    {
                        mode = NotificationMode.POINT;
                        Console.WriteLine("MODE set to " + mode);
                    }
                    else if (value.ToLower() == "state")
                    {
                        mode = NotificationMode.STATE;
                        Console.WriteLine("MODE set to " + mode);
                    }
                    else if (value.ToLower() == "global")
                    {
                        mode = NotificationMode.GLOBAL;
                        Console.WriteLine("MODE set to " + mode);
                    }
                    else
                    {
                        Console.WriteLine("Error setting mode, option was invalid: " + mode);
                    }
                }
                else if (option.ToLower() == "location") //location
                {
                    var latlon = value.Split(",");
                    if (latlon.Length != 2)
                    {
                        Console.WriteLine("Error setting location, more than two ',' encountered in line: " + line);
                        continue;
                    }

                    try
                    {
                        var lat = float.Parse(latlon[0]);
                        var lon = float.Parse(latlon[1]);
                        location = new GeoPoint(lat, lon);
                        Console.WriteLine("Location set to " + lat + "," + lon);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error parsing lat or lon value in line: " + line);
                        continue;
                    }
                }
                else if (option.ToLower() == "state") //state
                {
                    if (value.Length != 2)
                    {
                        Console.WriteLine("Error parsing state, value is not of length 2: " + line);
                        continue;
                    }
                    state = value.ToUpper();
                    Console.WriteLine("State set to " + state);
                }
                else if (option.ToLower() == "warnings") // warnings
                {
                    warnings.Clear(); //clear the previous warnings
                    var warningIds = value.ToUpper().Split(",");
                    foreach (var w in warningIds)
                    {
                        switch (w)
                        {
                            case "TORE":
                                warnings.Add(SevereWarnings.TORE);
                                break;
                            case "PDSTOR":
                                warnings.Add(SevereWarnings.PDSTOR);
                                break;
                            case "TORO":
                                warnings.Add(SevereWarnings.TORO);
                                break;
                            case "TORR":
                                warnings.Add(SevereWarnings.TORR);
                                break;
                            case "EDSSTW":
                                warnings.Add(SevereWarnings.EDSSTW);
                                break;
                            case "DSTW":
                                warnings.Add(SevereWarnings.DSTW);
                                break;
                            case "CSTW":
                                warnings.Add(SevereWarnings.CSTW);
                                break;
                            case "STW":
                                warnings.Add(SevereWarnings.STW);
                                break;
                            case "FFE":
                                warnings.Add(SevereWarnings.FFE);
                                break;
                            case "FFC":
                                warnings.Add(SevereWarnings.FFC);
                                break;
                            case "FF":
                                warnings.Add(SevereWarnings.FF);
                                break;
                            default:
                                continue;
                        }
                        Console.WriteLine("Added " + w + " to warning list");
                    }
                }
                else if (option.ToLower() == "onlyshowmostsevere")
                {
                    if (value.ToLower() == "true")
                    {
                        onlyShowMostSevere = true;
                    }
                    else
                    {
                        onlyShowMostSevere = false;
                    }
                }
                else if (option.ToLower() == "notifyonwarningupdate")
                {
                    if (value.ToLower() == "true")
                    {
                        notifyOnWarningUpdate = true;
                    }
                    else
                    {
                        notifyOnWarningUpdate = false;
                    }
                }
            }
        }

    }

    internal enum NotificationMode
    {
        POINT,
        STATE,
        GLOBAL,
    }

    internal enum SevereWarnings
    {
        TORE,
        PDSTOR,
        TORO,
        TORR,
        EDSSTW,
        DSTW,
        CSTW,
        STW,
        FFE,
        FFC,
        FF
    }

}