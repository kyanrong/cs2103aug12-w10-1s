﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Type
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Presenter app;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            app = new Presenter();
        }
    }
}
