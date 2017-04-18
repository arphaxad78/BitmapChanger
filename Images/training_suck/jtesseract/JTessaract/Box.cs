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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace JTessaract
{
    public class Box
    {
        private string charactor;

        public string Charactor
        {
            get { return charactor; }
            set { charactor = value; }
        }
        private int x1;

        public int X1
        {
            get { return x1; }
            set { x1 = value; }
        }
        private int y1;

        public int Y1
        {
            get { return y1; }
            set { y1 = value; }
        }
        private int x2;

        public int X2
        {
            get { return x2; }
            set { x2 = value; }
        }
        private int y2;

        public int Y2
        {
            get { return y2; }
            set { y2 = value; }
        }
    }
}
