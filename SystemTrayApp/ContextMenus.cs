using System;
using System.Diagnostics;
using System.Windows.Forms;
using SerenityClientOCR.Properties;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Net;

namespace SerenityClientOCR
{
	public class ContextMenus
	{
		bool isAboutLoaded = false;
        bool isOptionsLoaded = false;
        ToolStripMenuItem toggleItem;
        NotifyIcon ni_parent;
        TimerOptions timerOptions;

		public ContextMenuStrip Create(NotifyIcon _ni, TimerOptions _timerOptions)
		{
            ni_parent = _ni;
            timerOptions = _timerOptions;

			// Add the default menu options.
			ContextMenuStrip menu = new ContextMenuStrip();
			ToolStripMenuItem item;
			ToolStripSeparator sep;

            // Toggle
            item = new ToolStripMenuItem();
            item.Click += new EventHandler(Toggle_Click);
            menu.Items.Add(item);

            toggleItem = item;

            // Separator
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

			// Options
			item = new ToolStripMenuItem();
			item.Text = "Options";
			item.Click += new EventHandler(Options_Click);
			item.Image = Resources.Explorer;
			menu.Items.Add(item);

            // View Error Log
            item = new ToolStripMenuItem();
            item.Text = "View Error Log";
            item.Click += new EventHandler(View_Errorlog_Click);
            item.Image = Resources.Explorer;
            menu.Items.Add(item);

            // Check For Updates
            item = new ToolStripMenuItem();
            item.Text = "Check For Updates";
            item.Click += new EventHandler(Update_Click);
            item.Image = Resources.Explorer;
            menu.Items.Add(item);

			// About
			item = new ToolStripMenuItem();
			item.Text = "About";
			item.Click += new EventHandler(About_Click);
			item.Image = Resources.About;
			menu.Items.Add(item);

			// Separator
			sep = new ToolStripSeparator();
			menu.Items.Add(sep);

			// Exit
			item = new ToolStripMenuItem();
			item.Text = "Exit";
			item.Click += new System.EventHandler(Exit_Click);
			item.Image = Resources.Exit;
			menu.Items.Add(item);

            timerOptions.AddStateOCR(new OCR_State(toggleItem, ni_parent));
            timerOptions.GetStateOCR().SetState("off");
			return menu;
		}

        public class OCR_State
        {
            ToolStripMenuItem toggleItem;
            NotifyIcon ni_parent;

            public OCR_State(ToolStripMenuItem _toggleItem, NotifyIcon _ni_parent)
            {
                toggleItem = _toggleItem;
                ni_parent = _ni_parent;
            }
            public void SetState(string state)
            {
                if (state == "off")
                {
                    toggleItem.Text = "Turn on OCR";
                    ni_parent.Icon = Resources.Off;
                    toggleItem.Image = Resources.On.ToBitmap();
                }
                else if (state == "on")
                {
                    toggleItem.Text = "Turn off OCR";
                    ni_parent.Icon = Resources.On;
                    toggleItem.Image = Resources.Off.ToBitmap();
                }
                else if (state == "error")
                {
                    toggleItem.Text = "Turn on OCR";
                    ni_parent.Icon = Resources.ProgressError;
                    toggleItem.Image = Resources.On.ToBitmap();
                }
            }

        }


        void Toggle_Click(object sender, EventArgs e)
        {
            if (timerOptions.IsWatcherRunning())
            {
                // turn off the watcher
                timerOptions.DeactivateWatcher();
            }
            else
            {
                // turn on the watcher
                timerOptions.ActivateWatcher();
            }
        }

		void Options_Click(object sender, EventArgs e)
		{
            if (!isOptionsLoaded)
            {
                isOptionsLoaded = true;
                OptionsBox optForm = new OptionsBox();
                optForm.LoadTimerOptionsToForm(timerOptions);
                optForm.Show();

                isOptionsLoaded = false;
            }
		}

        void View_Errorlog_Click(object sender, EventArgs e)
		{
            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\log.txt"))
                Process.Start("notepad.exe", System.IO.Directory.GetCurrentDirectory() + "\\log.txt");
		}

        void Update_Click(object sender, EventArgs e)
        {
            // try and download the update XML file
            try
            {
                WebClient Client = new WebClient();
                Client.DownloadFile(timerOptions.strPathURL, System.IO.Directory.GetCurrentDirectory() + "\\update.xml");
            }
            catch (Exception ex)
            {
                timerOptions.SendErrorLog("Error downloading update.xml, error: " + ex.ToString());
            }

            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\update.xml"))
            {
                TimerOptions.UpdaterData updaterData = timerOptions.LoadUpdaterData();
                if (updaterData != null)
                {
                    // check the version
                    if (Assembly.GetExecutingAssembly().GetName().Version.ToString() == updaterData.version)
                        MessageBox.Show("You have the latest version: " + updaterData.version);
                    else
                    {
                        if (MessageBox.Show("Your version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", Server version: " + updaterData.version,
                            "Would you like to navigate to the update directory?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Process.Start(updaterData.PathURL);
                        }
                    }
                    return;
                }
            }
            timerOptions.SendErrorLog("No update.xml File Found!");
        }

		void About_Click(object sender, EventArgs e)
		{
			if (!isAboutLoaded)
			{
				isAboutLoaded = true;
				new AboutBox().Show();
				isAboutLoaded = false;
			}
		}

		void Exit_Click(object sender, EventArgs e)
		{
			// Quit without further ado.
			Application.Exit();
		}
	}
}