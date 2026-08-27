# SPC Desktop Notifications
<i>Custom NWS Severe Alerts for your desktop!</i>

## Features:
- <b>Only get the alerts that matter to you - </b> Alerts can be filtered by three geographic scopes. You can choose to receive notifications for every storm-based alert issued by the NWS nationwide, limit the notifications to alerts issued for a single state, or only receive alerts that cover a single geographic location.
- <b>Filter by alert type - </b> Only get notified by the alerts you want to receive. If you're planning on using this application to keep you weather-aware, you shouldn't filter any of the alerts as they've been issued to keep you safe. If you're a weather enthusiast however, and have global alerts turned on, you can filter the alerts you receive.
- <b>Stay up to date - </b> Automatically queries the official NWS weather API for new alerts every 30 seconds, helping you stay up to date on the official advisories issued in your area.

## User Guide:
### Download and Setup:
1. Download the latest version's installer from the "Releases" section on the right of this page.
2. Open the "Assets" dropdown, and download SPCDesktopNotificationsWin.msi
3. Run the installer, this will install the app to your program files, create a shortcut in your start menu, and set the app to automatically run on startup.
4. To run the app immediately, search for "SPC Desktop Notifications" in start, and left click it to run (its a background process, so no window will appear when you click it)

### Disable run on Startup:
1. Open "Run" in start.
2. Type "shell:startup" in the textbox and press "ok" (this will open a folder).
3. Delete the shortcut "SPC Desktop Notifications" in the folder that opens.
4. The app will no longer run on startup.

### Options:
1. Open your system tray (the "^" symbol next to your time and date in the bottom right).
2. Find "SPC Desktop Notifications", and right click its icon.
3. This should bring a menu with three options:

- "Open Config", this opens the config file. Editing this file will change the behaviour of the app. Each config option is explained in the file.
- "Apply Config", this restarts the app and applies any new changes made to the config file. Changes will be automatically applied on system start.
- "Exit", this stops the app from running (The app will still run on system start even if previously exited, look at the guide above to disable this).

### How to Uninstall:
The app can be uninstalled either by running the installer and selecting "Remove SPC-Desktop-Notifications", or by navigating to "Add or remove programs" in windows settings, searching for "SPC Desktop Notifications", and clicking "Uninstall".


## FAQ:
1. <b>Is this an official SPC/NWS/US Government product?</b> No, This is a personal project unaffiliated with any company/organisation.
2. <b>Is Mac or Linux supported?</b> Currently Mac or Linux are not supported, only Windows. I might port it to Mac or Linux if there is any demand at all.
3. <b>I can't see the app?</b> The app runs in the background. You can interact with it or exit it by right clicking its icon in the system tray (the ^ symbol in the bottom right of your screen).
4. <b>The setup is complicated! what's the bare minimum for it to work for me?</b> If you want it to only serve alerts for your location, right click on the app when its running in the tray, click "Open Config", change the value of "mode" to "Point", and change the value of "location" to your latitude,longitude coordinates. To apply your changes, right click the app again and click "Apply Config". You'll now receive notifications about all alerts that impact your location.