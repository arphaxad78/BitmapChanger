using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace BitmapChanger
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdOpenFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            txtInputPathAndFile.Text = openFileDialog1.FileName;
        }

        public class ocrData
        {
            // This is the class that will contain everything we need about the screenshot and will get serialized at the end
            public string stationName;
            public List<ocrDataRow> ocrDataRows = new List<ocrDataRow>(){};

            public ocrData()
            {
                stationName = "";
            }
        }

        public class ocrDataRow
        {
            // This class represents one row of data in the screenshot
            // NOTE:  The number fields here are treated like strings (and even concatenated) because sometimes the integers are split up
            //        into different "words" and need to be constructed like strings...then parse as integers at the very end
            // 

            public DateTime goodDate;
            public string goodName;     // i.e. EXPLOSIVES
            public string sellPrice;       // i.e. 300
            public string buyPrice;        // i.e. 240
            public string demandA;         // i.e. 1000
            public string demandB;      // i.e. LOW
            public string supplyA;         // i.e. 1000
            public string supplyB;      // i.e. LOW
            private int yPosition;  // THIS IS THE Y-POSITION THAT THIS GOOD APPEARS AT IN THE HOCR OUTPUT 

            public ocrDataRow()
            {
                goodDate = DateTime.Now;  
                goodName = "";
                sellPrice = "";
                buyPrice = "";
                demandA = "";
                demandB = "";
                supplyA = "";
                supplyB = "";
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
                if (goodName == "")
                    goodName += thisData;
                else
                    goodName += " " + thisData;
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
                demandA += ReplaceBadCharacers(thisData);
            }

            public void AddDemandBData(string thisData)
            {
                thisData = thisData.Replace("MEO", "MED");
                thisData = thisData.Replace("ME0", "MED");
                thisData = thisData.Replace("LDW", "LOW");
                if (thisData == "LOW" || thisData == "MED" || thisData == "HIGH")
                    demandB = thisData;
            }

            public void AddSupplyAData(string thisData)
            {
                supplyA += ReplaceBadCharacers(thisData);
            }

            public void AddSupplyBData(string thisData)
            {
                thisData = thisData.Replace("MEO", "MED");
                thisData = thisData.Replace("ME0", "MED");
                thisData = thisData.Replace("LDW", "LOW");
                if (thisData == "LOW" || thisData == "MED" || thisData == "HIGH")
                    supplyB = thisData;
            }

            public bool isThisRowEmpty()        
            {
                // this is needed before the final serialization to pull out any Good column headers (CHEMICALS, FOODS, etc)
                if (sellPrice == "" && buyPrice == "" && demandA == "" && demandB == "" && supplyA == "" && supplyB == "")
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

        public void RunTesseractProcess(string fileName, string workingPath)
        {
            var psi101 = new ProcessStartInfo("tesseract.exe");
            psi101.WorkingDirectory = workingPath;
            psi101.Arguments = fileName + ".tif " + fileName + " -l big -psm 6 hocr";
            var process101 = Process.Start(psi101);
            process101.WaitForExit();
        }

        private void cmd7_Click(object sender, EventArgs e)
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

            Cursor.Current = Cursors.WaitCursor;

            string fileAndPathNoExtension = txtInputPathAndFile.Text.Substring(0, txtInputPathAndFile.Text.IndexOf("."));
            string filenameOnly = fileAndPathNoExtension.Substring(fileAndPathNoExtension.LastIndexOf("\\") + 1);
            string pathOnly = fileAndPathNoExtension.Substring(0, fileAndPathNoExtension.Length - filenameOnly.Length - 1);

            bool stillLookingForNewDirectory = true;
            int count = 1;
            string workingPath = "";    // <-- This is the temporary directory you can put things in for this run

            while (stillLookingForNewDirectory)
            {
                workingPath = pathOnly + "\\out" + count.ToString();
                if (!Directory.Exists(workingPath))
                    stillLookingForNewDirectory = false;
                count++;
                Directory.CreateDirectory(workingPath);
            }

            // STEP 0:  ADJUST FOR RESOLUTION OF ORIGINAL SCREENSHOT
            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
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
                //x_percent_demand_b_column_width = 4;
                //x_percent_supply_a_column_width = 11;
            }

            // STEP 1:  GRAYSCALE
            bmp = MakeGrayscale3(bmp);

            // STEP 2:  INVERT
            bmp = Transform(bmp);

            // STEP 3:  SPLIT BITMAP INTO TWO PIECES
            // The first piece is the header (only get station name from this piece)
            // The second piece is the grid below, with all margins removed (everything else from here)        
            Bitmap bmpHeader = CropBitmap(bmp, 0, 0, (int)(bmp.Width * 75 / 100), (int)(bmp.Height * y_percent_header / 100));
            Bitmap bmpGrid = CropBitmap(bmp, (int)(bmp.Width * x_percent_left_margin / 100), (int)(bmp.Height * y_percent_header / 100), (int)(bmp.Width * x_percent_grid / 100), (int)(bmp.Height * y_percent_grid / 100));

            // STEP 4:  SAVE AS PNGs
            string pngFile = @workingPath + "\\header.png";
            bmpHeader.Save(@pngFile, ImageFormat.Png);
            pngFile = @workingPath + "\\grid.png";
            bmpGrid.Save(@pngFile, ImageFormat.Png);

            // CONVERT HEADER TO DATA
            // STEP 5A:  RUN SCAN TAILOR ON PNG HEADER
            string args = " --dpi=300 --threshold=-40 --layout=1 " + workingPath + "\\header.png " + workingPath;
            var process = Process.Start("\"C:\\Program Files (x86)\\Scan Tailor\\scantailor-cli.exe\"", args);
            process.WaitForExit();

            // STEP 5B:  CONVERT HEADER TO .PNG FOR TESSERACT
            System.Drawing.Bitmap.FromFile(workingPath + "\\header.tif").Save(workingPath + "\\header2.png", System.Drawing.Imaging.ImageFormat.Png);

            // STEP 5C:  RUN TESSERACT FOR HEADER
            var psi = new ProcessStartInfo("tesseract.exe");
            psi.WorkingDirectory = workingPath;
            psi.Arguments = "header2.png header -psm 6 hocr"; 
            var process2 = Process.Start(psi);
            process2.WaitForExit();

            // STEP 6:  GET HEADER STATION NAME FROM HEADER FILE
            string STATION_NAME = "";   // <-- THIS IS THE ONLY THING WE NEED FROM THIS HEADER FILE
            string hocrFileHeader = workingPath + "\\header.html";
            string[] lines = System.IO.File.ReadAllLines(hocrFileHeader);
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

            // STEP 7:  RUN SCAN TAILOR ON PNG FILE
            string args3 = " --dpi=250 --layout=1 --threshold=-18 --disable-content-detection " + workingPath + "\\grid.png " + workingPath;
            var process3 = Process.Start("\"C:\\Program Files (x86)\\Scan Tailor\\scantailor-cli.exe\"", args3);
            process3.WaitForExit();


            // STEP 8:  CONVERT TO .PNG FOR TESSERACT
            System.Drawing.Bitmap.FromFile(workingPath + "\\grid.tif").Save(workingPath + "\\grid2.png", System.Drawing.Imaging.ImageFormat.Png);
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
                        thisData.ocrDataRows.Add(thisDataRow);
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
                                for (int i = 0; i < thisData.ocrDataRows.Count; i++)
                                {
                                    ocrDataRow thisDataRow = thisData.ocrDataRows[i];

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
            for (int i = 0; i < thisData.ocrDataRows.Count; i++)
            {
                ocrDataRow thisItem = thisData.ocrDataRows[i];
                if (thisItem.isThisRowEmpty())
                    PrettySureImDoingThisWrongButWhatever.Add(i);
            }
            for (int i = PrettySureImDoingThisWrongButWhatever.Count - 1; i >= 0 ; i--)
            {
                // have to do this backwards so it will fucking work
                thisData.ocrDataRows.RemoveAt(PrettySureImDoingThisWrongButWhatever[i]);
            }

            // STEP 14:  CONVERT HOCR DATA INTO SERIALZIED DATA FILE
            XmlSerializer serializer = new XmlSerializer(thisData.GetType());
            StreamWriter writer = new StreamWriter(workingPath + "\\data.txt");
            serializer.Serialize(writer.BaseStream, thisData);

            // STEP 99:  DISPLAY THIS DATA ON A GRID IN THIS WINDOWS APP SO I CAN DEBUG FASTER
            lsvData.Clear();
            lblStation.Text = thisData.stationName;
            lsvData.Columns.Add("Goods", 150);
            lsvData.Columns.Add("Sell", 50);
            lsvData.Columns.Add("Buy", 50);
            lsvData.Columns.Add("Demand", 100);
            lsvData.Columns.Add("Supply", 100);

            foreach (ocrDataRow thisRow in thisData.ocrDataRows)
            {
                string[] arr = new string[5];
                arr[0] = thisRow.goodName;
                arr[1] = thisRow.sellPrice.ToString();
                arr[2] = thisRow.buyPrice.ToString();
                arr[3] = thisRow.demandA.ToString() + " " + thisRow.demandB.ToString();
                arr[4] = thisRow.supplyA.ToString() + " " + thisRow.supplyB.ToString();  
                ListViewItem lvi = new ListViewItem(arr);
                lsvData.Items.Add(lvi);
            }

            // Count the characters and display the number so I can see get accuracy % for my spreadsheet
            int charCount = 0;
            charCount += countChars(thisData.stationName);
            foreach (ocrDataRow thisRow in thisData.ocrDataRows)
            {
                charCount += countChars(thisRow.goodName);
                charCount += countChars(thisRow.sellPrice);
                charCount += countChars(thisRow.buyPrice);
                charCount += countChars(thisRow.supplyA);
                charCount += countChars(thisRow.supplyB);
                charCount += countChars(thisRow.demandA);
                charCount += countChars(thisRow.demandB);
            }
            lblCharNumber.Text = charCount.ToString();

            Cursor.Current = Cursors.Default;
        }

        public int countChars(string thisString)
        {
            string outString = thisString.Replace(" ", "");
            return outString.Length;
        }


        #region BUTTONS THAT ARE ONLY FOR TESTING NOW
        private void cmdOne_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);

            //            List<MikeyPixel> MikeyList = new List<MikeyPixel>();
            PixelRange thisRange = new PixelRange(0, 0, 0, 0, 0, 0);
            for (int i = 0; i < bmp.Height * bmp.Width; ++i)
            {
                int row = i / bmp.Width;
                int col = i % bmp.Width;

                var pixel = bmp.GetPixel(col, row);
                var mikeyP = new MikeyPixel(pixel.R, pixel.G, pixel.B, pixel.A);
                //                if (!isThisPixelInTheList(MikeyList, mikeyP))
                //                    MikeyList.Add(mikeyP);
                if (row == 0 && col == 0)
                {
                    // initial load
                    thisRange.data_R1 = mikeyP.data_R;
                    thisRange.data_R2 = mikeyP.data_R;
                    thisRange.data_G1 = mikeyP.data_G;
                    thisRange.data_G2 = mikeyP.data_G;
                    thisRange.data_B1 = mikeyP.data_B;
                    thisRange.data_B2 = mikeyP.data_B;
                }
                else
                {
                    if (mikeyP.data_R < thisRange.data_R1)
                        thisRange.data_R1 = mikeyP.data_R;
                    if (mikeyP.data_G < thisRange.data_G1)
                        thisRange.data_G1 = mikeyP.data_G;
                    if (mikeyP.data_B < thisRange.data_B1)
                        thisRange.data_B1 = mikeyP.data_B;

                    if (mikeyP.data_R > thisRange.data_R2)
                        thisRange.data_R2 = mikeyP.data_R;
                    if (mikeyP.data_G > thisRange.data_G2)
                        thisRange.data_G2 = mikeyP.data_G;
                    if (mikeyP.data_B > thisRange.data_B2)
                        thisRange.data_B2 = mikeyP.data_B;
                }
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("R: " + thisRange.data_R1 + "-" + thisRange.data_R2 + ", G: " + thisRange.data_G1 + "-" + thisRange.data_G2 + ", B: " + thisRange.data_B1 + "-" + thisRange.data_B2);
        }

        private void cmdTwo_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            //set up the four ranges
            PixelRange range1 = new PixelRange(0, 200, 0, 200, 0, 200);
            //            PixelRange range1 = new PixelRange(73, 79, 35, 46, 9, 31);
            //            PixelRange range2 = new PixelRange(190, 211, 81, 90, 9, 18);
            //            PixelRange range3 = new PixelRange(255, 255, 105, 108, 6, 7);
            //            PixelRange range4 = new PixelRange(139, 184, 64, 86, 22, 28);

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
            //Bitmap bmp_new = new Bitmap(bmp.Width, bmp.Height);

            for (int i = 0; i < bmp.Height * bmp.Width; ++i)
            {
                int row = i / bmp.Width;
                int col = i % bmp.Width;

                var pixel = bmp.GetPixel(col, row);
                var mikeyP = new MikeyPixel(pixel.R, pixel.G, pixel.B, pixel.A);

                bool isThisTheRightColor = false;

                if (isThisPixelInThisRange(mikeyP, range1))
                    isThisTheRightColor = true;
                //                if (isThisPixelInThisRange(mikeyP, range2))
                //                   isThisTheRightColor = true;
                //                if (isThisPixelInThisRange(mikeyP, range3))
                //                    isThisTheRightColor = true;
                //                if (isThisPixelInThisRange(mikeyP, range4))
                //                    isThisTheRightColor = true;

                //if (isThisTheRightColor)
                //    bmp.SetPixel(col, row, Color.Black);

                if (!isThisTheRightColor)
                    bmp.SetPixel(col, row, Color.White);

            }

            bmp.Save(findNewFilePath(txtInputPathAndFile.Text));
            Cursor.Current = Cursors.Default;
        }

        private void cmdThree_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
            Bitmap bmp_new = MakeGrayscale3(bmp);
            bmp_new.Save(findNewFilePath(txtInputPathAndFile.Text));

            Cursor.Current = Cursors.Default;
        }


        private void cmdFour_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
            Bitmap bmp_new = Transform(bmp);
            bmp_new.Save(findNewFilePath(txtInputPathAndFile.Text));

            Cursor.Current = Cursors.Default;
        }

        private void cmdFive_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
            Bitmap bmp_new = AdjustContrast(bmp, float.Parse(txtContrast.Text));
            bmp_new.Save(findNewFilePath(txtInputPathAndFile.Text));

            Cursor.Current = Cursors.Default;
        }

        private void cmd6_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Bitmap bmp = new Bitmap(txtInputPathAndFile.Text);
            AdjustBrightnessMatrix(bmp, int.Parse(txtBrightness.Text));
            bmp.Save(findNewFilePath(txtInputPathAndFile.Text), ImageFormat.Tiff);

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region METHODS/CLASSES THAT ARE ONLY FOR TESTING NOW
        public string findNewFilePath(string incomingString)
        {
            string pathAndFileNameWithoutExtension = incomingString.Substring(0, incomingString.IndexOf("."));
            string fileExtention = incomingString.Substring(pathAndFileNameWithoutExtension.Length);
            string newFile = "";
            bool stillLookingForNewFileSpot = true;
            int count = 1;
            while (stillLookingForNewFileSpot)
            {
                newFile = pathAndFileNameWithoutExtension + "_" + count.ToString() + fileExtention;
                if (!File.Exists(newFile))
                    stillLookingForNewFileSpot = false;
                count++;
            }
            return newFile;
        }

        public static Bitmap AdjustContrast(Bitmap Image, float Value)
        {
            Value = (100.0f + Value) / 100.0f;
            Value *= Value;
            System.Drawing.Bitmap NewBitmap = Image;

            for (int x = 0; x < NewBitmap.Width; ++x)
            {
                for (int y = 0; y < NewBitmap.Height; ++y)
                {
                    Color Pixel = NewBitmap.GetPixel(x, y);
                    float Red = Pixel.R / 255.0f;
                    float Green = Pixel.G / 255.0f;
                    float Blue = Pixel.B / 255.0f;
                    Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                    Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                    Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;
                    NewBitmap.SetPixel(x, y, Color.FromArgb(Clamp((int)Red, 255, 0), Clamp((int)Green, 255, 0), Clamp((int)Blue, 255, 0)));
                }
            }

            return NewBitmap;
        }

        public static T Clamp<T>(T Value, T Max, T Min)
             where T : System.IComparable<T>
        {
            if (Value.CompareTo(Max) > 0)
                return Max;
            if (Value.CompareTo(Min) < 0)
                return Min;
            return Value;
        }

        public static void AdjustBrightnessMatrix(Bitmap img, int value)
        {
            if (value == 0) // No change, so just return
                return;

            float sb = (float)value / 255F;
            float[][] colorMatrixElements =
                  {
                        new float[] {1,  0,  0,  0, 0},
                        new float[] {0,  1,  0,  0, 0},
                        new float[] {0,  0,  1,  0, 0},
                        new float[] {0,  0,  0,  1, 0},
                        new float[] {sb, sb, sb, 1, 1}
                  };

            ColorMatrix cm = new ColorMatrix(colorMatrixElements);
            ImageAttributes imgattr = new ImageAttributes();
            Rectangle rc = new Rectangle(0, 0, img.Width, img.Height);
            Graphics g = Graphics.FromImage(img);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            imgattr.SetColorMatrix(cm);
            g.DrawImage(img, rc, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgattr);

            //Clean everything up
            imgattr.Dispose();
            g.Dispose();
        }


        private bool isThisPixelInTheList(List<MikeyPixel> thisList, MikeyPixel inputPixel)
        {
            bool thisResult = false;
            foreach (MikeyPixel thisPixel in thisList)
            {
                if (areThesePixelsTheSame(thisPixel, inputPixel))
                    thisResult = true;
            }

            return thisResult;
        }
        private bool areThesePixelsTheSame(MikeyPixel pixel1, MikeyPixel pixel2)
        {
            bool thisResult = false;
            if (pixel1.data_A == pixel2.data_A)
            {
                if (pixel1.data_B == pixel2.data_B)
                {
                    if (pixel1.data_G == pixel2.data_G)
                    {
                        if (pixel1.data_R == pixel2.data_R)
                            thisResult = true;
                    }
                }
            }
            return thisResult;
        }
        private bool isThisPixelInThisRange(MikeyPixel thisPixel, PixelRange thisRange)
        {
            bool thisResult = false;
            if (thisPixel.data_R >= thisRange.data_R1 && thisPixel.data_R <= thisRange.data_R2)
            {
                if (thisPixel.data_G >= thisRange.data_G1 && thisPixel.data_G <= thisRange.data_G2)
                {
                    if (thisPixel.data_B >= thisRange.data_B1 && thisPixel.data_B <= thisRange.data_B2)
                        thisResult = true;
                }
            }

            return thisResult;
        }
        class MikeyPixel
        {
            public int data_R;
            public int data_G;
            public int data_B;
            public int data_A;

            public MikeyPixel(int in_R, int in_G, int in_B, int in_A)
            {
                data_R = in_R;
                data_G = in_G;
                data_B = in_B;
                data_A = in_A;
            }
        }
        class PixelRange
        {
            public int data_R1;
            public int data_G1;
            public int data_B1;

            public int data_R2;
            public int data_G2;
            public int data_B2;

            public PixelRange(int in_R1, int in_R2, int in_G1, int in_G2, int in_B1, int in_B2)
            {
                data_R1 = in_R1;
                data_G1 = in_G1;
                data_B1 = in_B1;

                data_R2 = in_R2;
                data_G2 = in_G2;
                data_B2 = in_B2;
            }
        }
        #endregion

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        #region OLD CODE FOR ARCHIVE
        //// STEP 8:  GET POSITIONAL DATA FROM ALL FILE
        //string hocrFileAll = workingPath + "\\all.html";
        //string[] lines2 = System.IO.File.ReadAllLines(hocrFileAll);
        //foreach (string line in lines2)
        //{
        //    string thisLine = line.Replace("<strong>", "");
        //    thisLine = thisLine.Replace("<em>", "");

        //    // <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
        //    Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");
        //    while (match.Success)
        //    {
        //        if (match.Success)
        //        {
        //            ocrWord thisWord = new ocrWord(match.Groups[5].Value.ToString(), int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
        //            //ocrList.Add(thisWord);
        //        }
        //        match = match.NextMatch();

        //    }
        //}

        // STEP ?:  UNPACK DATA FROM OCR HTML
        //ocrData thisData = new ocrData();
        //string hocrFile = workingPath + "\\out.html";
        //string[] lines3 = System.IO.File.ReadAllLines(hocrFile);
        //foreach (string line in lines)
        //{
        //    List<ocrWord> ocrList = new List<ocrWord>();
        //    //       <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
        //    string thisLine = line.Replace("<strong>", "");
        //    thisLine = thisLine.Replace("<em>", "");
        //    Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");
        //    while (match.Success)
        //    {
        //        if (match.Success)
        //        {
        //            ocrWord thisWord = new ocrWord(match.Groups[5].Value.ToString(), int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
        //            ocrList.Add(thisWord);
        //        }
        //        match = match.NextMatch();
        //    }
        //    // Process the ocrWord list
        //    if (ocrList.Count > 0)
        //        AddOcrData(ocrList, thisData);
        //}

        //XmlSerializer serializer = new XmlSerializer(thisData.GetType());
        //StreamWriter writer = new StreamWriter(workingPath + "\\" + filenameOnly + ".txt");
        //serializer.Serialize(writer.BaseStream, thisData);


        //// STEP ?:  RUN TESSERACT FOR ALL
        //var psi2 = new ProcessStartInfo("tesseract.exe");
        //psi2.WorkingDirectory = workingPath;
        //psi2.Arguments = "grid2.png grid -l big -psm 3 hocr";
        ////var process4 = Process.Start(psi2);
        ////process4.WaitForExit();


        //// STEP ?:  GET POSITIONAL DATA FROM ALL FILE
        //string hocrFileAll = workingPath + "\\all.html";
        //string[] lines2 = System.IO.File.ReadAllLines(hocrFileAll);
        //foreach (string line in lines2)
        //{
        //    string thisLine = line.Replace("<strong>", "");
        //    thisLine = thisLine.Replace("<em>", "");

        //    // <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
        //    Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");
        //    while (match.Success)
        //    {
        //        if (match.Success)
        //        {
        //            ocrWord thisWord = new ocrWord(match.Groups[5].Value.ToString(), int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
        //            //ocrList.Add(thisWord);
        //        }
        //        match = match.NextMatch();

        //    }
        //}



        // STEP ?:  UNPACK DATA FROM OCR HTML
        //ocrData thisData = new ocrData();
        //string hocrFile = workingPath + "\\out.html";
        //string[] lines3 = System.IO.File.ReadAllLines(hocrFile);
        //foreach (string line in lines)
        //{
        //    List<ocrWord> ocrList = new List<ocrWord>();
        //    //       <span class='ocrx_word' id='[a-z]+_[0-9]+' title="bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)">([a-zA-Z0-9.,]+)
        //    string thisLine = line.Replace("<strong>", "");
        //    thisLine = thisLine.Replace("<em>", "");
        //    Match match = Regex.Match(thisLine, "<span class='ocrx_word' id='[a-z]+_[0-9]+' title=\"bbox ([0-9]+) ([0-9]+) ([0-9]+) ([0-9]+)\">([a-zA-Z0-9.,]+)");
        //    while (match.Success)
        //    {
        //        if (match.Success)
        //        {
        //            ocrWord thisWord = new ocrWord(match.Groups[5].Value.ToString(), int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));
        //            ocrList.Add(thisWord);
        //        }
        //        match = match.NextMatch();
        //    }
        //    // Process the ocrWord list
        //    if (ocrList.Count > 0)
        //        AddOcrData(ocrList, thisData);
        //}

        //XmlSerializer serializer = new XmlSerializer(thisData.GetType());
        //StreamWriter writer = new StreamWriter(workingPath + "\\" + filenameOnly + ".txt");
        //serializer.Serialize(writer.BaseStream, thisData)

        //private void AddOcrData(List<ocrWord> theseWords, ocrData Data)
        //{
        //    // X-VARIABLES
        //    rangeData x_goods_column = new rangeData("x", 477, 1000);
        //    rangeData x_sell_column = new rangeData("x", 1001, 1280);
        //    rangeData x_buy_column = new rangeData("x", 1281, 1455);
        //    rangeData x_demand_column = new rangeData("x", 1456, 1960);
        //    rangeData x_supply_column = new rangeData("x", 1961, 2350);

        //    // Y-VARIABLES
        //    rangeData y_station = new rangeData("y", 119, 123);
        //    rangeData y_goods = new rangeData("y", 450, 2000);  // this is the y-range that all goods can land in

        //    ocrDataRow thisOcrRow = new ocrDataRow();
        //    bool thisIsDataWeWant = false;
        //    foreach (ocrWord thisWord in theseWords)
        //    {
        //        if (thisWord.isItInRange(y_station))
        //             Data.AddStationData(thisWord.text);
        //        else
        //        {
        //            if (thisWord.isItInRange(y_goods))
        //            {
        //                // this is a value we want, now we need to check which column it is in
        //                if (thisWord.isItInRange(x_goods_column))
        //                    thisOcrRow.AddGoodsNameData(thisWord.text);
        //                else if (thisWord.isItInRange(x_sell_column))
        //                {
        //                    thisOcrRow.AddSellPriceData(thisWord.text);
        //                    thisIsDataWeWant = true;
        //                }
        //                else if (thisWord.isItInRange(x_buy_column))
        //                {
        //                    thisOcrRow.AddBuyPriceData(thisWord.text);
        //                    thisIsDataWeWant = true;
        //                }
        //                else if (thisWord.isItInRange(x_demand_column))
        //                {
        //                    thisOcrRow.AddDemandData(thisWord.text);
        //                    thisIsDataWeWant = true;
        //                }
        //                else if (thisWord.isItInRange(x_supply_column))
        //                {
        //                    thisOcrRow.AddSupplyNameData(thisWord.text);
        //                    thisIsDataWeWant = true;
        //                }
        //            }
        //        }
        //    }
        //    if (thisIsDataWeWant)
        //        Data.ocrDataRows.Add(thisOcrRow);
        //}

        //public class rangeData
        //{
        //    public string axis;
        //    public int range1;
        //    public int range2;

        //    public rangeData(string _axis, int _range1, int _range2)
        //    {
        //        axis = _axis;
        //        range1 = _range1;
        //        range2 = _range2;
        //    }
        //}

        //public class ocrWord
        //{
        //    public string text;
        //    public int x0;
        //    public int x1;
        //    public int y0;
        //    public int y1;

        //    public ocrWord(string _text, int _x0, int _y0, int _x1, int _y1)
        //    {
        //        this.text = _text;
        //        this.x0 = _x0;
        //        this.x1 = _x1;
        //        this.y0 = _y0;
        //        this.y1 = _y1;
        //    }
        //}
        #endregion


    }
}
