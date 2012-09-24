using System;
using System.Windows.Forms;


namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester

{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Primary());
        }
    }
}
