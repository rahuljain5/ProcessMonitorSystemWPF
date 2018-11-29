using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSAppWPF
{
    public class ViewController : IViewController
    {
        FileDialog dialog;
        public ViewController(FileDialog dialog)
        {
            this.dialog = dialog;
            this.dialog.DefaultExt = "Excel Worksheets|*.xls";
        }

        public string getPath()
        {
            return dialog.FileName;
        }

        public bool Open()
        {
            if (dialog.ShowDialog() == true)
                return true;
            else return false;
        }

    }
}
