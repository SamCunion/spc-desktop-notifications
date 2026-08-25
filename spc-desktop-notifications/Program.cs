using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace spc_desktop_notifications
{
    internal class Program
    {

        private static NotifyIcon? trayIcon;
        private static SpcDesktopNotifications app;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //system tray functions
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            app = new SpcDesktopNotifications();


            //context menu
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show config", null, OnShowConfigClicked);
            trayMenu.Items.Add("Apply config", null, OnApplyConfigClicked);
            trayMenu.Items.Add("Exit", null, OnExitClicked);

            //app icon
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Text = "SPC Desktop Notifications",
                Visible = true,
                ContextMenuStrip = trayMenu
            };

            Application.Run();
        }

        //event triggers when the "Exit" button is clicked on the tray icon context menu
        static void OnExitClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("Exit clicked.");
            trayIcon!.Visible = false;
            Application.Exit();
        }

        //event triggers when the "Show Config" button is clicked on the tray icon context menu
        static void OnShowConfigClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("Show Config clicked.");
            try
            {
                Process.Start(new ProcessStartInfo { FileName = ConfigManager.cfgPath, UseShellExecute = true });
            }
            catch(Exception err)
            {
                Console.WriteLine("Error while opening config file.");
            }
        }

        //event triggers when the "Apply Config" button is clicked on the tray icon context menu
        static void OnApplyConfigClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("Apply Config clicked.");
            app.Stop();
            app = new SpcDesktopNotifications();
        }
    }
}
