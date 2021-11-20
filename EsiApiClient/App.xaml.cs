﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EsiApiClient.Models;

namespace EsiApiClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigModel AppConfig { get; set; }
        public static string ConfigFilePath = Directory.GetCurrentDirectory() + @"\config.json";

    }
}
