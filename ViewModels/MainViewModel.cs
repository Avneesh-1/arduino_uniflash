using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UniFlash.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string selectedComPort;
        private int selectedBaudRate;
        private bool isConnected;
        private bool isPaused;
        private bool isStopped;
        private string terminalText;

        public ObservableCollection<string> ComPorts { get; } = new ObservableCollection<string>();
        public ObservableCollection<int> BaudRates { get; } = new ObservableCollection<int>();

        public string SelectedComPort
        {
            get => selectedComPort;
            set { selectedComPort = value; OnPropertyChanged(nameof(SelectedComPort)); }
        }
        public int SelectedBaudRate
        {
            get => selectedBaudRate;
            set { selectedBaudRate = value; OnPropertyChanged(nameof(SelectedBaudRate)); }
        }
        public bool IsConnected
        {
            get => isConnected;
            set { isConnected = value; OnPropertyChanged(nameof(IsConnected)); }
        }
        public bool IsPaused
        {
            get => isPaused;
            set { isPaused = value; OnPropertyChanged(nameof(IsPaused)); }
        }
        public bool IsStopped
        {
            get => isStopped;
            set { isStopped = value; OnPropertyChanged(nameof(IsStopped)); }
        }
        public string TerminalText
        {
            get => terminalText;
            set { terminalText = value; OnPropertyChanged(nameof(TerminalText)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 