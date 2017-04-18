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
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;

namespace JTessaract
{
    class JProject
    {
        private string languageName = "ras";//null;
        private string version = "1.0"; //null;
        private string languageFontFamily = @"Verdana";

        public string LanguageFontFamily
        {
            get { return languageFontFamily; }
            set { languageFontFamily = value; }
        }

        public string LanguageName
        {
            get { return languageName; }
            set { languageName = value; }
        }

        private string projectFile = null;
        private string projectFolder = null;

        public string ProjectFolder
        {
            get { return projectFolder; }
            set 
            {
                projectFolder = value;

                if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
                {
                    projectFolder += '\\';
                }                
            }
        } 

        // Following file locations are locations with respect to projectLocation.
        private string frequentWordListFile = null; 
        private string wordsListFile = null;
        private string dangAmbigsFile = null;

        // Once images are imported to the project, they are copied to the project location.
        private ArrayList sourceImages = null;

        public ArrayList SourceImages
        {
            get { return sourceImages; }
            set { sourceImages = value; }
        }
        private ArrayList boxFiles = null;

        // Overriding certain steps
        private bool overrideInitialBoxFileCreation = true;
        private bool overrideTrainingFileCreation = false;
        private bool overrideClustering = false;
        private bool overrideUnicharset = false;
        private bool overrideDictionary = false;
        private bool overrideDangAmbs = false;

        public JProject()
        {
            sourceImages = new ArrayList();
            boxFiles = new ArrayList();
        }

        public bool NewProject(string projectFile)
        {
            this.projectFile = projectFile;
            ProjectFolder = projectFile.Substring(0, projectFile.LastIndexOf('.'));

            if (Directory.Exists(projectFolder))
            {
                Directory.Delete(projectFolder, true); // I know this is bad. But we have to make sure we are not mixing things up.
            }

            Directory.CreateDirectory(projectFolder);
            File.WriteAllText(projectFolder + @"\frequent_words.txt", "", Encoding.UTF8);
            File.WriteAllText(projectFolder + @"\words.txt", "", Encoding.UTF8);
            File.WriteAllText(projectFolder + @"\user_words.txt", "", Encoding.UTF8);
            File.WriteAllText(projectFolder + @"\ambiguities.txt", "", Encoding.UTF8);
            return true;
        }

        public bool LoadProject(string projectFile)
        {
            this.projectFile = projectFile;
            ProjectFolder = projectFile.Substring(0, projectFile.LastIndexOf('.'));
            bool operationSuccess = false;
           
            if (!Directory.Exists(projectFolder))
                return false; // There should be a folder.

            try
            {
                /*
                  <jtesseract-project>
                      <version>1.0</version>
                      <language-pack-name>ras</language-pack-name>
                      <training-images>
                             <image>train01.tif</image>
                             <image>train02.tif</image>
                             <image>train03.tif</image>
                             <image>train04.tif</image>
                             <image>train05.tif</image>
                      </training-images>
                 </jtesseract-project>
                  * */
                XmlDocument doc = new XmlDocument();
                doc.Load(projectFile);

                XmlElement rootElement = doc.DocumentElement;

                XmlNode languagePackNameElement = rootElement.SelectSingleNode("language-pack-name");
                languageName = languagePackNameElement.InnerText;
                XmlNode versionElement = rootElement.SelectSingleNode("version");
                version = versionElement.InnerText;
                XmlNode trainingImagesElement = rootElement.SelectSingleNode("training-images");

                XmlNodeList currentNodeList = trainingImagesElement.SelectNodes("image");
                foreach (XmlNode node in currentNodeList)
                {
                    sourceImages.Add(node.InnerText);
                }

                if (!File.Exists(projectFolder + @"\frequent_words.txt"))
                    File.WriteAllText(projectFolder + @"\frequent_words.txt", "", Encoding.UTF8);
                if (!File.Exists(projectFolder + @"\words.txt"))
                    File.WriteAllText(projectFolder + @"\words.txt", "", Encoding.UTF8);
                if (!File.Exists(projectFolder + @"\user_words.txt"))
                    File.WriteAllText(projectFolder + @"\user_words.txt", "", Encoding.UTF8);
                if (!File.Exists(projectFolder + @"\ambiguities.txt"))
                    File.WriteAllText(projectFolder + @"\ambiguities.txt", "", Encoding.UTF8);

                operationSuccess = true;
            }
            catch
            {
                operationSuccess = false;
            }

            return operationSuccess;
        }

        public bool SaveProject()
        {
            bool operationSuccess = false;
            try
            {
                /*
                  <jtesseract-project>
                      <version>1.0</version>
                      <language-pack-name>ras</language-pack-name>
                      <training-images>
                             <image>train01.tif</image>
                             <image>train02.tif</image>
                             <image>train03.tif</image>
                             <image>train04.tif</image>
                             <image>train05.tif</image>
                      </training-images>
                 </jtesseract-project>
                 */
                XmlDocument doc = new XmlDocument();
                
                XmlElement rootElement = doc.CreateElement("jtesseract-project");
                doc.AppendChild(rootElement);

                XmlElement languagePackNameElement = doc.CreateElement("language-pack-name");
                languagePackNameElement.InnerText = this.languageName;
                rootElement.AppendChild(languagePackNameElement);

                XmlElement versionElement = doc.CreateElement("version");
                versionElement.InnerText = version;
                rootElement.AppendChild(versionElement);

                XmlElement trainingImagesElement = doc.CreateElement("training-images");
                rootElement.AppendChild(trainingImagesElement);

                foreach (string imagefile in sourceImages)
                {
                    XmlElement image = doc.CreateElement("image");
                    image.InnerText = imagefile;

                    trainingImagesElement.AppendChild(image);
                }

                doc.Save(projectFile);

                operationSuccess = true;
            }
            catch
            {
                operationSuccess = false;
            }

            return operationSuccess;
        }

        public bool CreateLanguagePack()
        {
            if (languageName == null)
                return false;

            return true;
        }

        public bool ImportSourceImage(string sourceImageFile)
        {
            string fileNameWithoutPath = sourceImageFile.Substring(sourceImageFile.LastIndexOf('\\') + 1);
            string destinationImageFile = projectFolder + '\\' + fileNameWithoutPath;
            if (File.Exists(destinationImageFile))
                return false;

            File.Copy(sourceImageFile, destinationImageFile);         
            sourceImages.Add(fileNameWithoutPath);
            return true;
        }

        public bool RemoveSourceImage(string sourceImageFile)
        {
            return false;
        }
        
    }
}
