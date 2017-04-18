using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace SerenityServiceOCR
{
    public partial class Service1 : ServiceBase
    {
        public DataOptions serviceOptions;
        public string servicesDirectory = "C:\\services";
        public string servicesTempDirectory = "C:\\services\\temp";
        public string servicesOptionFile = "service_options.xml";
        public string logFile = "log.txt";
        private FileSystemWatcher watcher;

        public Service1()
        {
            InitializeComponent();
            //LetsDoThisShit();                         // <-- THIS IS TEMPORARY DEBUG CODE
            //System.Threading.Thread.Sleep(1000000);   // <-- SO IS THIS
        }

        protected override void OnStart(string[] args)
        {
            // when the service starts, grab the "SerenityServiceOCR.xml" options file 
            LetsDoThisShit();
        }
        protected override void OnStop()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
        private void LetsDoThisShit()
        {
            // load the service options from the data file
            LoadOptions();
            if (serviceOptions.OutputWhenServiceStarts)
            {
                // when the service loads, get the pictures from the directory and capture them
                string[] bmpFilePaths = Directory.GetFiles(serviceOptions.ScreenshotInputDirectory, "*.bmp");
                foreach (string screenshotFilepath in bmpFilePaths)
                {
                    StartScreenshotTask(screenshotFilepath);
                }
            }
            if (serviceOptions.OutputWhenWatcherExecutes)
            {
                // turn on the watcher that will watch for new screenshots and capture them in real-time
                if (Directory.Exists(serviceOptions.ScreenshotInputDirectory))
                {
                    watcher = new FileSystemWatcher(serviceOptions.ScreenshotInputDirectory, "*.bmp");
                    watcher.Created += new FileSystemEventHandler(WatcherMethod);
                    watcher.Changed += new FileSystemEventHandler(WatcherMethod);
                    watcher.EnableRaisingEvents = true;
                }
                else
                {
                    SendErrorLog("Your screenshot directory does not exist!");
                    this.Stop();
                }
            }
        }
        private void WatcherMethod(object source, FileSystemEventArgs e)
        {
            StartScreenshotTask(e.FullPath);
        }
        public void StartScreenshotTask(string filepath)
        {
            Task task1 = new Task(() => ProcessScreenshot(filepath));
            task1.Start();
        }

        public void ProcessScreenshot(string filepath)
        {
            // This method is the whole enchilada.  The only input this takes is the filename to the PNG image. 
            // The PNG image is then turned into usable Elite market data.  While many files are generated in an
            // output directory during this process, the only one that matters at the end is the serialized XML file "data.txt".

            // VARIABLES
            // --------------------------------------------------------------------------------------
            // THESE ARE USED TO SEPARATE OUT THE header.png AND SLIM DOWN THE grid.png
            // I left these values as defaults, the resolutions are fixed below
            float y_percent_header = 23;
            float y_percent_grid = 67;
            float x_percent_left_margin = 3;
            float x_percent_grid = 54;

            // THESE ARE USED TO SEPARATE THE grid.png INTO THE SEVEN HOCR FILES
            int x_percent_goods_column_width = 38;
            int x_percent_sell_column_width = 9;
            int x_percent_buy_column_width = 8;
            int x_percent_cargo_column_width = 8;  // THIS ONE IS NOT CREATED INTO A HOCR FILE BUT IT MUST BE ACCOUNTED FOR WIDTH PURPOSES
            int x_percent_demand_a_column_width = 7;
            int x_percent_demand_b_column_width = 4;
            int x_percent_supply_a_column_width = 11;
            // --------------------------------------------------------------------------------------

                                                                                                                                // filepath = C:\inetpub\ftproot\miker_2015-02-18_09-41-29-PM.bmp
                                                                                                                                // servicesDirectory = "C:\\services";
                                                                                                                                // servicesTempDirectory = "C:\\services\temp";
            
            string fileAndPathNoExtension = filepath.Substring(0, filepath.IndexOf("."));                                       // = C:\inetpub\ftproot\miker_2015-02-18_09-41-29-PM
            string filenameOnly = fileAndPathNoExtension.Substring(fileAndPathNoExtension.LastIndexOf("\\") + 1);               // = miker_2015-02-18_09-41-29-PM
            string pathOnly = fileAndPathNoExtension.Substring(0, fileAndPathNoExtension.Length - filenameOnly.Length - 1);     // = C:\inetpub\ftproot

            bool stillLookingForNewDirectory = true;
            string workingPath = "";    // <-- This is the temporary directory you can put things in for this run               // = C:\services\temp\AAAA-BBBB-CCCC-DDDD

            while (stillLookingForNewDirectory)
            {
                workingPath = servicesTempDirectory + "\\" + Guid.NewGuid().ToString();
                if (!Directory.Exists(workingPath))
                {
                    stillLookingForNewDirectory = false;
                    Directory.CreateDirectory(workingPath);
                }
            }

            // STEP 0:  MOVE THE FILE TO THE NEW TEMPORARY LOCATION
            try
            {
                File.Move(filepath, workingPath + "\\" + filenameOnly + ".bmp");
            }
            catch (Exception ex)
            {
                SendErrorLog("Error moving screenshot file from '" + filepath + "' to '" + workingPath + "\\" + filenameOnly + ".bmp', Error: " + ex.ToString());
            }

            // STEP 1:  ADJUST FOR RESOLUTION OF ORIGINAL SCREENSHOT
            Bitmap bmp = new Bitmap(workingPath + "\\" + filenameOnly + ".bmp");
            try
            {
                if (bmp.Width == 1920 && bmp.Height == 1080)
                {
                    y_percent_header = 23;
                    y_percent_grid = 67;
                    x_percent_left_margin = 3;
                    x_percent_grid = 54;
                }
                else if (bmp.Width == 1920 && bmp.Height == 1200)
                {
                    y_percent_header = 26;
                    y_percent_grid = 60.5f;
                    x_percent_left_margin = 3;
                    x_percent_grid = 54;
                }
                else if (bmp.Width == 2560 && bmp.Height == 1440)
                {
                    y_percent_header = 23;
                    y_percent_grid = 67f;
                    x_percent_left_margin = 3;
                    x_percent_grid = 54;

                    x_percent_demand_a_column_width = 8;
                }
            }
            catch (Exception ex)
            {
                SendErrorLog("Error on Step 1: ADJUST FOR RESOLUTION OF ORIGINAL SCREENSHOT, Error: " + ex.ToString());
            }
            // STEP 3:  SPLIT BITMAP INTO TWO PIECES
            // The first piece is the header (only get station name from this piece)
            // The second piece is the grid below, with all margins removed (everything else from here) 

            Bitmap bmpHeader = CropBitmap(bmp, 0, 0, (int)(bmp.Width * 75 / 100), (int)(bmp.Height * y_percent_header / 100));
            Bitmap bmpGrid = CropBitmap(bmp, (int)(bmp.Width * x_percent_left_margin / 100), (int)(bmp.Height * y_percent_header / 100), (int)(bmp.Width * x_percent_grid / 100), (int)(bmp.Height * y_percent_grid / 100));

            // STEP 4:  SAVE AS PNGs
            string pngFile = @workingPath + "\\header.png";
            if (bmpHeader != null)
                bmpHeader.Save(@pngFile, ImageFormat.Png);
            pngFile = @workingPath + "\\grid.png";
            bmpGrid.Save(@pngFile, ImageFormat.Png);

            // CONVERT HEADER TO DATA
            // STEP 5A:  RUN SCAN TAILOR ON PNG HEADER
            string scanTailorPathAndFile = "\"" + serviceOptions.ScanTailorDirectory + "\\scantailor-cli.exe\"";
            try
            {
                string args = " --dpi=300 --threshold=-40 --layout=1 " + workingPath + "\\header.png " + workingPath;
                if (serviceOptions.DebugMode)
                {
                    SendErrorLog("process: " + scanTailorPathAndFile, "DEBUG");
                    SendErrorLog("args: " + args, "DEBUG");
                    SendErrorLog("Directory: \"" + serviceOptions.ScanTailorDirectory + "\" exists = " + Directory.Exists("\""+ serviceOptions.ScanTailorDirectory + "\"").ToString());
                    SendErrorLog("File: " + scanTailorPathAndFile + " exists = " + File.Exists(scanTailorPathAndFile).ToString());
                }
                var process = Process.Start(scanTailorPathAndFile, args);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                SendErrorLog("Error running STEP 5A:  RUN SCAN TAILOR ON PNG HEADER, Error: " + ex.ToString());
            }

            // STEP 5B:  CONVERT HEADER TO .PNG FOR TESSERACT
            using (Image headertif = System.Drawing.Bitmap.FromFile(workingPath + "\\header.tif"))
            {
                headertif.Save(workingPath + "\\header2.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            // STEP 5C:  RUN TESSERACT FOR HEADER
            var psi = new ProcessStartInfo("tesseract.exe");
            psi.WorkingDirectory = workingPath;
            psi.Arguments = "header2.png header -l big -psm 6 hocr";
            var process2 = Process.Start(psi);
            process2.WaitForExit();

            string STATION_NAME = "";   // <-- THIS IS THE ONLY THING WE NEED FROM THIS HEADER FILE
            string hocrFileHeader = workingPath + "\\header.html";
            string[] lines = System.IO.File.ReadAllLines(hocrFileHeader);
            try
            {
                // STEP 6:  GET HEADER STATION NAME FROM HEADER FILE
                foreach (string line in lines)
                {
                    bool exitForeach = false;
                    string thisLine = line.Replace("<strong>", "");
                    thisLine = thisLine.Replace("<em>", "");

                    // <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
                    Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");
                    // This REGEX should grab the first text line (this should be the station name) and then exit the loop
                    while (match.Success)
                    {
                        if (match.Success)
                        {
                            if (STATION_NAME == "")
                                STATION_NAME += match.Groups[5].Value.ToString();
                            else
                                STATION_NAME += " " + match.Groups[5].Value.ToString();
                        }
                        match = match.NextMatch();

                        //  We have the title now, so GTFO
                        exitForeach = true;
                    }
                    if (exitForeach)
                        break;
                }
            }
            catch (Exception ex)
            {
                SendErrorLog("Error on STEP 6:  GET HEADER STATION NAME FROM HEADER FILE, Error: " + ex.ToString());
            }

            try
            {
                // STEP 7:  RUN SCAN TAILOR ON PNG FILE
                string args3 = " " + serviceOptions.ScanTailorCommandLineOptions + " " + workingPath + "\\grid.png " + workingPath;
                //var process3 = Process.Start("\"C:\\Program Files (x86)\\Scan Tailor\\scantailor-cli.exe\"", args3);
                var process3 = Process.Start(scanTailorPathAndFile, args3);
                process3.WaitForExit();
            }
            catch (Exception ex)
            {
                SendErrorLog("Error on STEP 7:  RUN SCAN TAILOR ON PNG FILE, Error: " + ex.ToString());
            }

            // STEP 8:  CONVERT TO .PNG FOR TESSERACT
            //System.Drawing.Bitmap.FromFile(workingPath + "\\grid.tif").Save(workingPath + "\\grid2.png", System.Drawing.Imaging.ImageFormat.Png);

            using (Image gridtif = System.Drawing.Bitmap.FromFile(workingPath + "\\grid.tif"))
            {
                gridtif.Save(workingPath + "\\grid2.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            Bitmap bmpGrid2 = new Bitmap(workingPath + "\\grid2.png");

            // STEP 8.99:  CREATE A FUCKING LIST OF THE FILENAMES BECAUSE YOU ARE WRITING TOO MUCH CODE FOR REPETITIVE SHIT
            List<string> columnFilenames = new List<string>();
            columnFilenames.Add("column1_goods");
            columnFilenames.Add("column2_sell");
            columnFilenames.Add("column3_buy");
            columnFilenames.Add("column4_demand_a");
            columnFilenames.Add("column5_demand_b");
            columnFilenames.Add("column6_supply_a");
            columnFilenames.Add("column7_supply_b");

            // STEP 9:  BREAK UP THE GRID FILE INTO THE SEVEN COLUMN FILES (GOODS, SELL, BUY, DEMAND A, DEMAND B, SUPPLY A, SUPPLY B)
            int current_x_value = 0;
            Bitmap bmpColumn1_Goods = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_goods_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_goods_column_width;
            Bitmap bmpColumn2_Sell = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_sell_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_sell_column_width;
            Bitmap bmpColumn3_Buy = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_buy_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_buy_column_width + x_percent_cargo_column_width;
            Bitmap bmpColumn4_Demand_A = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_demand_a_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_demand_a_column_width;
            Bitmap bmpColumn5_Demand_B = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_demand_b_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_demand_b_column_width;
            Bitmap bmpColumn6_Supply_A = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * x_percent_supply_a_column_width / 100, bmpGrid2.Height);
            current_x_value += x_percent_supply_a_column_width;
            Bitmap bmpColumn7_Supply_B = CropBitmap(bmpGrid2, bmpGrid2.Width * current_x_value / 100, 0, bmpGrid2.Width * (100 - current_x_value) / 100, bmpGrid2.Height);

            // STEP 10:  SAVE AS PNGs
            pngFile = @workingPath + "\\column1_goods.tif";
            bmpColumn1_Goods.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column2_sell.tif";
            bmpColumn2_Sell.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column3_buy.tif";
            bmpColumn3_Buy.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column4_demand_a.tif";
            bmpColumn4_Demand_A.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column5_demand_b.tif";
            bmpColumn5_Demand_B.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column6_supply_a.tif";
            bmpColumn6_Supply_A.Save(@pngFile, ImageFormat.Tiff);
            pngFile = @workingPath + "\\column7_supply_b.tif";
            bmpColumn7_Supply_B.Save(@pngFile, ImageFormat.Tiff);

            // STEP 11:  RUN TESSERACT ON ALL SEVEN COLUMN FILES
            foreach (string thisFilename in columnFilenames)
            {
                RunTesseractProcess(thisFilename, workingPath);
            }

            // STEP 12:  EXTRACT DATA FROM ALL SEVEN HOCR FILES
            ocrData thisData = new ocrData();
            thisData.stationName = STATION_NAME;
            // add the screenshot path
            thisData.screenshotURL = "http://www.hearthforge.com/elite/screenshots/" + filenameOnly + ".bmp";

            // add the user and date from the file
            int posUnderscore = filenameOnly.IndexOf("_");
            string thisUserName = filenameOnly.Substring(0, posUnderscore);
            string thisStringDate = filenameOnly.Substring(posUnderscore + 1);  // "2015-02-18_09-41-29-PM"
            DateTime thisDatetimeForISO = DateTime.MinValue;

            try
            {
                // turn the string datetime in "yyyy-mm-dd_hh-mm-ss-tt" to a datetime
                thisDatetimeForISO = GetDateTimeFromString(thisStringDate);
            }
            catch (Exception ex)
            {
                SendErrorLog("Error converting date to ISO!  Error: " + ex.ToString());
            }

            thisData.AddDateAndUser(thisDatetimeForISO, thisUserName);

            // STEP 12a:  PULL OUT THE GOODS DATA (COLUMN 1 IN THE GRID) AND CREATE THE NUMBER OF ROWS
            string[] linesGoods = System.IO.File.ReadAllLines(workingPath + "\\" + columnFilenames[0] + ".html");
            foreach (string line in linesGoods)
            {
                string thisLine = line.Replace("<strong>", "");
                thisLine = thisLine.Replace("<em>", "");

                // <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,\-]+)
                Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,\\-]+)");

                ocrDataRow thisDataRow = new ocrDataRow();
                while (match.Success)
                {
                    if (match.Success)
                    {
                        thisDataRow.AddGoodsNameData(match.Groups[5].Value.ToString());
                        thisDataRow.AddyPositionData(int.Parse(match.Groups[2].Value));  // <-- match[2] is the Y-coordinate in the OCR
                    }
                    match = match.NextMatch();
                    if (!match.Success)
                    {
                        thisData.commodityPrices.Add(thisDataRow);
                    }
                }
            }

            // STEP 12b:  PULL OUT THE OTHER SIX COLUMNS DATA (COLUMN 2-7 IN THE GRID) AND ADD DATA TO THE EXISTING ROWS
            int iCounter = 0;
            foreach (string thisFilename in columnFilenames)
            {
                if (thisFilename != columnFilenames[0])  // Don't pull the Goods column, we already pulled it above
                {
                    string[] linesColumn = System.IO.File.ReadAllLines(workingPath + "\\" + columnFilenames[iCounter] + ".html");
                    foreach (string line in linesColumn)
                    {
                        string thisLine = line.Replace("<strong>", "");
                        thisLine = thisLine.Replace("<em>", "");
                        thisLine = thisLine.Replace("&#39;", "");

                        // <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
                        Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");

                        while (match.Success)
                        {
                            if (match.Success)
                            {
                                for (int i = 0; i < thisData.commodityPrices.Count; i++)
                                {
                                    ocrDataRow thisDataRow = thisData.commodityPrices[i];

                                    if (int.Parse(match.Groups[2].Value) >= thisDataRow.GetyPositionData() - 3 && int.Parse(match.Groups[2].Value) <= thisDataRow.GetyPositionData() + 3)
                                    {
                                        // this data entry matches the Goods Item you are on
                                        // now you just have to figure out which kind of data it is (Sell, Buy, Demand A, Demand B, Supply A, Supply B)
                                        if (iCounter == 1)
                                            thisDataRow.AddSellPriceData(match.Groups[5].Value);
                                        else if (iCounter == 2)
                                            thisDataRow.AddBuyPriceData(match.Groups[5].Value);
                                        else if (iCounter == 3)
                                            thisDataRow.AddDemandAData(match.Groups[5].Value);
                                        else if (iCounter == 4)
                                            thisDataRow.AddDemandBData(match.Groups[5].Value);
                                        else if (iCounter == 5)
                                            thisDataRow.AddSupplyAData(match.Groups[5].Value);
                                        else if (iCounter == 6)
                                            thisDataRow.AddSupplyBData(match.Groups[5].Value);
                                    }
                                }
                            }
                            match = match.NextMatch();
                        }
                    }
                }
                iCounter++;  // incrememt to the next column of data
            }

            // STEP 13:  REMOVE ANY GOODS COLUMN HEADERS
            // These would be the Headers such as:  CHEMICALS, CONSUMER ITEMS, FOODS, etc.
            // The easiest way to find these is to look for rows that have no other data in them

            List<int> PrettySureImDoingThisWrongButWhatever = new List<int>();
            for (int i = 0; i < thisData.commodityPrices.Count; i++)
            {
                ocrDataRow thisItem = thisData.commodityPrices[i];
                if (thisItem.isThisRowEmpty())
                    PrettySureImDoingThisWrongButWhatever.Add(i);
            }
            for (int i = PrettySureImDoingThisWrongButWhatever.Count - 1; i >= 0; i--)
            {
                // have to do this backwards so it will fucking work
                thisData.commodityPrices.RemoveAt(PrettySureImDoingThisWrongButWhatever[i]);
            }

            // STEP 14:  CONVERT HOCR DATA INTO SERIALZIED DATA FILE
            if (serviceOptions.OutputToFile)
            {
                if (!Directory.Exists(serviceOptions.OutputFileDirectory))
                    Directory.CreateDirectory(serviceOptions.OutputFileDirectory);
                string json = JsonConvert.SerializeObject(thisData);
                System.IO.File.WriteAllText(serviceOptions.OutputFileDirectory + "\\" + filenameOnly + ".txt", json);

                //    XmlSerializer serializer = new XmlSerializer(thisData.GetType());
                //    StreamWriter writer = new StreamWriter(serviceOptions.OutputFileDirectory + "\\" + filenameOnly + ".txt");
                //    serializer.Serialize(writer.BaseStream, thisData);
            }

            if (serviceOptions.OutputToCall)
            {
                // TO-DO: INSERT CALL TO DAN'S PROGRAM HERE
                // serviceOptions.OutputCallURL gets used here


            }

            
            // STEP 15: Move the screenshot to the OCR server website so that it can be linked to from the main website
            bmp.Dispose();
            try
            {
                File.Move(workingPath + "\\" + filenameOnly + ".bmp", serviceOptions.OutputScreenshotDirectory + "\\" + filenameOnly + ".bmp");
            }
            catch (Exception ex)
            {
                SendErrorLog("Error moving screenshot file from '" + workingPath + "\\" + filenameOnly + ".bmp'" + "' to '" + serviceOptions.OutputScreenshotDirectory + "\\" + filenameOnly + ".bmp', Error: " + ex.ToString());
            }


            // object cleanup
            bmpHeader.Dispose();
            bmpGrid.Dispose();
            bmpGrid2.Dispose();
            bmpColumn1_Goods.Dispose();
            bmpColumn2_Sell.Dispose();
            bmpColumn3_Buy.Dispose();
            bmpColumn4_Demand_A.Dispose();
            bmpColumn5_Demand_B.Dispose();
            bmpColumn6_Supply_A.Dispose();
            bmpColumn7_Supply_B.Dispose();

            // STEP 16:  DELETE THE TEMPORARY GUID DIRECTORY
            if (!serviceOptions.DebugMode)
            {
                if (Directory.Exists(workingPath))
                {
                    try
                    {
                        Directory.Delete(workingPath, true);
                    }
                    catch (Exception ex)
                    {
                        SendErrorLog("Error Deleting Temporary Directory \"" + workingPath + "\", Error: " + ex.ToString());
                    }
                }
            }
        }

        public DateTime GetDateTimeFromString(string inputString)
        {
            // turn the string datetime in "yyyy-mm-dd_HH-mm-ss" to a datetime
            Match match = Regex.Match(inputString, "([0-9]+)-([0-9]+)-([0-9]+)_([0-9]+)-([0-9]+)-([0-9]+)");

            if (match.Success)
            {
                DateTime outputDateTime = new DateTime(int.Parse(match.Groups[1].ToString()), int.Parse(match.Groups[2].ToString()), int.Parse(match.Groups[3].ToString()), int.Parse(match.Groups[4].ToString()), int.Parse(match.Groups[5].ToString()), int.Parse(match.Groups[6].ToString()));
                return outputDateTime;
            }
            else
                return DateTime.MinValue;
        }

        public Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }
        public void RunTesseractProcess(string fileName, string workingPath)
        {
            var psi101 = new ProcessStartInfo("tesseract.exe");
            psi101.WorkingDirectory = workingPath;
            psi101.Arguments = fileName + ".tif " + fileName + " " + serviceOptions.TesseractCommandLineOptions;
            var process101 = Process.Start(psi101);
            process101.WaitForExit();
        }
        public class ocrData
        {
            // This is the class that will contain everything we need about the screenshot and will get serialized at the end
            public string stationName;
            public DateTime date;
            public string screenshotURL;
            public string userName;
            public List<ocrDataRow> commodityPrices = new List<ocrDataRow>() { };

            public ocrData()
            {
                stationName = "";
                date = DateTime.MinValue;
                screenshotURL = "";
            }

            public void AddDateAndUser(DateTime dtDate, string strUser)
            {
                date = dtDate;
                userName = strUser;
            }
        }
        public class ocrDataRow
        {
            // This class represents one row of data in the screenshot
            // NOTE:  The number fields here are treated like strings (and even concatenated) because sometimes the integers are split up
            //        into different "words" and need to be constructed like strings...then parse as integers at the very end
            // 

            public string commodityName;     // i.e. EXPLOSIVES
            public string sellPrice;       // i.e. 300
            public string buyPrice;        // i.e. 240
            public string demandNumber;         // i.e. 1000
            public string demandString;      // i.e. LOW
            public string supplyNumber;         // i.e. 1000
            public string supplyString;      // i.e. LOW
            private int yPosition;  // THIS IS THE Y-POSITION THAT THIS GOOD APPEARS AT IN THE HOCR OUTPUT 

            public ocrDataRow()
            {
                commodityName = "";
                sellPrice = "";
                buyPrice = "";
                demandNumber = "";
                demandString = "";
                supplyNumber = "";
                supplyString = "";
            }


            public void AddyPositionData(int pos)
            {
                this.yPosition = pos;
            }
            public int GetyPositionData()
            {
                return this.yPosition;
            }
            public void AddGoodsNameData(string thisData)
            {
                if (commodityName == "")
                    commodityName += thisData;
                else
                    commodityName += " " + thisData;
            }
            public void AddSellPriceData(string thisData)
            {
                sellPrice += ReplaceBadCharacers(thisData);
            }
            public void AddBuyPriceData(string thisData)
            {
                buyPrice += ReplaceBadCharacers(thisData);
            }
            public void AddDemandAData(string thisData)
            {
                demandNumber += ReplaceBadCharacers(thisData);
            }

            public void AddDemandBData(string thisData)
            {
                thisData = thisData.Replace("MEO", "MED");
                thisData = thisData.Replace("ME0", "MED");
                thisData = thisData.Replace("LDW", "LOW");
                if (thisData == "LOW" || thisData == "MED" || thisData == "HIGH")
                    demandString = thisData;
            }

            public void AddSupplyAData(string thisData)
            {
                supplyNumber += ReplaceBadCharacers(thisData);
            }

            public void AddSupplyBData(string thisData)
            {
                thisData = thisData.Replace("MEO", "MED");
                thisData = thisData.Replace("ME0", "MED");
                thisData = thisData.Replace("LDW", "LOW");
                if (thisData == "LOW" || thisData == "MED" || thisData == "HIGH")
                    supplyString = thisData;
            }

            public bool isThisRowEmpty()
            {
                // this is needed before the final serialization to pull out any Good column headers (CHEMICALS, FOODS, etc)
                if (sellPrice == "" && buyPrice == "" && demandNumber == "" && demandString == "" && supplyNumber == "" && supplyString == "")
                    return true;
                else
                    return false;
            }

            private string ReplaceBadCharacers(string incomingString)
            {
                string output = incomingString;
                output = output.Replace(",", "");
                output = output.Replace(".", "");
                output = output.Replace("-", "");
                output = output.Replace("B", "8");
                output = output.Replace("G", "8");
                output = output.Replace("o", "0");
                output = output.Replace("O", "0");
                output = output.Replace("'", "");
                output = output.Replace("I", "1");
                output = output.Replace("i", "1");
                return output;
            }
        }

        public void LoadOptions()
        {
            if (File.Exists(servicesDirectory + "\\" + servicesOptionFile))
            {
                serviceOptions = new DataOptions();
                XmlSerializer deserializer = new XmlSerializer(serviceOptions.GetType());
                TextReader textReader = new StreamReader(servicesDirectory + "\\" + servicesOptionFile);
                serviceOptions = (DataOptions)deserializer.Deserialize(textReader);
                textReader.Close();
            }
            else
            {
                // send message to error log and close
                SendErrorLog("Options file did not exist in '" + servicesDirectory + "\\" + servicesOptionFile + "'...stopping service!");
                this.Stop();
            }
        }
        public class DataOptions
        {
            public string ScanTailorCommandLineOptions;
            public string ScanTailorDirectory;
            public string TesseractCommandLineOptions;
            public string TesseractDirectory;

            public string ScreenshotInputDirectory;

            public bool OutputToFile;
            public string OutputFileDirectory;
            public string OutputScreenshotDirectory;
            public bool OutputToCall;
            public string OutputCallURL;

            public bool OutputWhenServiceStarts;
            public bool OutputWhenWatcherExecutes;

            public bool DebugMode = false;

        }
        public void SendErrorLog(string message)
        {
            SendErrorLog(message, "");
        }
                public void SendErrorLog(string message, string debug)
        {
            // ...if any fail: send an error to the log, turn off the watcher, and change the main icon
            using (StreamWriter sw = new StreamWriter(servicesDirectory + "\\" + logFile, true))
            {
                if (debug == "DEBUG")
                    sw.WriteLine(DateTime.Now.ToString() + " *DEBUG*: " + message);
                else
                    sw.WriteLine(DateTime.Now.ToString() + " " + message);
            }
        }

        #region Debug Methods

        //public class DataOptions
        //{
        //    public string ScanTailorCommandLineOptions = "-amazing";
        //    public string ScanTailorDirectory = "C:\\dontknow";
        //    public string TesseractCommandLineOptions = "-amazing";
        //    public string TesseractDirectory = "C:\\dontknow";

        //    public string ScreenshotInputDirectory = "C:\\dontknow";

        //    public bool OutputToFile = true;
        //    public string OutputFileDirectory = "C:\\services\\temp";
        //    public bool OutputToCall = false;
        //    public string OutputCallURL = "http://danserver.com";

        //    public bool OutputWhenServiceStarts = true;
        //    public bool OutputWhenWatcherExecutes = true;
        //}


        //public void SaveOptions()
        //{
        //    // this is only being used once to generate the file that the service will use
        //    DataOptions tempOptions = new DataOptions();
        //    XmlSerializer serializer = new XmlSerializer(tempOptions.GetType());
        //    StreamWriter writer = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + "\\service_options.xml");
        //    serializer.Serialize(writer.BaseStream, tempOptions);
        //    writer.Close();
        //}
        #endregion
    }
}
