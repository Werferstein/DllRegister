using BaseApp;
using Helper.Ipc;
using SLogging;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DllRegister
{

    public static class Program
    {
        public const string ProgramName = "DllRegister";
        public const string ProgramDescription = "Simple program to register or deregister dotnet DLLs. \r\n\r\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Version ver = RuntimeInformationEx.GetDotnetFrameworkVersion();
            if (ver != null)
            {
                if (ver < new Version(4, 6, 2))
                {
                    MessageBox.Show("NET Framework < 4.6.2 (" + ver.ToString()  + ")", "NET Framework?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                   return;
                }
            }
            else
            {
                MessageBox.Show("NET Framework version ?","NET Framework?", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            Base.OnProgStart += Base_OnProgStart;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            Base.Instance.Option.ForceUAC = false;
#else
            Base.Instance.Option.ForceUAC = true;
#endif
            Base.Instance.Option.ReceiveIpcMessages = false;
            Base.Instance.Start(args);
            
        }

        private static MainForm mainForm = null;
        private static void Base_OnProgStart(object sender, CommandEventArgs e)
        {
            if (mainForm != null || e.EventType != CommandEventEnum.ShowForm) return;
            mainForm = new MainForm();
            Application.Run(mainForm);
        }
    }
}
