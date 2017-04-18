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
using System.IO;
using System.Collections;
using System.Xml;

namespace JTessaract
{
    public partial class Mainform : Form
    {
        private ArrayList boxes = null;
        private string fileName = null;
        private ArrayList frequentChars = null;
        private JProject project = null;
        private TesseractWrapper wrapper = null;
        private string languageFontFamily = @"Verdana";

        public string LanguageFontFamily
        {
            get { return languageFontFamily; }
            set 
            { 
                languageFontFamily = value;
                textBoxChars.Font = new Font(value, textBoxChars.Font.Size);
                listView.Font = new Font(value, listView.Font.Size);
                textBoxAmbiguity.Font = new Font(value, textBoxAmbiguity.Font.Size);
                textBoxWordsList.Font = new Font(value, textBoxWordsList.Font.Size);
                textBoxFrequentWordsList.Font = new Font(value, textBoxFrequentWordsList.Font.Size);
                textBoxUserWords.Font = new Font(value, textBoxUserWords.Font.Size);
                contextMenuStripCharactorMap.Font = new Font(value, contextMenuStripCharactorMap.Font.Size);
            }
        }
        private bool boxDirty = false;
        private bool projectDirty = false;

        #region Constructor / Dispose
        public Mainform()
        {
            InitializeComponent();
            boxes = new ArrayList();
            frequentChars = new ArrayList();
            wrapper = new TesseractWrapper();
            wrapper.EventCompleted += new TesseractWrapper.EventCompletedDelegate(wrapper_EventCompleted);
        }

        #endregion // Constructor / Dispose

        #region Events Subscribed (Non UI)
        void wrapper_EventCompleted(string tesseractLog, object result)
        {
            textBoxTesseractLog.AppendText(Environment.NewLine + tesseractLog);
        }

        #endregion // Events Subscribed (Non UI)

        #region Events Subscribed (UI)
        private void Mainform_Load(object sender, EventArgs e)
        {
            LoadConfiguration(GetAssemblyFolder());
            panel2.Location = new Point(3, 3);

            foreach (string charactor in frequentChars)
            {
                ToolStripCharMapMenuItem item = new ToolStripCharMapMenuItem(charactor);
                item.Text = charactor;
                item.CharacterMapClickEvent += new CharacterMapClickEventDelegate(menu_CharacterMapClickEvent);
                contextMenuStripCharactorMap.Items.Add(item);
            }
        }

        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((project != null) && projectDirty)
            {
                if (MessageBox.Show("Do you want to save the current project?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProject();
                }
            }
        }

        #endregion // Events Subscribed (UI)

        private string GetAssemblyFolder()
        {
            return Path.GetDirectoryName(GetType().Assembly.CodeBase.Replace("file:///", String.Empty));
        }

        #region Config.xml
        private bool LoadConfiguration(string path)
        {
            string configurationFile = path + @"\config.xml";
            bool operationSuccess = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configurationFile);

                XmlElement rootElement = doc.DocumentElement;

                XmlNode tesseractBinElement = rootElement.SelectSingleNode("tesseract-bin");

                wrapper.TesseractBinaryFolder = tesseractBinElement.InnerText;

                XmlNode frequentCharElement = rootElement.SelectSingleNode("frequent-char");
                

                XmlNodeList currentNodeList = frequentCharElement.SelectNodes("char");
                foreach (XmlNode node in currentNodeList)
                {
                    frequentChars.Add(node.InnerText);
                }

                operationSuccess = true;
            }
            catch
            {
                operationSuccess = false;
            }

