using System;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.IO;

namespace SerenityClientOCR
{
	/// <summary>
	/// 
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            if (!IsAlreadyRunning())
            {

                // Show the system tray icon.					
                using (ProcessIcon pi = new ProcessIcon())
                {
                    pi.Display();

                    // Make sure the application runs!
                    Application.Run();
                }
            }
		}

        private static bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;
            bool bCreatedNew;

            Mutex mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
            if (bCreatedNew)
                mutex.ReleaseMutex();

            return !bCreatedNew;
        }
	}


}