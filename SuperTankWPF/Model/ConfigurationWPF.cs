﻿using SuperTank.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperTankWPF.Model
{
    class ConfigurationWPF : ConfigurationView
    {
        protected ConfigurationWPF() { }

        public static string TexturePath { get { return "..\\" + ConfigurationView.TexturePath; } }
    }
}