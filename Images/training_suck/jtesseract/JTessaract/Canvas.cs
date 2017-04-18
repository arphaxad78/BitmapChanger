using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace JTessaract
{
    public partial class Canvas : UserControl
    {
        private Image mainImage = null;
        private float locationX = 0;
        private float locationY = 0;
        private float scaleX = 1;
        private float scaleY = 1;
        private bool mouseDown = false;
        private float oldLocationX, oldLocationY, startLocationX, startLocationY;
        private int currentBox = -1;

        public int CurrentBox
        {
            get { return currentBox; }
            set { currentBox = value; }
        }

        private ArrayList boxes = null;

        public ArrayList Boxes
        {
            get { return boxes; }
            set { boxes = value; }
        }


        public Image MainImage
        {
            get { return mainImage; }
            set 
            { 
                mainImage = value;
                scaleX = 1;
                scaleY = 1;
                CenterImage();
            }
        }

        public Canvas()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Canvas_MouseWheel);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldScaleX, oldScaleY;
            oldScaleX = scaleX;
            oldScaleY = scaleY;

            if (e.Delta > 0)
            {
                scaleX *= 0.95f;
                scaleY *= 0.95f;
            }
            else if (e.Delta < 0)
            {
                scaleX *= 1.05f;
                scaleY *= 1.05f;
            }

            locationX = e.X - (e.X - locationX) * (oldScaleX / scaleX);
            locationY = e.Y - (e.Y - locationY) * (oldScaleY / scaleY);

            this.Invalidate();
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            oldLocationX = locationX;
            oldLocationY = locationY;
            startLocationX = e.X;
            startLocationY = e.Y;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                locationX = oldLocationX + (e.X - startLocationX);
                locationY = oldLocationY + (e.Y - startLocationY);

                this.Invalidate();
            }
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (mainImage != null)
            {
                g.DrawImage(mainImage, locationX, locationY, mainImage.Width / scaleX, mainImage.Height / scaleY);
                if (boxes != null)
                {
                    Pen nonSelectedPen = new Pen(Color.Red, 1.0f);
                    Pen selectedPen = new Pen(Color.Blue, 1.2f);
                    Pen fillPen = new Pen(Color.FromArgb(50, Color.Blue));
                    for (int i = boxes.Count - 1; i >= 0; i--)
                    {
                        Box box = (Box)boxes[i];
                     
                        if (i == currentBox)
                        {
                            g.DrawRectangle(selectedPen, locationX + box.X1 / scaleX, locationY + (mainImage.Height - box.Y2) / scaleY, (box.X2 - box.X1) / scaleX, (box.Y2 - box.Y1) / scaleY);
                            g.FillRectangle(fillPen.Brush, locationX + box.X1 / scaleX, locationY + (mainImage.Height - box.Y2) / scaleY, (box.X2 - box.X1) / scaleX, (box.Y2 - box.Y1) / scaleY);
                        }
                        else 
                        {
                            g.DrawRectangle(nonSelectedPen, locationX + box.X1 / scaleX, locationY + (mainImage.Height - box.Y2) / scaleY, (box.X2 - box.X1) / scaleX, (box.Y2 - box.Y1) / scaleY);
                        }
                    }

                    fillPen.Dispose();
                    nonSelectedPen.Dispose();
                    selectedPen.Dispose();
                }
            }
        }

        private void Canvas_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void CenterImage()
        {
            if (mainImage != null)
            {
                locationX = (this.Width - mainImage.Width) / (scaleX * 2.0f);
                locationY = (this.Height - mainImage.Height) / (scaleY * 2.0f);
            }
        }
    }
}
