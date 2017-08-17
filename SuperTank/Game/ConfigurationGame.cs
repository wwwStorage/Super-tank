﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperTank
{
    public class ConfigurationGame : ConfigurationBase
    {
        private static int countLevel = 1;
        private static Point positionEagle = new Point(12 * ConfigurationGame.WidthTile, ConfigurationGame.HeightBoard - ConfigurationGame.HeightTile * 2);
        private static Point startPositionTankPlaeyr = new Point(9 * ConfigurationGame.WidthTile, ConfigurationGame.HeightBoard - ConfigurationGame.HeigthTank);

        public static string Maps { get { return @"Content\Maps\"; } }
        public static int CountLevel { get { return countLevel; } }
        public static Point PositionEagle { get { return positionEagle; } }
        public static Point StartPositionTankPlaeyr { get { return startPositionTankPlaeyr; } }
    }
}