using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PMSAppWPF.Commands;
using Microsoft.Win32;
using PMSAppWPF.BLL;
using System.Threading;
using System.Windows;

namespace PMSAppWPF.ViewModel
{
    public class ProcessViewModel : INotifyPropertyChanged
    {
        public ICommand RefreshList { get; set; }
        public ICommand Search { get; set; }
        public ICommand Select { get; set; }
        public ICommand StartWriting { get; set; }
        public ICommand StopWriting { get; set; }
        public ICommand BrowseFile { get; set; }
        private bool isDone = false;
        private string searchTerm;
        private double progress;
        private int samplingRate;
        private string path;
        private string fileFormat = ".xlsx";
        Dictionary<DateTime, List<Process>> sampledData = null;
        ProcessWriteManager manager = new ProcessWriteManager();
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Process> allProcesses = new ObservableCollection<Process>();
        private ObservableCollection<Process> selectedProcesses = new ObservableCollection<Process>();
        private ObservableCollection<bool> selectorList = new ObservableCollection<bool>();
        private bool isWriting = false;

        public ObservableCollection<bool> SelectorList
        {
            get { return selectorList; }
            set { { selectorList = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectorList))); } }
        }

        public ObservableCollection<Process> AllProcesses
        {
            get { return allProcesses; }
            set { { allProcesses = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllProcesses))); } }
        }

        public ObservableCollection<Process> SelectedProcesses
        {
            get { return selectedProcesses; }
            set { { selectedProcesses = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProcesses))); } }
        }

        public int SamplingRate { get => samplingRate; set { samplingRate = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SamplingRate))); } }

        public string Path { get => path; set { path = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path))); } }

        public string FileFormat { get => fileFormat; set { fileFormat = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileFormat))); } }

        public string SearchTerm { get => searchTerm; set { searchTerm = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchTerm))); } }

        public double Progress { get => progress; set { progress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))); } }

        public ProcessViewModel()
        {
            GetAllProcesses();
            RefreshList = new PMSCommands(refresh, canRefresh);
            Search = new PMSCommands(search, canSearch);
            BrowseFile = new PMSCommands(browse, canBrowse);
            StartWriting = new PMSCommands(Start, CanStart);
            StopWriting = new PMSCommands(Stop, CanStop);
            Select = new PMSCommands(Selected, CanSelect);
        }

        private bool canRefresh(object parameter)
        {
            return true;
        }
        private void refresh(object parameter)
        {
            GetAllProcesses();
            SelectedProcesses.Clear();
        }

        private bool canSearch(object parameter)
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void search(object parameter)
        {
            AllProcesses = new ObservableCollection<Process>(AllProcesses.Where(p =>
            {
                if (p.ProcessName.Contains(SearchTerm) || p.Id.ToString().Contains(SearchTerm)) return true;
                else return false;
            }));
        }

        private bool canBrowse(object parameter)
        {

            return true;
        }
        private void browse(object parameter)
        {
            IViewController view = new ViewController(new SaveFileDialog {DefaultExt= "Excel Worksheets|*.xls", Filter= "Excel Worksheets|*.xls*" });
            if (view.Open())
                Path = view.getPath();
        }

        private bool CanStart(object parameter)
        {
            if (string.IsNullOrEmpty(Path) || SamplingRate == 0 || SelectedProcesses.Count == 0 || isWriting)
                return false;
            else return true;
        }
        private void Start(object parameter)
        {
            manager.WritetoFile(SelectedProcesses.ToList(), SamplingRate, Path);
            isWriting = true;
            new Task(() =>{
                while(!isDone) {
                Progress++;
                Thread.Sleep(SamplingRate * 1000);
                }
                isDone = false;
            }).Start(); ;
            
        }

        private bool CanStop(object parameter)
        {
            if (isWriting)
                return true;
            else
                return false;
        }
        private void Stop(object parameter)
        {
            manager.Stop();
            sampledData = manager.GetSampledData();
            if(sampledData != null)
            {
                GraphWindow window = new GraphWindow(sampledData, SelectedProcesses.ToList());
                window.Show();
            }
            CleanUp();
            MessageBox.Show("Completed Writing Successfully");
        }

        private void CleanUp()
        {
            SelectedProcesses.Clear();
            Path = string.Empty;
            SamplingRate = 0;
            isDone = true;
            Progress = 0.0;
            isWriting = false;
        }

        private bool CanSelect(object parameter)
        {
            return true;
        }
        private void Selected(object parameter)
        {
            Process p = parameter as Process;
            if (p != null && SelectedProcesses.Contains(p))
                SelectedProcesses.Remove(p);
            else
                SelectedProcesses.Add(p);
        }

        private void GetAllProcesses()
        {
            AllProcesses.Clear();
            foreach (var p in Process.GetProcesses())
            {
                AllProcesses.Add(p);
            }
        }
    }
}
