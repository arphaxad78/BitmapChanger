/*
 * Copyright 2008 Ruwan Janapriya Egoda Gamage. http://www.janapriya.net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JTessaract
{
    public partial class Options : Form
    {
        public string TesseractBinFolder
        {
            get
            {
                return textBoxTesseractFolder.Text;
            }
            set
            {
                textBoxTesseractFolder.Text = value;
            }
        }

        public string LanguageFontFamily
        {
            get
            {
                return (string)comboBoxFontFamily.SelectedItem;
            }
            set
            {
                if (comboBoxFontFamily.Items.IndexOf(value) > -1)
                    comboBoxFontFamily.SelectedItem = value;
                else
                    comboBoxFontFamily.SelectedIndex = 0;
            }
        }

        public Options()
        {
            InitializeComponent();

            FontFamily[] ff = FontFamily.Families;
            for (int i = 0; i < ff.Length; i++)
            {
                comboBoxFontFamily.Items.Add(ff[i].Name);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                if (!selectedPath.EndsWith("\\"))
                {
                    selectedPath += "\\";
                }

                textBoxTesseractFolder.Text = selectedPath;
            }
        }
    }   
}
