namespace BitmapChanger
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtInputPathAndFile = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.cmdOpenFile = new System.Windows.Forms.Button();
            this.cmdExit = new System.Windows.Forms.Button();
            this.cmd7 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBrightness = new System.Windows.Forms.TextBox();
            this.cmd6 = new System.Windows.Forms.Button();
            this.txtContrast = new System.Windows.Forms.TextBox();
            this.cmdFive = new System.Windows.Forms.Button();
            this.cmdFour = new System.Windows.Forms.Button();
            this.cmdThree = new System.Windows.Forms.Button();
            this.cmdTwo = new System.Windows.Forms.Button();
            this.cmdOne = new System.Windows.Forms.Button();
            this.lsvData = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStation = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCharNumber = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtInputPathAndFile
            // 
            this.txtInputPathAndFile.Location = new System.Drawing.Point(93, 56);
            this.txtInputPathAndFile.Name = "txtInputPathAndFile";
            this.txtInputPathAndFile.Size = new System.Drawing.Size(367, 20);
            this.txtInputPathAndFile.TabIndex = 0;
            this.txtInputPathAndFile.Text = "C:\\workspace\\BitmapChanger\\Images\\tests\\2.bmp";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.InitialDirectory = "C:\\workspace\\BitmapChanger\\Images\\tests";
            // 
            // cmdOpenFile
            // 
            this.cmdOpenFile.Location = new System.Drawing.Point(229, 28);
            this.cmdOpenFile.Name = "cmdOpenFile";
            this.cmdOpenFile.Size = new System.Drawing.Size(86, 22);
            this.cmdOpenFile.TabIndex = 1;
            this.cmdOpenFile.Text = "Choose File";
            this.cmdOpenFile.UseVisualStyleBackColor = true;
            this.cmdOpenFile.Click += new System.EventHandler(this.cmdOpenFile_Click);
            // 
            // cmdExit
            // 
            this.cmdExit.Location = new System.Drawing.Point(229, 541);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.Size = new System.Drawing.Size(86, 22);
            this.cmdExit.TabIndex = 2;
            this.cmdExit.Text = "Exit";
            this.cmdExit.UseVisualStyleBackColor = true;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // cmd7
            // 
            this.cmd7.Location = new System.Drawing.Point(229, 91);
            this.cmd7.Name = "cmd7";
            this.cmd7.Size = new System.Drawing.Size(86, 22);
            this.cmd7.TabIndex = 11;
            this.cmd7.Text = "DO IT";
            this.cmd7.UseVisualStyleBackColor = true;
            this.cmd7.Click += new System.EventHandler(this.cmd7_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "*****BELOW ARE OLD TESTING FUNCTIONS*****";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBrightness);
            this.groupBox1.Controls.Add(this.cmd6);
            this.groupBox1.Controls.Add(this.txtContrast);
            this.groupBox1.Controls.Add(this.cmdFive);
            this.groupBox1.Controls.Add(this.cmdFour);
            this.groupBox1.Controls.Add(this.cmdThree);
            this.groupBox1.Controls.Add(this.cmdTwo);
            this.groupBox1.Controls.Add(this.cmdOne);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(120, 569);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(311, 242);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            // 
            // txtBrightness
            // 
            this.txtBrightness.Location = new System.Drawing.Point(149, 207);
            this.txtBrightness.Name = "txtBrightness";
            this.txtBrightness.Size = new System.Drawing.Size(88, 20);
            this.txtBrightness.TabIndex = 20;
            // 
            // cmd6
            // 
            this.cmd6.Location = new System.Drawing.Point(57, 205);
            this.cmd6.Name = "cmd6";
            this.cmd6.Size = new System.Drawing.Size(86, 22);
            this.cmd6.TabIndex = 19;
            this.cmd6.Text = "Brightness";
            this.cmd6.UseVisualStyleBackColor = true;
            // 
            // txtContrast
            // 
            this.txtContrast.Location = new System.Drawing.Point(149, 161);
            this.txtContrast.Name = "txtContrast";
            this.txtContrast.Size = new System.Drawing.Size(88, 20);
            this.txtContrast.TabIndex = 18;
            // 
            // cmdFive
            // 
            this.cmdFive.Location = new System.Drawing.Point(57, 161);
            this.cmdFive.Name = "cmdFive";
            this.cmdFive.Size = new System.Drawing.Size(86, 22);
            this.cmdFive.TabIndex = 17;
            this.cmdFive.Text = "Contrast";
            this.cmdFive.UseVisualStyleBackColor = true;
            // 
            // cmdFour
            // 
            this.cmdFour.Location = new System.Drawing.Point(193, 105);
            this.cmdFour.Name = "cmdFour";
            this.cmdFour.Size = new System.Drawing.Size(86, 22);
            this.cmdFour.TabIndex = 16;
            this.cmdFour.Text = "Invert";
            this.cmdFour.UseVisualStyleBackColor = true;
            // 
            // cmdThree
            // 
            this.cmdThree.Location = new System.Drawing.Point(57, 105);
            this.cmdThree.Name = "cmdThree";
            this.cmdThree.Size = new System.Drawing.Size(86, 22);
            this.cmdThree.TabIndex = 15;
            this.cmdThree.Text = "Greyscale";
            this.cmdThree.UseVisualStyleBackColor = true;
            // 
            // cmdTwo
            // 
            this.cmdTwo.Location = new System.Drawing.Point(193, 63);
            this.cmdTwo.Name = "cmdTwo";
            this.cmdTwo.Size = new System.Drawing.Size(86, 22);
            this.cmdTwo.TabIndex = 14;
            this.cmdTwo.Text = "Empty Range";
            this.cmdTwo.UseVisualStyleBackColor = true;
            // 
            // cmdOne
            // 
            this.cmdOne.Location = new System.Drawing.Point(57, 63);
            this.cmdOne.Name = "cmdOne";
            this.cmdOne.Size = new System.Drawing.Size(86, 22);
            this.cmdOne.TabIndex = 13;
            this.cmdOne.Text = "Analyze";
            this.cmdOne.UseVisualStyleBackColor = true;
            // 
            // lsvData
            // 
            this.lsvData.GridLines = true;
            this.lsvData.Location = new System.Drawing.Point(9, 136);
            this.lsvData.Name = "lsvData";
            this.lsvData.ShowGroups = false;
            this.lsvData.Size = new System.Drawing.Size(536, 399);
            this.lsvData.TabIndex = 14;
            this.lsvData.UseCompatibleStateImageBehavior = false;
            this.lsvData.View = System.Windows.Forms.View.Details;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Station Name:";
            // 
            // lblStation
            // 
            this.lblStation.AutoSize = true;
            this.lblStation.Location = new System.Drawing.Point(117, 119);
            this.lblStation.Name = "lblStation";
            this.lblStation.Size = new System.Drawing.Size(0, 13);
            this.lblStation.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(332, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Total Number of Characters:";
            // 
            // lblCharNumber
            // 
            this.lblCharNumber.AutoSize = true;
            this.lblCharNumber.Location = new System.Drawing.Point(488, 120);
            this.lblCharNumber.Name = "lblCharNumber";
            this.lblCharNumber.Size = new System.Drawing.Size(0, 13);
            this.lblCharNumber.TabIndex = 18;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(482, 796);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(28, 13);
            this.lblVersion.TabIndex = 19;
            this.lblVersion.Text = "v1.0";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 819);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblCharNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lsvData);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmd7);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.cmdOpenFile);
            this.Controls.Add(this.txtInputPathAndFile);
            this.Name = "frmMain";
            this.Text = "Serenity OCR for Elite: Dangerous";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInputPathAndFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button cmdOpenFile;
        private System.Windows.Forms.Button cmdExit;
        private System.Windows.Forms.Button cmd7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtBrightness;
        private System.Windows.Forms.Button cmd6;
        private System.Windows.Forms.TextBox txtContrast;
        private System.Windows.Forms.Button cmdFive;
        private System.Windows.Forms.Button cmdFour;
        private System.Windows.Forms.Button cmdThree;
        private System.Windows.Forms.Button cmdTwo;
        private System.Windows.Forms.Button cmdOne;
        private System.Windows.Forms.ListView lsvData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCharNumber;
        private System.Windows.Forms.Label lblVersion;
    }
}

