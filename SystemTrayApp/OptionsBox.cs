using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SerenityClientOCR
{
    public partial  class OptionsBox : Form
    {
        public TimerOptions timerOptions;  

        public OptionsBox()
        {
            InitializeComponent();
        }

        public void LoadTimerOptionsToForm(TimerOptions _timerOptions)
        {                       
            timerOptions = _timerOptions;// and then store it internally
            
            txtPath.Text = timerOptions.strScreenshotDirectory;
            if (timerOptions.bThreeMonitors)
                radio2.Checked = true;
            else
                radio1.Checked = true;
            txtLeftBezel.Text = timerOptions.intLeftBezel.ToString();
            txtRightBezel.Text = timerOptions.intRightBezel.ToString();
            txtUsername.Text = timerOptions.strUsername;
            txtPathURL.Text = timerOptions.strPathURL;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // save

            // if there is a backslash at the end, remove it
            if (txtPath.Text.Substring(txtPath.Text.Length - 1, 1) == "\\")
                txtPath.Text = txtPath.Text.Substring(0, txtPath.Text.Length - 1);

            string response = ValidateTextBoxes();
            if (response == "")
            {
                timerOptions.strScreenshotDirectory = txtPath.Text;
                timerOptions.bThreeMonitors = radio2.Checked;
                if (timerOptions.bThreeMonitors)
                {
                    timerOptions.intLeftBezel = int.Parse(txtLeftBezel.Text);
                    timerOptions.intRightBezel = int.Parse(txtRightBezel.Text);
                }
                else
                {
                    timerOptions.intLeftBezel = 0;
                    timerOptions.intRightBezel = 0;
                }
                timerOptions.strUsername = txtUsername.Text;
                timerOptions.strPathURL = txtPathURL.Text;
                timerOptions.SaveOptions();

                this.Close();
            }
            else
            {
                MessageBox.Show(response, "Incorrect Data in Settings");
            }
        }

        private string ValidateTextBoxes()
        {
            if (!Directory.Exists(txtPath.Text))
                return "The screenshot directory you provided does not exist!";
            if (txtUsername.Text == "")
                return "You must have a username!";
            if (txtUsername.Text.IndexOf(" ") > -1)
                return "Spaces are not allowed in the Username field!";
            if (txtUsername.Text.IndexOf("-") > -1)
                return "Dashes are not allowed in the Username field!";
            if (txtUsername.Text.IndexOf("_") > -1)
                return "Underscores are not allowed in the Username field!";
            if (txtPathURL.Text == "")
                return "The Path URL cannot be blank!";

            int x;
            if (!int.TryParse(txtLeftBezel.Text, out x))
                return "The left bezel amount needs to be a number!";
            if (x < 0)
                return "The left bezel amount must be positive!";
            if (!int.TryParse(txtRightBezel.Text, out x))
                return "The right bezel amount needs to be a number!";
            if (x < 0)
                return "The right bezel amount must be positive!";

            return "";
        }

        private void RadioButtonsChanged(bool oneMonitorSelected)
        {
            txtLeftBezel.Enabled = !oneMonitorSelected;
            txtRightBezel.Enabled = !oneMonitorSelected;
        }

        private void radio1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButtonsChanged(radio1.Checked);
        }

        private void radio2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButtonsChanged(radio1.Checked);
        }


    }
}
