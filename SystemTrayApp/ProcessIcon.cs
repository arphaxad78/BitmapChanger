using System;
using System.Diagnostics;
using System.Windows.Forms;
using SerenityClientOCR.Properties;
using System.IO;
using System.Threading;
using System.Reflection;

namespace SerenityClientOCR
{
	class ProcessIcon : IDisposable
	{
		NotifyIcon ni;
        TimerOptions timerOptions;
        ContextMenus contextMenus;

		public ProcessIcon()
		{



			// Instantiate the NotifyIcon object.
			ni = new NotifyIcon();
            timerOptions = new TimerOptions();
            timerOptions.LoadOptions();
            contextMenus = new ContextMenus();
		}

		public void Display()
		{
			// Put the icon in the system tray and allow it react to mouse clicks.			
			ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.Off;
			ni.Text = "Serenity OCR";
			ni.Visible = true;

			// Attach a context menu.
			ni.ContextMenuStrip = contextMenus.Create(ni, timerOptions);

            // start the watcher
            if (Directory.Exists(timerOptions.strScreenshotDirectory))
                timerOptions.ActivateWatcher();
            else
                timerOptions.DeactivateWatcher();

		}

		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			ni.Dispose();
		}


		void ni_MouseClick(object sender, MouseEventArgs e)
		{
			// Handle mouse button clicks.
            //if (e.Button == MouseButtons.Left)
            //{
            //    // Start Windows Explorer.
            //    Process.Start("explorer", null);
            //}
		}

	}
}