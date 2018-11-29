using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
namespace PMSAppWPF.BLL
{
    public class ProcessWriteManager
    {
        IXLWorkbook workbook = null;
        IXLWorksheet ws = null;
        bool isDone = false;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token;
        Dictionary<DateTime, List<Process>> sampledData = new Dictionary<DateTime, List<Process>>();
        public ProcessWriteManager()
        {
            token = tokenSource.Token;
            
            //MessageBox.Show("got token");
        }
        public void WritetoFile(List<Process> processes, int sampling, string path)
        {
            workbook = new XLWorkbook();
            ws = workbook.AddWorksheet("Process Info");
            int row = 2;
            SetHeaders(ws);

            Task t1 = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    DateTime time = DateTime.Now;
                    ws.Cell("A" + row.ToString()).Value = time.ToLocalTime();
                    sampledData.Add(time, new List<Process>());
                    row++;

                    if (
                    Parallel.For(0, processes.Count, (a) =>
                    {
                        Process item = getProcessInfo(processes.ElementAt(a));
                        if (item != null)
                        {
                            sampledData[time].Add(item);
                            ws.Cell("A" + (row + a).ToString()).Value = item.Id;
                            ws.Cell("B" + (row + a).ToString()).Value = item.PrivateMemorySize64;
                            ws.Cell("C" + (row + a).ToString()).Value = item.VirtualMemorySize64;
                            ws.Cell("D" + (row + a).ToString()).Value = item.WorkingSet64;
                            ws.Cell("E" + (row + a).ToString()).Value = item.PrivateMemorySize64;
                            ws.Cell("F" + (row + a).ToString()).Value = item.PagedMemorySize64;
                            ws.Cell("G" + (row + a).ToString()).Value = item.PagedSystemMemorySize64;
                            ws.Cell("H" + (row + a).ToString()).Value = item.NonpagedSystemMemorySize64;
                        }
                        
                    }).IsCompleted)
                        row += processes.Count;

                    Thread.Sleep(sampling * 1000);
                }
                if (token.IsCancellationRequested)
                {
                    workbook.SaveAs(path);
                    isDone = true;
                }
            }
            );
        }

        private Process getProcessInfo(Process item)
        {
            try
            {
                return Process.GetProcessById(item.Id);
            }
            catch 
            {
                return null;
            }
            
        }

        private void SetHeaders(IXLWorksheet ws)
        {

            ws.Cell("A1").Value = "Process ID";
            ws.Cell("B1").Value = "Private Memory";
            ws.Cell("C1").Value = "Virtual Memory";
            ws.Cell("D1").Value = "Working Set";
            ws.Cell("E1").Value = "Working Set - Private";
            ws.Cell("F1").Value = "Paged Memory";
            ws.Cell("G1").Value = "Paged System Memory";
            ws.Cell("H1").Value = "Non-Paged System Memory";
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public Dictionary<DateTime, List<Process>> GetSampledData()
        {
            while (true)
            {
            if (isDone)
            {
                return sampledData;
            }
            else
            {
                Thread.Sleep(100);
            }
            }
        }
    }
}
