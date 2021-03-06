﻿using SuperTank.View;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SuperTank.WindowsForms
{
    /// <summary>
    /// The level design screen
    /// </summary>
    class ScrenConstructor : Label
    {
        private Image brickWall;
        private Image concreteWall;
        private Image water_1;
        private Image forest;
        private Image ice;

        private Timer timer = new Timer();
        private Image imgTank = Images.Plaeyr.SmallTank.Up1;
        private Point posTank;
        private char spaceChar = ' ';
        private char currentChar;
        private char[,] map;
        private int col;
        private int row;

        public bool left;
        public bool right;
        public bool up;
        public bool down;
        public bool space;
        public bool enter;

        public ScrenConstructor()
        {
            brickWall = Images.ImagesBlickWall.BrickWall;
            concreteWall = Images.ConcreteWall;
            water_1 = Images.Water_1;
            forest = Images.Forest;
            ice = Images.Ice;

            this.BackColor = ConfigurationWinForms.BackColor;
            col = ConfigurationWinForms.WidthBoard / ConfigurationWinForms.WidthTile;
            row = ConfigurationWinForms.HeightBoard / ConfigurationWinForms.HeightTile;
            map = new char[col, row];
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            GraphicsOption();
        }

        public bool IsActiv { get; set; }

        public char[,] GetMap()
        {
            return map;
        }
        public new void KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    left = true;
                    break;
                case Keys.Right:
                    right = true;
                    break;
                case Keys.Up:
                    up = true;
                    break;
                case Keys.Down:
                    down = true;
                    break;
                case Keys.Space:
                    space = true;
                    break;
                case Keys.Enter:
                    enter = true;
                    break;
            }
        }
        public new void KeyUp(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    left = false;
                    break;
                case Keys.Right:
                    right = false;
                    break;
                case Keys.Up:
                    up = false;
                    break;
                case Keys.Down:
                    down = false;
                    break;
                case Keys.Space:
                    space = false;
                    break;
                case Keys.Enter:
                    enter = false;
                    NextChar();
                    break;
            }
        }
        public void Stop()
        {
            timer.Stop();
            IsActiv = false;
        }
        public void Start()
        {
            currentChar = ConfigurationWinForms.CharBrickWall;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    map[i, j] = spaceChar;
                }
            }
            posTank = new Point();
            IsActiv = true;
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, ConfigurationWinForms.WidthTile, ConfigurationWinForms.HeightTile, ConfigurationWinForms.WidthBoard, ConfigurationWinForms.HeightBoard);

            Image img = null;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    img = GetImg(map[i, j]);
                    if (img == null) continue;
                    g.DrawImage(img, j * ConfigurationWinForms.WidthTile + ConfigurationWinForms.WidthTile, i * ConfigurationWinForms.HeightTile + ConfigurationWinForms.HeightTile, ConfigurationWinForms.WidthTile, ConfigurationWinForms.HeightTile);
                }
            }

            g.DrawImage(imgTank, posTank.X * ConfigurationWinForms.WidthTank + ConfigurationWinForms.WidthTile, posTank.Y * ConfigurationWinForms.HeightTank + ConfigurationWinForms.HeightTile);
        }

        private Image GetImg(char c)
        {
            if (c == ConfigurationWinForms.CharBrickWall)
            {
                return brickWall;
            }
            else if (c == ConfigurationWinForms.CharConcreteWall)
            {
                return concreteWall;
            }
            else if (c == ConfigurationWinForms.CharWater)
            {
                return water_1;
            }
            else if (c == ConfigurationWinForms.CharForest)
            {
                return forest;
            }
            else if (c == ConfigurationWinForms.CharIce)
            {
                return ice;
            }
            return null;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (left)
            {
                posTank.X--;
                if (posTank.X < 0) posTank.X = 0;
            }
            else if (right)
            {
                posTank.X++;
                if (posTank.X >= col / 2) posTank.X = col / 2 - 1;
            }
            else if (up)
            {
                posTank.Y--;
                if (posTank.Y < 0) posTank.Y = 0;
            }
            else if (down)
            {
                posTank.Y++;
                if (posTank.Y >= row / 2) posTank.Y = row / 2 - 1;
            }
            else if (space) ;
            else
            {
                return;
            }

            if (space)
            {
                int i = posTank.Y * 2;
                int j = posTank.X * 2;
                map[i, j] = map[i, j + 1] = map[i + 1, j] = map[i + 1, j + 1] = currentChar;
            }

            Invalidate();
        }
        private void NextChar()
        {
            if (currentChar == spaceChar)
            {
                currentChar = ConfigurationWinForms.CharBrickWall;
            }
            else if (currentChar == ConfigurationWinForms.CharBrickWall)
            {
                currentChar = ConfigurationWinForms.CharConcreteWall;
            }
            else if (currentChar == ConfigurationWinForms.CharConcreteWall)
            {
                currentChar = ConfigurationWinForms.CharForest;
            }
            else if (currentChar == ConfigurationWinForms.CharForest)
            {
                currentChar = ConfigurationWinForms.CharIce;
            }
            else if (currentChar == ConfigurationWinForms.CharIce)
            {
                currentChar = ConfigurationWinForms.CharWater;
            }
            else if (currentChar == ConfigurationWinForms.CharWater)
            {
                currentChar = spaceChar;
            }
        }
        private void GraphicsOption()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
