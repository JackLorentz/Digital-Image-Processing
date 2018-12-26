using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIP_HW
{
    static class Program
    {
        static SplashScreen splashScreen;
        static Form1 form1;
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            splashScreen = new SplashScreen();
            if(splashScreen != null)
            {
                Thread splashThread = new Thread(new ThreadStart(() => { Application.Run(splashScreen); }));
                splashThread.SetApartmentState(ApartmentState.STA);
                splashThread.Start();
            }
            form1 = new Form1();
            form1.LoadCompleted += MainForm_LoadCompleted;
            Application.Run(form1);
            if (!(splashScreen == null || splashScreen.Disposing || splashScreen.IsDisposed))
            {
                splashScreen.Invoke(new Action(() => {
                    splashScreen.TopMost = true;
                    splashScreen.Activate();
                    splashScreen.TopMost = false;
                }));
            }        
        }

        private static void MainForm_LoadCompleted(object sender, EventArgs e)
        {
            if (splashScreen == null || splashScreen.Disposing || splashScreen.IsDisposed)
            {
                return;
            }    
            splashScreen.Invoke(new Action(() => { splashScreen.Close(); }));
            splashScreen.Dispose();
            splashScreen = null;
            form1.TopMost = true;
            form1.Activate();
            form1.TopMost = false;
        }
    }
}
