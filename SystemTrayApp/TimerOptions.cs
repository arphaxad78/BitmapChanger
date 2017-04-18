using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;


namespace SerenityClientOCR
{
    [XmlRootAttribute("TimerOptions")]
    public class TimerOptions
    {
        public string strScreenshotDirectory;
        public bool bThreeMonitors;
        public int intLeftBezel;
        public int intRightBezel;
        public string strUsername;
        public string strPathURL;

        private ContextMenus.OCR_State stateOCR;
        private FileSystemWatcher watcher;
        private bool watcherIsRunning = false;

        //private Timer timer;
        //private int timerTicks = 5000;                      // <-- IN MILLISECONDS
        //private int timerSecondsTotalBeforeFTP = 10;        // <-- IN SECONDS
        //private DateTime timerDate;
        //private bool TimerIsRunning = true;
        //private List<string> screenshotFiles;

        public ContextMenus.OCR_State GetStateOCR()
        {
            return stateOCR;
        }

        public void AddStateOCR(ContextMenus.OCR_State _stateOCR)
        {
            stateOCR = _stateOCR;
        }

        public TimerOptions()
        {
        }

        public void LoadOptions()
        {
            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\options.xml"))
            {
                XmlSerializer deserializer = new XmlSerializer(this.GetType());
                TextReader textReader = new StreamReader(System.IO.Directory.GetCurrentDirectory() + "\\options.xml");
                TimerOptions tempOptions = (TimerOptions)deserializer.Deserialize(textReader);
                textReader.Close();

                this.strScreenshotDirectory = tempOptions.strScreenshotDirectory;
                this.bThreeMonitors = tempOptions.bThreeMonitors;
                this.intLeftBezel = tempOptions.intLeftBezel;
                this.intRightBezel = tempOptions.intRightBezel;
                this.strUsername = tempOptions.strUsername;
                this.strPathURL = tempOptions.strPathURL;
            }
            else
            {
                // just fill the object with defaults
                strScreenshotDirectory = "C:\\Users\\YOUR_COMPUTER_NAME\\Pictures\\Frontier Developments\\Elite Dangerous";
                bThreeMonitors = false;
                intLeftBezel = 0;
                intRightBezel = 0;
                strUsername = "Anonymous";
                strPathURL = "http://www.hearthforge.com/elite/client/update.xml";
            }

        }
        public void SaveOptions()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            StreamWriter writer = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\options.xml");
            serializer.Serialize(writer.BaseStream, this);
            writer.Close();
        }
        public UpdaterData LoadUpdaterData()
        {
            UpdaterData thisData = new UpdaterData();
            XmlSerializer deserializer = new XmlSerializer(thisData.GetType());
            TextReader textReader = new StreamReader(System.IO.Directory.GetCurrentDirectory() + "\\update.xml");
            thisData = (UpdaterData)deserializer.Deserialize(textReader);
            textReader.Close();

            return thisData;
        }
        public void SaveUpdaterData()
        {
            UpdaterData thisData = new UpdaterData();
            thisData.version = "1.0.3.0";
            thisData.PathURL = "http://www.hearthstone.com/whatever";

            // this method is only used to create the file once that I will put on the server...this shouldn't get called on the client
            XmlSerializer serializer = new XmlSerializer(thisData.GetType());
            StreamWriter writer = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\update.xml");
            serializer.Serialize(writer.BaseStream, thisData);
            writer.Close();
        }
        public void ActivateWatcher()
        {
            if (!watcherIsRunning)
            {
                if (Directory.Exists(strScreenshotDirectory))
                {
                    watcher = new FileSystemWatcher(strScreenshotDirectory, "*.bmp");
                    watcher.Created += new FileSystemEventHandler(OnCreated);
                    if (AreThereAnyScreenshots())
                    {
                        List<string> newList = GetListOfFiles();
                        SendOffTheScreenshots(newList);
                    }
                    watcher.EnableRaisingEvents = true;
                    watcherIsRunning = true;
                    stateOCR.SetState("on");
                }
                else
                {
                    SendErrorLog("Your screenshot directory does not exist!");
                    stateOCR.SetState("off");
                    watcherIsRunning = false;
                }
            }

        }
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            //Show that a file has been created, changed, or deleted.
            //WatcherChangeTypes wt = e.ChangeType;
            //Console.WriteLine("File {0} {1}", e.FullPath, wct.ToString());
            SendOffTheScreenshotTask(e.Name);
        }
        public void DeactivateWatcher()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            stateOCR.SetState("off");
            watcherIsRunning = false;
        }

        private void SendOffTheScreenshots(List<string> screenshotFileList)
        {
            foreach (string thisFile in screenshotFileList)
            {
                SendOffTheScreenshotTask(thisFile);
            }
        }
        private void SendOffTheScreenshotTask(string screenshotFile)
        {
            Task task1 = new Task(() => SendOffTheScreenshot(screenshotFile));
            task1.Start();
        }
        private void SendOffTheScreenshot(string screenshotFile)
        {
            if (DoesThisScreenshotFitTheSize(screenshotFile))
            {
                string screenshotDateTimeFilename = GetDateTimeAndUserFilename(strScreenshotDirectory + "\\" + screenshotFile);
                string ArchiveDirectory = strScreenshotDirectory + "\\Archive";

                if (!Directory.Exists(ArchiveDirectory))
                    Directory.CreateDirectory(ArchiveDirectory);
                try
                {
                    if (File.Exists(ArchiveDirectory + "\\" + screenshotDateTimeFilename))
                    {
                        // If the same file was taken at the same second, then just abort this process
                        return;
                    }
                    File.Move(strScreenshotDirectory + "\\" + screenshotFile, ArchiveDirectory + "\\" + screenshotDateTimeFilename);
                }
                catch (Exception ex)
                {
                    SendErrorLog("Error moving file to \\Archive, error: " + ex.ToString());
                    return;
                }

                // next prepare the Temp directory for .zip
                string TempDirectory = ArchiveDirectory + "\\Temp";
                if (!Directory.Exists(TempDirectory))
                    Directory.CreateDirectory(TempDirectory);

                Bitmap bmp = new Bitmap(ArchiveDirectory + "\\" + screenshotDateTimeFilename);

                // STEP 1:  GRAYSCALE
                bmp = MakeGrayscale3(bmp);

                // STEP 2:  INVERT
                bmp = Transform(bmp);

                if (bThreeMonitors)
                {
                    // adjust the bmp for the width 
                    int oneMonitorWidth = (bmp.Width - intLeftBezel - intRightBezel) / 3;
                    bmp = CropBitmap(bmp, oneMonitorWidth + intLeftBezel, 0, oneMonitorWidth, bmp.Height);
                }
                // save the bitmap to the temp directory
                if (File.Exists(TempDirectory + "\\" + screenshotDateTimeFilename))
                {
                    try
                    {
                        File.Delete(TempDirectory + "\\" + screenshotDateTimeFilename);
                    }
                    catch (Exception ex)
                    {
                        SendErrorLog("Error deleting file from Temp Directory, Error: " + ex.ToString());
                    }
                }
                bmp.Save(TempDirectory + "\\" + screenshotDateTimeFilename);

                //// zip up all the files in the temp directory
                //string ZipDirectory = TempDirectory + "\\Zip";
                //if (!Directory.Exists(ZipDirectory))
                //    Directory.CreateDirectory(ZipDirectory);
                //Guid guid = Guid.NewGuid();
                //ZipFile.CreateFromDirectory(TempDirectory, ZipDirectory + "\\" + guid.ToString() + ".zip", CompressionLevel.Optimal, false);
                bmp.Dispose();

                FTPFile(screenshotDateTimeFilename);
                File.Delete(TempDirectory + "\\" + screenshotDateTimeFilename);
            }
            else
            {
                SendErrorLog("Your screenshots do not meet the resolution requirements!  Either adjust the settings through \"Options\" or contact Mike!");
            }
        }
        private string GetDateTimeAndUserFilename(string filePath)
        {
            if (File.Exists(filePath))
            {
                string extension = filePath.Substring(filePath.LastIndexOf(".") + 1);
                DateTime createdDateTime = File.GetLastWriteTime(filePath);
                //return string.Format("{0}_{1:yyyy-MM-dd_hh-mm-ss-tt}.{2}", strUsername, createdDateTime, extension);
                TimeZone zone = TimeZone.CurrentTimeZone;
                //DateTime universal = zone.ToUniversalTime(DateTime.Now);
                return string.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}.{2}", strUsername, zone.ToUniversalTime(createdDateTime), extension);
            }
            else
                return "";
        }

        public Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }
        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
                  {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                  });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
        public Bitmap Transform(Bitmap source)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(source.Width, source.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            // create the negative color matrix
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {-1, 0, 0, 0, 0},
                new float[] {0, -1, 0, 0, 0},
                new float[] {0, 0, -1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {1, 1, 1, 0, 1}
            });

            // create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            return newBitmap;
        }

        public void SendErrorLog(string message)
        {
            // ...if any fail: send an error to the log, turn off the watcher, and change the main icon
            using (StreamWriter sw = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\log.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + " " + message);
            }
            DeactivateWatcher();
            stateOCR.SetState("error");
        }
        private bool DoesThisScreenshotFitTheSize(string screenshotFile)
        {
            List<Resolution> res_list = new List<Resolution>();
            res_list.Add(new Resolution(1920, 1080));
            res_list.Add(new Resolution(1920, 1200));
            res_list.Add(new Resolution(2560, 1440));

            if (!File.Exists(strScreenshotDirectory + "\\" + screenshotFile))
            {
                SendErrorLog("Error grabbing image in DoesThisScreenshotFitTheSize()");
                return false;
            }
            Image img = Image.FromFile(strScreenshotDirectory + "\\" + screenshotFile);
            Resolution r1 = new Resolution(img.Width, img.Height);
            bool DidOneMatch = false;
            foreach (Resolution thisRes in res_list)
            {
                if (thisRes.Compare(r1.ReturnCenterScreenWidth(bThreeMonitors, intLeftBezel, intRightBezel), r1.height))
                    DidOneMatch = true;
            }
            if (!DidOneMatch)
                return false;
            img.Dispose();
            
            return true;
        }

        public bool IsWatcherRunning()
        {
            return watcherIsRunning;
        }

        private List<string> GetListOfFiles()
        {
            string[] theseFiles = Directory.GetFiles(strScreenshotDirectory);
            List<string> outputList = new List<string>();
            foreach (string thisFile in theseFiles)
            {
                if (thisFile.Substring(thisFile.IndexOf(".") + 1) == "bmp")
                {
                    outputList.Add(thisFile.Substring(thisFile.LastIndexOf("\\") + 1));
                }
            }
            return outputList;
        }
        private bool AreThereAnyScreenshots()
        {
            string[] theseFiles = Directory.GetFiles(strScreenshotDirectory);
            foreach (string thisFile in theseFiles)
            {
                if (thisFile.Substring(thisFile.IndexOf(".") + 1) == "bmp")
                    return true;
            }
            return false;
        }
        class Resolution
        {
            public int height;
            private int width;

            public Resolution(int _width, int _height)
            {
                height = _height;
                width = _width;
            }

            public bool Compare(int _width, int _height)
            {
                if (height == _height && width == _width)
                    return true;
                return false;
            }
            public int ReturnCenterScreenWidth(bool _bThreeMonitors, int _intLeftBezel, int _intRightBezel)
            {
                if (!_bThreeMonitors)
                    return width;
                else
                    return (width - _intLeftBezel - _intRightBezel) / 3;
            }
        }

        public void FTPFile(string fileName)
        {
            string filePath = strScreenshotDirectory + "\\Archive\\Temp\\" + fileName;
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Credentials = new NetworkCredential("ftpuser", "FTPomg78$$$");
                    client.UploadFile("ftp://166.62.53.75/" + fileName, "STOR", filePath);
                }
                catch (Exception ex)
                {
                    SendErrorLog("Error sending FTP file, error: " + ex.ToString());
                }
            }
        }

        [XmlRootAttribute("UpdaterData")]
        public class UpdaterData
        {
            public string version;
            public string PathURL;

        }

        #region Old Code

        //public void DoSomething(object obj)
        //{
        //    //debug code
        //    //----
        //    //Random r = new Random();
        //    //File.Create(System.IO.Directory.GetCurrentDirectory() + "\\" + r.Next(1,1000000) + ".txt");
        //    //----

        //    if (AreThereAnyScreenshots())
        //    {
        //        List<string> newList = GetListOfFiles();

        //        if (AreTheseListsTheSame(screenshotFiles, newList))
        //        {
        //            if (HasEnoughTimePassed())
        //            {
        //                //SendOffTheScreenshots("poop");
        //            }

        //        }
        //        else
        //        {
        //            // restart the timer date
        //            timerDate = DateTime.Now;
        //            screenshotFiles = newList;
        //        }
        //    }
        //}
        //private bool DoAllScreenshotsFitTheSize()
        //{
        //    List<Resolution> res_list = new List<Resolution>();
        //    res_list.Add(new Resolution(1920, 1080));
        //    res_list.Add(new Resolution(1920, 1200));

        //    foreach (string fileName in screenshotFiles)
        //    {
        //        if (!File.Exists(strScreenshotDirectory + "\\" + fileName))
        //        {
        //            SendErrorLog("Error grabbing image in DoAllScreenshotsFitTheSize()");
        //            return false;
        //        }
        //        Image img = Image.FromFile(strScreenshotDirectory + "\\" + fileName);
        //        Resolution r1 = new Resolution(img.Width, img.Height);
        //        bool DidOneMatch = false;
        //        foreach (Resolution thisRes in res_list)
        //        {
        //            if (thisRes.Compare(r1.ReturnCenterScreenWidth(bThreeMonitors, intLeftBezel, intRightBezel), r1.height))
        //                DidOneMatch = true;
        //        }
        //        if (!DidOneMatch)
        //            return false;
        //        img.Dispose();
        //    }
        //    return true;
        //}
        //public void StopTimer()
        //{
        //    if (TimerIsRunning)
        //        timer.Change(Timeout.Infinite, Timeout.Infinite);
        //    TimerIsRunning = false;
        //    stateOCR.SetState("off");
        //}

        //public void StartTimer()
        //{
        //    if (!TimerIsRunning)
        //        timer.Change(0, timerTicks);
        //    TimerIsRunning = true;
        //    timerDate = DateTime.MinValue;
        //    if (screenshotFiles != null)
        //        screenshotFiles.Clear();
        //    stateOCR.SetState("on");
        //}
        //public TimerOptions(bool fullOptions)
        //{
        //    if (fullOptions)  // <--- if this object is meant as a real timer and not just for serialization purposes
        //        timer = new System.Threading.Timer(new TimerCallback(DoSomething), null, timerTicks, Timeout.Infinite);

        //}
        //public bool IsTimerRunning()
        //{
        //    return TimerIsRunning;
        //}
        //private bool HasEnoughTimePassed()
        //{
        //    int secondsDiff = ((TimeSpan)(DateTime.Now - timerDate)).Seconds;
        //    if (secondsDiff < timerSecondsTotalBeforeFTP)
        //        return false;
        //    return true;
        //}

        //private bool AreTheseListsTheSame(List<string> list1, List<string> list2)
        //{
        //    if (list1 != null && list2 != null)
        //    {
        //        if (list1.Count != list2.Count)
        //            return false;
        //        for (int i = 0; i < list1.Count; i++)
        //        {
        //            if (list1[i] != list2[i])
        //                return false;
        //        }
        //        return true;
        //    }
        //    else
        //        return false;
        //}
        ////  http://stackoverflow.com/questions/15268760/upload-file-to-ftp-using-c-sharp
        //public void UploadFtpFile(string folderName, string fileName)
        //{

        //    FtpWebRequest request;
        //    try
        //    {
        //        string absoluteFileName = Path.GetFileName(fileName);

        //        request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", "127.0.0.1", folderName, absoluteFileName))) as FtpWebRequest;
        //        request.Method = WebRequestMethods.Ftp.UploadFile;
        //        request.UseBinary = 1;
        //        request.UsePassive = 1;
        //        request.KeepAlive = 1;
        //        request.Credentials = new NetworkCredential("ftpuser", "FTPomg78$$$");
        //        request.ConnectionGroupName = "group";

        //        using (FileStream fs = File.OpenRead(fileName))
        //        {
        //            byte[] buffer = new byte[fs.Length];
        //            fs.Read(buffer, 0, buffer.Length);
        //            fs.Close();
        //            Stream requestStream = request.GetRequestStream();
        //            requestStream.Write(buffer, 0, buffer.Length);
        //            requestStream.Close();
        //            requestStream.Flush();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        #endregion

    }
}
