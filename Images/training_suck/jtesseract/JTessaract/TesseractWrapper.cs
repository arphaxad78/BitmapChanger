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
using System.IO;
using System.Collections;

namespace JTessaract
{
    class TesseractWrapper
    {
        string tesseractBinaryFolder = "C:\\ruwan\\";

        public event EventCompletedDelegate EventCompleted = null;

        public string TesseractBinaryFolder
        {
            get { return tesseractBinaryFolder; }
            set { tesseractBinaryFolder = value; }
        }

        public TesseractWrapper()
        {

        }

        public bool CreateBoxFiles(string projectFolder, ArrayList sourceImages, bool overwriteExisting)
        {
            int count = sourceImages.Count;

            for (int i = 0; i < count; i++)
            {
                string fileName = (string)sourceImages[i];
                string fileNameSegment = fileName.Substring(0, fileName.LastIndexOf('.'));

                if (!File.Exists(projectFolder + fileNameSegment + ".box"))
                {
                    CreateBoxFile(projectFolder, fileName);
                }
                else
                {
                    if (overwriteExisting)
                    {
                        CreateBoxFile(projectFolder, fileName);
                    }
                }
            }

            return true;
        }               

        public bool CreateBoxFile(string projectFolder, string imageFile)
        {
            return CreateBoxFile(projectFolder, imageFile, null);
        }

        public bool CreateBoxFile(string projectFolder, string imageFile, string newLanguage)
        {
            string fileNameSegment = imageFile.Substring(0, imageFile.LastIndexOf('.'));
            int exitCode;
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            try
            {
                string output;
                if (newLanguage == null)
                {
                    output = ShellExecute("tesseract.exe", tesseractBinaryFolder, projectFolder + imageFile + " " + projectFolder + fileNameSegment + " batch.nochop makebox", projectFolder, out exitCode);
                }
                else
                {
                    // Bootstrapping
                    output = ShellExecute("tesseract.exe", tesseractBinaryFolder, projectFolder + imageFile + " " + projectFolder + fileNameSegment + " -l " + newLanguage + " batch.nochop makebox", projectFolder, out exitCode);
                }

                if (File.Exists(projectFolder + fileNameSegment + ".txt"))
                {
                    if (File.Exists(projectFolder + fileNameSegment + ".box"))
                        File.Delete(projectFolder + fileNameSegment + ".box");

                    File.Move(projectFolder + fileNameSegment + ".txt", projectFolder + fileNameSegment + ".box");
                }
            }
            catch
            {
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null);

            return true;
        }

        public bool CreateTrainingFiles(string projectFolder, ArrayList sourceImages, bool overwriteExisting)
        {
            // TODO: handle overwrite case
            int count = sourceImages.Count;
            bool returnFlag = true;

            for (int i = 0; i < count; i++)
            {
                string fileName = (string)sourceImages[i];
                returnFlag = returnFlag & CreateTrainingFile(projectFolder, fileName);
            }

            return returnFlag;
        }

        public bool CreateTrainingFile(string projectFolder, string imageFile)
        {
            string fileNameSegment = imageFile.Substring(0, imageFile.LastIndexOf('.'));
            int exitCode;
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            try
            {
                string output;
                output = ShellExecute("tesseract.exe", tesseractBinaryFolder, projectFolder + imageFile + " junk nobatch box.train", projectFolder, out exitCode);
            }
            catch
            {
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null);

            return true;
        }

        public bool PerformClustering(string projectFolder, ArrayList imageFiles)
        {          
            int exitCode;
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            string parameters = "";
            if ((imageFiles != null) && (imageFiles.Count > 0))
            {
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    string imageFile = (string)imageFiles[i];
                    string fileNameSegment = imageFile.Substring(0, imageFile.LastIndexOf('.'));
                    parameters += projectFolder + fileNameSegment + ".tr ";
                }

                try
                {
                    string output;
                    output = ShellExecute("mftraining.exe", tesseractBinaryFolder, parameters, projectFolder, out exitCode);
                    output = ShellExecute("cntraining.exe", tesseractBinaryFolder, parameters, projectFolder, out exitCode);
                }
                catch
                {
                }
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null); 

            return true;
        }

        public bool GenerateUniCharSet(string projectFolder, ArrayList imageFiles)
        {
            int exitCode;
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            string parameters = "";
            if ((imageFiles != null) && (imageFiles.Count > 0))
            {
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    string imageFile = (string)imageFiles[i];
                    string fileNameSegment = imageFile.Substring(0, imageFile.LastIndexOf('.'));
                    parameters += projectFolder + fileNameSegment + ".box ";
                }

                try
                {
                    string output;
                    output = ShellExecute("unicharset_extractor.exe", tesseractBinaryFolder, parameters, projectFolder, out exitCode);
                }
                catch
                {
                }
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null); 

            return true;
        }

        public bool CreateDictionaries(string projectFolder)
        {
            int exitCode;
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            try
            {
                string output;
                output = ShellExecute("wordlist2dawg.exe", tesseractBinaryFolder, " frequent_words.txt freq-dawg", projectFolder, out exitCode);
                output = ShellExecute("wordlist2dawg.exe", tesseractBinaryFolder, " words.txt word-dawg", projectFolder, out exitCode);
                if (File.Exists(projectFolder + "user_words.txt"))
                {
                    File.Copy(projectFolder + "user_words.txt", projectFolder + "user-words");
                }
            }
            catch
            {
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null); 

            return true;
        }

        public bool CreateDangAmbigs(string projectFolder)
        {
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            if (File.Exists(projectFolder + "ambiguities.txt"))
            {
                File.Copy(projectFolder + "ambiguities.txt", projectFolder + "DangAmbigs");
            }

            if (EventCompleted != null)
                EventCompleted(GetTesseractLog(projectFolder), null); 

            return true;
        }

        public bool CreateLanguagePack(string projectFolder, string languageName)
        {
            if ((projectFolder.LastIndexOf('\\') + 1) != projectFolder.Length)
            {
                projectFolder += '\\';
            }

            if (Directory.Exists(projectFolder + "langpack"))
            {
                Directory.Delete(projectFolder + "langpack");
            }

            Directory.CreateDirectory(projectFolder + "langpack");

            string file = "inttemp"; 

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "pffmtable";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "Microfeat";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "normproto";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "unicharset";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "freq-dawg";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "word-dawg";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "user-words";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            file = "DangAmbigs";

            if (File.Exists(projectFolder + file))
            {
                File.Copy(projectFolder + file, projectFolder + "\\langpack\\" + languageName + "." + file);
            }

            if (EventCompleted != null)
                EventCompleted("Language Pack Created", null);

            return true;
        }
        
        private string ShellExecute(string command, string path, string parameters, string projectFolder, out int exitCode)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(path + command, parameters);
            //psi.RedirectStandardOutput = true;
            psi.UseShellExecute = true;
            psi.WorkingDirectory = projectFolder;
            psi.CreateNoWindow = true;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);

            //string toolOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            exitCode = process.ExitCode;

            return null; // toolOutput;
        }

        internal string GetTesseractLog(string projectFolder)
        {
            string logFileName = projectFolder + "tesseract.log";
            if (File.Exists(logFileName))
            {
                return File.ReadAllText(logFileName);
            }

            return "";
        }

        public delegate void EventCompletedDelegate(string tesseractLog, object result);
    }
}
