using HyakuninKumiteApplication.Services;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace HyakuninKumiteApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            /*this.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "Crash");
                e.Handled = true;
            };*/
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.ExceptionObject.ToString(), "Fatal Crash");
            };

            #if DEBUG
            AllocConsole();
            Task.Run(CMDService.Start);
            #endif

            Console.WriteLine("Application Started");

            base.OnStartup(e);

            // 1. Show the custom splash window
            SplashScreenWindow splash = new SplashScreenWindow();
            splash.Show();

            // 2. Wait for 1 second
            await Task.Delay(750);

            // 3. Initialize and show the Main Window
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // 4. Close the splash window once the main window is ready
            splash.Close();
        }


    }
}
