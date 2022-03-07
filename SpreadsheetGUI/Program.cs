using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SpreadsheetGUI;


namespace SS
{
    class SpreadsheetContext : ApplicationContext
    {
       
        private int formCount = 0;

       
        private static SpreadsheetContext ssContext;

        
        private SpreadsheetContext()
        {
        }

       
        public static SpreadsheetContext getAppContext()
        {
            if (ssContext == null)
            {
                ssContext = new SpreadsheetContext();
            }
            return ssContext;
        }

        
        public void RunForm(Form form)
        {
           
            formCount++;

          
            form.FormClosed += (o, e) => { if (--formCount <= 0) ExitThread(); };

            
            form.Show();
        }

    }


    static class Program
    {
       
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            
            SpreadsheetContext appContext = SpreadsheetContext.getAppContext();
            appContext.RunForm(new SpreadsheetForm());
            Application.Run(appContext);
        }
    }
}