            return operationSuccess;
        }

        private bool SaveConfiguration(string path)
        {
            string configurationFile = path + @"\config.xml";
            bool operationSuccess = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                
                doc.Load(configurationFile);

                XmlElement rootElement = doc.DocumentElement;

                XmlNode tesseractBinElement = rootElement.SelectSingleNode("tesseract-bin");

                tesseractBinElement.InnerText = wrapper.TesseractBinaryFolder;

                doc.Save(configurationFile);

                operationSuccess = true;
            }
            catch
            {
                operationSuccess = false;
            }

            return operationSuccess;
        }

        #endregion // Config.xml

        #region BoxEditor UI functions

        #region Events
        private void textBoxChars_TextChanged(object sender, EventArgs e)
        {
            string str = textBoxChars.Text;
            if (str.Length > 0)
            {
                textBoxUnicode1.Text = String.Format("{0:x4}", (int)str[0]).ToUpper();
                if (str.Length > 1)
                {
                    textBoxUnicode2.Text = String.Format("{0:x4}", (int)str[1]).ToUpper();
                    if (str.Length > 2)
                    {
                        textBoxUnicode3.Text = String.Format("{0:x4}", (int)str[2]).ToUpper();
                        if (str.Length > 3)
                        {
                            textBoxUnicode4.Text = String.Format("{0:x4}", (int)str[3]).ToUpper();
                            if (str.Length > 4)
                            {
                                textBoxUnicode5.Text = String.Format("{0:x4}", (int)str[4]).ToUpper();
                                if (str.Length > 5)
                                {
                                    textBoxUnicode6.Text = String.Format("{0:x4}", (int)str[5]).ToUpper();
                                }
                                else
                                {
                                    textBoxUnicode6.Text = "";
                                }
                            }
                            else
                            {
                                textBoxUnicode5.Text = "";
                                textBoxUnicode6.Text = "";
                            }
                        }
                        else
                        {
                            textBoxUnicode4.Text = "";
                            textBoxUnicode5.Text = "";
                            textBoxUnicode6.Text = "";
                        }
                    }
                    else
                    {
                        textBoxUnicode3.Text = "";
                        textBoxUnicode4.Text = "";
                        textBoxUnicode5.Text = "";
                        textBoxUnicode6.Text = "";
                    }
                }
                else
                {
                    textBoxUnicode2.Text = "";
                    textBoxUnicode3.Text = "";
                    textBoxUnicode4.Text = "";
                    textBoxUnicode5.Text = "";
                    textBoxUnicode6.Text = "";
                }
            }
            else
            {
                textBoxUnicode1.Text = "";
                textBoxUnicode2.Text = "";
                textBoxUnicode3.Text = "";
                textBoxUnicode4.Text = "";
                textBoxUnicode5.Text = "";
                textBoxUnicode6.Text = "";
            }
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 2)
            {
                // consecutive boxes assumed
                int firstIndex = listView.SelectedItems[0].Index;
                int secondIndex = listView.SelectedItems[1].Index;

                Box box1 = (Box)boxes[firstIndex];
                Box box2 = (Box)boxes[secondIndex];

                box1.X2 = box2.X2;
                box1.Y2 = box2.Y2;

                ListViewItem item = listView.Items[firstIndex];
                item.SubItems[3].Text = box1.X2.ToString();
                item.SubItems[4].Text = box1.Y2.ToString();

                boxes.RemoveAt(secondIndex);
                listView.Items.RemoveAt(secondIndex);

                if (listView.Items.Count > firstIndex)
                {
                    listView.Items[firstIndex].Selected = true;
                }

                canvas.Invalidate();
                preview.Invalidate();
                previewPerChar.Invalidate();
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (listView.SelectedItems.Count == 1)
            {
                addAfterToolStripMenuItem.Enabled = true;
                addBeforeToolStripMenuItem.Enabled = true;
                splitToolStripMenuItem.Enabled = true;
                mergeToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = true;
            }
            else if (listView.SelectedItems.Count == 2)
            {
                if (1 != Math.Abs(listView.SelectedItems[0].Index - listView.SelectedItems[1].Index))
                {
                    mergeToolStripMenuItem.Enabled = false;
                }
                else
                {
                    mergeToolStripMenuItem.Enabled = true;
                }

                addAfterToolStripMenuItem.Enabled = false;
                addBeforeToolStripMenuItem.Enabled = false;
                splitToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = true;
            }
            else if (listView.SelectedItems.Count > 2)
            {
                addAfterToolStripMenuItem.Enabled = false;
                addBeforeToolStripMenuItem.Enabled = false;
                splitToolStripMenuItem.Enabled = false;
                mergeToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = true;
            }
        }

        private void buttonAddSourceImages_Click(object sender, EventArgs e)
        {
            ImportImages_Step_1_1();
        }

        void menu_CharacterMapClickEvent(string charactor)
        {
            textBoxChars.AppendText(charactor);
        }

        private void buttonCharactorMap_Click(object sender, EventArgs e)
        {
            contextMenuStripCharactorMap.Show(buttonCharactorMap, 0, 0);
        }

        private void splitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Investigate tesseract more to implement this function
            if (listView.SelectedIndices.Count > 0)
            {
                int index = listView.SelectedIndices[0];
                Box boxOriginal = (Box)boxes[index];
                Box boxNew = new Box();
                boxNew.Charactor = boxOriginal.Charactor;
                boxNew.X2 = boxOriginal.X2;
                boxNew.Y2 = boxOriginal.Y2;
                boxOriginal.X2 = (boxOriginal.X1 + boxOriginal.X2) / 2;
                boxNew.X1 = boxOriginal.X2;
                boxNew.Y1 = boxOriginal.Y1;

                ListViewItem item = new ListViewItem(boxNew.Charactor);
                item.SubItems.Add(boxNew.X1.ToString());
                item.SubItems.Add(boxNew.Y1.ToString());
                item.SubItems.Add(boxNew.X2.ToString());
                item.SubItems.Add(boxNew.Y2.ToString());

                if (boxes.Count <= index + 1)
                {
                    boxes.Add(boxNew);
                    listView.Items.Add(item);
                }
                else
                {
                    boxes.Insert(index + 1, boxNew);
                    listView.Items.Insert(index + 1, item);
                }

                canvas.Invalidate();

                // TODO: Handle Preview
                preview.Invalidate();
                previewPerChar.Invalidate();
            }
        }

        private void buttonSetCharactors_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count > 0)
            {
                int index = listView.SelectedIndices[0];
                ((Box)boxes[index]).Charactor = textBoxChars.Text;
                listView.Items[index].Text = textBoxChars.Text;
                canvas.Invalidate();
                boxDirty = true;
            }
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                Box box = ((Box)boxes[e.ItemIndex]);
                canvas.CurrentBox = e.ItemIndex;
                canvas.Invalidate();
                preview.CurrentBox = box;
                previewPerChar.CurrentBox = box;
                textBoxChars.Text = box.Charactor;
                preview.Invalidate();
                previewPerChar.Invalidate();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count > 0)
            {
                int lastSelect = 0;
                int[] selectedIndices = new int[listView.SelectedItems.Count];
                for (int i = 0; i < listView.SelectedItems.Count; i++)
                {
                    selectedIndices[i] = listView.SelectedItems[i].Index;
                }

                Array.Sort<int>(selectedIndices);

                lastSelect = selectedIndices[0];

                for (int i = selectedIndices.Length - 1; i >= 0; i--)
                {
                    boxes.RemoveAt(selectedIndices[i]);
                    listView.Items.RemoveAt(selectedIndices[i]);
                }

                if (listView.Items.Count > lastSelect)
                {
                    listView.Items[lastSelect].Selected = true;
                }
                else
                {
                    if ((listView.Items.Count == lastSelect) && (listView.Items.Count > 0))
                    {
                        listView.Items[lastSelect - 1].Selected = true;
                    }
                }

                canvas.Invalidate();
                preview.Invalidate();
                previewPerChar.Invalidate();
            }
        }

        private void listViewSourceImages_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (boxDirty)
            {
                if (MessageBox.Show("Box is changed, do you want to save changes?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    SaveBoxFile();
            }

            fileName = project.ProjectFolder + e.Item.SubItems[1].Text;
            LoadImage();
            LoadBoxFile();
            LoadTrainingFile();
        }

        #endregion // Events

        #region Helper Functions
        public bool UpdateListViewSourceImages()
        {
            if (project != null)
            {
                int imagesCount = project.SourceImages.Count;

                listViewSourceImages.Items.Clear();

                for (int i = 0; i < imagesCount; i++)
                {
                    ListViewItem item = listViewSourceImages.Items.Add(new ListViewItem(new string[] {(i + 1).ToString(), project.SourceImages[i].ToString(), "yes"}));
                    //item.SubItems.Add(project.SourceImages[i].ToString());
                    //tem.SubItems.Add("yes");
                }
            }

            return true;
        }

        private void LoadImage()
        {
            canvas.MainImage = Bitmap.FromFile(fileName);
            preview.MainImage = canvas.MainImage;
            previewPerChar.MainImage = canvas.MainImage;

            fileName = fileName.Substring(0, fileName.LastIndexOf(".")) + ".box";
            if (File.Exists(fileName))
            {
                // if (MessageBox.Show("Is " + fileName + " the box file?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    LoadBoxFile(fileName);
                }
            }

            if (boxes.Count > 0)
            {
                canvas.Boxes = boxes;
                listView.Items.Clear();

                for (int i = 0; i < boxes.Count; i++)
                {
                    Box box = (Box)boxes[i];
                    ListViewItem item = new ListViewItem(box.Charactor);
                    item.SubItems.Add(box.X1.ToString());
                    item.SubItems.Add(box.Y1.ToString());
                    item.SubItems.Add(box.X2.ToString());
                    item.SubItems.Add(box.Y2.ToString());

                    listView.Items.Add(item);
                }

                if (listView.Items.Count > 0)
                {
                    listView.Items[0].Selected = true;
                }
            }

            canvas.Invalidate();
            preview.Invalidate();
            previewPerChar.Invalidate();
        }

        private void LoadBoxFile(string fileName)
        {
            boxes.Clear();
            StreamReader stream = new StreamReader(fileName, Encoding.UTF8);
            string currentLine = null;

            currentLine = stream.ReadLine();

            while (currentLine != null)
            {
                try
                {
                    string[] pieces = currentLine.Split(' ');
                    if (pieces.Length >= 5)
                    {
                        Box box = new Box();

                        box.Charactor = pieces[0];
                        box.X1 = Int16.Parse(pieces[1]);
                        box.Y1 = Int16.Parse(pieces[2]);
                        box.X2 = Int16.Parse(pieces[3]);
                        box.Y2 = Int16.Parse(pieces[4]);

                        boxes.Add(box);
                    }
                    else
                    {
                        Logger.Warning("Box file has a incomplete line: " + currentLine);
                    }
                }
                catch
                {
                    Logger.Warning("Box file reading triggered an exception on line: " + currentLine);
                }

                currentLine = stream.ReadLine();
            }

            stream.Close();

        }

        private void LoadBoxFile()
        {
            string boxFile = fileName.Substring(0, fileName.LastIndexOf('.'));
            boxFile += ".box";

            if (File.Exists(boxFile))
            {
                textBoxBoxFile.Enabled = true;
                textBoxBoxFile.Text = File.ReadAllText(boxFile);
            }
            else
            {
                textBoxBoxFile.Enabled = false;
            }
        }

        private void LoadTrainingFile()
        {
            string trainingFile = fileName.Substring(0, fileName.LastIndexOf('.'));
            trainingFile += ".tr";

            if (File.Exists(trainingFile))
            {
                textBoxTrainingFile.Enabled = true;
                textBoxTrainingFile.Text = File.ReadAllText(trainingFile);
            }
            else
            {
                textBoxTrainingFile.Enabled = false;
            }
        }

        private void SaveBoxFile()
        {
            fileName = fileName.Substring(0, fileName.LastIndexOf(".")) + ".box";
            StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);

            for (int i = 0; i < boxes.Count; i++)
            {
                Box box = (Box)boxes[i];
                string currentLine = String.Format("{0} {1} {2} {3} {4}", box.Charactor, box.X1.ToString(), box.Y1.ToString(), box.X2.ToString(), box.Y2.ToString()); ;
                stream.WriteLine(currentLine);
            }

            boxDirty = false;

            stream.Close();
        }

        #endregion // Helper Functions

        #endregion // BoxEditor UI functions

        //public bool ImportImages()
        //{
        //    if (project == null)
        //    {
        //        NewProject();
        //    }

        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "TIFF (*.tif, *.tiff)|*.tif;*.tiff";
        //    openFileDialog.Multiselect = true;

        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        int count = openFileDialog.FileNames.Length;
        //        for (int i = 0; i < count; i++)
        //        {
        //            string fileName = openFileDialog.FileNames[i];
        //            project.ImportSourceImage(fileName);
        //        }
        //    }

        //    CreateBoxesWithoutOverwriting();

        //    UpdateListViewSourceImages();

        //    return true;
        //}

        //public void CreateBoxesWithoutOverwriting()
        //{
        //    int count = project.SourceImages.Count;
            
        //    for (int i = 0; i < count; i++)
        //    {
        //        string fileName = (string)project.SourceImages[i];
        //        string fileNameSegment = fileName.Substring(0, fileName.LastIndexOf('.'));

        //        if (!File.Exists(project.ProjectFolder + fileNameSegment + ".box"))
        //        {
        //            wrapper.CreateBoxFile(project.ProjectFolder, fileName);
        //        }
        //    }
        //}               


        #region Project functions

        private bool NewProject()
        {
            if ((project != null) && (projectDirty || boxDirty))
            {
                if (MessageBox.Show("Do you want to save the current project?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProject();
                }
            }

            CloseProject();

            string projectFilePath = null;
            bool existingProject = true;
            string projectFolderPath = null;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JTesseract Project Files (*.tes)|*.tes";
            ProjectSettings projectSettings = new ProjectSettings();

            projectSettings.LanguageName = "eng";

            if (projectSettings.ShowDialog() == DialogResult.OK)
            {
                while (existingProject)
                {
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        projectFilePath = saveFileDialog.FileName;


                        projectFolderPath = projectFilePath.Substring(0, projectFilePath.LastIndexOf('.'));

                        if (Directory.Exists(projectFolderPath))
                        {
                            if (MessageBox.Show("Do you want to overwrite the existing folder: " + projectFolderPath, "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                existingProject = false;
                            }
                        }
                        else
                        {
                            existingProject = false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            project = new JProject();
            project.LanguageName = projectSettings.LanguageName; // TODO: validate the language name, example - 3 letter language code, invalid chars etc..
            project.NewProject(projectFilePath);

            this.Text = "JTesseract - " + projectFilePath;

            textBoxFrequentWordsList.Text = File.ReadAllText(projectFolderPath + @"\frequent_words.txt");
            textBoxWordsList.Text = File.ReadAllText(projectFolderPath + @"\words.txt");
            textBoxUserWords.Text = File.ReadAllText(projectFolderPath + @"\user_words.txt");
            textBoxAmbiguity.Text = File.ReadAllText(projectFolderPath + @"\ambiguities.txt");
            projectDirty = false;

            return UpdateListViewSourceImages(); 
        }

        private bool SaveProject()
        {
            if (project == null)
            {
                return false;
            }

            File.WriteAllText(project.ProjectFolder + @"\frequent_words.txt", textBoxFrequentWordsList.Text, Encoding.UTF8);
            File.WriteAllText(project.ProjectFolder + @"\words.txt", textBoxWordsList.Text, Encoding.UTF8);
            File.WriteAllText(project.ProjectFolder + @"\user_words.txt", textBoxUserWords.Text, Encoding.UTF8);
            File.WriteAllText(project.ProjectFolder + @"\ambiguities.txt", textBoxAmbiguity.Text, Encoding.UTF8);

            projectDirty = false;

            if (boxDirty)
            {
                SaveBoxFile();
            }

            return project.SaveProject();
        }

        private bool OpenProject()
        {
            if ((project != null) && (projectDirty || boxDirty))
            {
                if (MessageBox.Show("Do you want to save the current project?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProject();
                }
            }

            CloseProject();

            project = new JProject();
            string projectFilePath = null;
            bool existingProject = false;
            string projectFolderPath = null;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JTesseract Project Files (*.tes)|*.tes";

            while (!existingProject)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    projectFilePath = openFileDialog.FileName;

                    projectFolderPath = projectFilePath.Substring(0, projectFilePath.LastIndexOf('.'));

                    if (!Directory.Exists(projectFolderPath))
                    {
                        MessageBox.Show("Seems like a currupted project: " + projectFolderPath + ". Please try another!", "JTesseract", MessageBoxButtons.OK);
                    }
                    else
                    {
                        existingProject = true;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (project.LoadProject(projectFilePath))
            {
                textBoxFrequentWordsList.Text = File.ReadAllText(projectFolderPath + @"\frequent_words.txt");
                textBoxWordsList.Text = File.ReadAllText(projectFolderPath + @"\words.txt");
                textBoxUserWords.Text = File.ReadAllText(projectFolderPath + @"\user_words.txt");
                textBoxAmbiguity.Text = File.ReadAllText(projectFolderPath + @"\ambiguities.txt");

                this.Text = "JTesseract - " + projectFilePath;
            }

            projectDirty = false;

            return UpdateListViewSourceImages();
        }

        private void CloseProject()
        {
            if ((project != null) && (projectDirty || boxDirty))
            {
                if (MessageBox.Show("Do you want to save the current project?", "JTesseract", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveProject();
                }
            }

            project = null;
            textBoxFrequentWordsList.Text = "";
            textBoxWordsList.Text = "";
            textBoxUserWords.Text = "";
            textBoxAmbiguity.Text = "";

            projectDirty = false;
            boxDirty = false;

            canvas.CurrentBox = -1;
            preview.CurrentBox = null;
            previewPerChar.CurrentBox = null;
            canvas.MainImage = null;
            preview.MainImage = null;
            previewPerChar.MainImage = null;
            listViewSourceImages.Items.Clear();
            listView.Items.Clear();
            textBoxChars.Text = "";
            textBoxUnicode1.Text = "";
            textBoxUnicode2.Text = "";
            textBoxUnicode3.Text = "";
            textBoxUnicode4.Text = "";
            textBoxUnicode5.Text = "";
            textBoxUnicode6.Text = "";
            textBoxBoxFile.Text = "";
            textBoxTrainingFile.Text = "";
            textBoxTesseractLog.Text = "";

            this.Text = "JTesseract";
        }

        #endregion // Project Functions

        private void SaveChangesOfBoxFile()
        {
            string boxFile = fileName.Substring(0, fileName.LastIndexOf('.'));
            boxFile += ".box";

            if (File.Exists(boxFile))
            {
                File.WriteAllText(boxFile, textBoxBoxFile.Text, Encoding.UTF8);
            }
        }

        private void SaveChangesOfTrainingFile()
        {
            string trainingFile = fileName.Substring(0, fileName.LastIndexOf('.'));
            trainingFile += ".tr";

            if (File.Exists(trainingFile))
            {
                File.WriteAllText(trainingFile, textBoxTrainingFile.Text, Encoding.UTF8);
            }
        }

        #region Advanced Steps 
        // project should be existing this to be called. 
        private bool ImportImages_Step_1_1()
        {
            if (project == null)
                return false;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TIFF (*.tif, *.tiff)|*.tif;*.tiff";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int count = openFileDialog.FileNames.Length;
                for (int i = 0; i < count; i++)
                {
                    string fileName = openFileDialog.FileNames[i];
                    project.ImportSourceImage(fileName);
                }
            }

            return UpdateListViewSourceImages();
        }

        private bool GenerateBoxFiles_Step_1_2(bool overwriteIfExisting)
        {
            return wrapper.CreateBoxFiles(project.ProjectFolder, project.SourceImages, overwriteIfExisting);
        }

        private bool CreateTRFiles_Step_2_1()
        {
            return wrapper.CreateTrainingFiles(project.ProjectFolder, project.SourceImages, true);
        }

        private bool Cluster_Step_2_2()
        {
            return wrapper.PerformClustering(project.ProjectFolder, project.SourceImages);
        }

        private bool CharSet_Step_3_1()
        {
            return wrapper.GenerateUniCharSet(project.ProjectFolder, project.SourceImages);
        }

        private bool Dictionary_Step_3_2()
        {
            return wrapper.CreateDictionaries(project.ProjectFolder);
        }

        private bool Ambiguity_Step_3_3()
        {
            return wrapper.CreateDangAmbigs(project.ProjectFolder);
        }

        private bool SaveLanguagePack_Step_4_1(string languageName)
        {
            return wrapper.CreateLanguagePack(project.ProjectFolder, languageName);
        }

        #endregion // Advanced Steps

        #region Step buttons
        private void buttonLoadImages_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                NewProject();
            }

            ImportImages_Step_1_1();
            // TODO: In multi-thread implementation this should go through a state machine.
            GenerateBoxFiles_Step_1_2(false);
        }

        private void buttonGenerateTrainingNClustering_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            CreateTRFiles_Step_2_1();
            Cluster_Step_2_2();
        }

        private void buttonGenerateDictionary_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            CharSet_Step_3_1();
            Dictionary_Step_3_2();
            Ambiguity_Step_3_3();
        }

        private void buttonSaveLanguagePack_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            ProjectSettings projectSettings = new ProjectSettings();

            projectSettings.LanguageName = project.LanguageName;

            if (projectSettings.ShowDialog() == DialogResult.OK)
            {
                SaveLanguagePack_Step_4_1(projectSettings.LanguageName);
            }          
        }      

        private void buttonImportedImages_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            ImportImages_Step_1_1();
        }

        private void buttonGenerateBoxFiles_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }
            GenerateBoxFiles_Step_1_2(false);
        }

        private void buttonCreateTRFiles_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            CreateTRFiles_Step_2_1();
        }

        private void buttonCluster_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            Cluster_Step_2_2();
        }

        private void buttonCharSet_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            CharSet_Step_3_1();
        }

        private void buttonDictionary_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            Dictionary_Step_3_2();
        }

        private void buttonAmbiguity_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            Ambiguity_Step_3_3();
        }
        
        private void buttonSaveLangPack_Click(object sender, EventArgs e)
        {
            if (project == null)
            {
                return;
            }

            SaveLanguagePack_Step_4_1("ras");
        }
        
        #endregion // Step buttons

        #region Menu
        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options options = new Options();
            options.TesseractBinFolder = wrapper.TesseractBinaryFolder;
            options.LanguageFontFamily = languageFontFamily;
            if (options.ShowDialog() == DialogResult.OK)
            {
                wrapper.TesseractBinaryFolder = options.TesseractBinFolder;
                LanguageFontFamily = options.LanguageFontFamily;
                if (project != null)
                    project.LanguageFontFamily = options.LanguageFontFamily;

                SaveConfiguration(GetAssemblyFolder());
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        #region Depricated
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TIF (*.tif,*.tiff)|*.tif;*.tiff|BMP (*.bmp)|*.bmp|PNG (*.png)|*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
                LoadImage();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileName = fileName.Substring(0, fileName.LastIndexOf(".")) + ".box";
            StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);

            for (int i = 0; i < boxes.Count; i++)
            {
                Box box = (Box)boxes[i];
                string currentLine = String.Format("{0} {1} {2} {3} {4}", box.Charactor, box.X1.ToString(), box.Y1.ToString(), box.X2.ToString(), box.Y2.ToString()); ;
                stream.WriteLine(currentLine);
            }

            stream.Close();
        }


        #endregion // Depricated

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (boxDirty)
            {
                SaveBoxFile();
            }
        }


        #endregion // Menu

        private void textBoxWordsList_TextChanged(object sender, EventArgs e)
        {
            projectDirty = true;
        }

        private void textBoxFrequentWordsList_TextChanged(object sender, EventArgs e)
        {
            projectDirty = true;
        }

        private void textBoxUserWords_TextChanged(object sender, EventArgs e)
        {
            projectDirty = true;
        }

        private void textBoxAmbiguity_TextChanged(object sender, EventArgs e)
        {
            projectDirty = true;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void buttonAdvancedMode_Click(object sender, EventArgs e)
        {
            if (buttonAdvancedMode.Text == "Advanced Mode")
            {
                buttonAdvancedMode.Text = "Simple Mode";
                panel2.Visible = true;
            }
            else
            {
                buttonAdvancedMode.Text = "Advanced Mode";
                panel2.Visible = false;
            }
        }


        private void buttonNavigatePrevious_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count > 0)
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    int index = listView.SelectedIndices[0];
                    if (index > 0)
                    {
                        listView.Items[index].Selected = false;
                        listView.Items[index - 1].Selected = true;
                    }
                }
            }
        }

        private void buttonNavigateEnd_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count > 0)
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    listView.Items[listView.SelectedIndices[0]].Selected = false;
                }
                listView.Items[listView.Items.Count - 1].Selected = true;
            }
        }

        private void buttonNavigateFirst_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count > 0)
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    listView.Items[listView.SelectedIndices[0]].Selected = false;
                }
                listView.Items[0].Selected = true;
            }
        }

        private void buttonNavigateNext_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count > 0)
            {
                if (listView.SelectedIndices.Count > 0)
                {
                    int index = listView.SelectedIndices[0];
                    if (index < listView.Items.Count - 1)
                    {
                        listView.Items[index].Selected = false;
                        listView.Items[index + 1].Selected = true;
                    }
                }
            }
        }

        private void buttonRemoveSourceImages_Click(object sender, EventArgs e)
        {

        }
    }

    public enum TrainingStep
    {
        NEW_PROJECT = 0,
        IMAGES_IMPORTED = 1,
        BOXES_CREATED = 2,
        TR_FILES_CREATED = 3,
        CLUSTERED = 4,
        CHARSET_CREATED = 5,
        DICTIONARY_CREATED = 6,
        AMBIGUITY_FIXED = 7,
        LANGUAGE_PACK_CREATED = 8
    }
}