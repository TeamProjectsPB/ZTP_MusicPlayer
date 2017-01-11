using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderPickerLib;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;

namespace ZTP_MusicPlayer.ViewModel
{
    class CreateNewLibraryViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _libName;
        private ICommand accept, cancel;
        private TreeItem selectedUrl;

        private bool? dialogResult;

        public bool? DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; OnPropertyChanged("DialogResult"); }
        }


        public string LibName
        {
            get { return _libName; }
            set { _libName = value; OnPropertyChanged("LibName"); }
        }

        public string SourceUrl { get { return selectedUrl.GetFullPath(); } }

        public TreeItem SelectedUrl
        {
            get { return selectedUrl; }
            set { selectedUrl = value; OnPropertyChanged("SelectedUrl"); }
        }

        public ICommand Accept
        {
            get
            {
                if (accept == null)
                {
                    accept = new RelayCommand(AcceptExecute,AcceptCanExecute);
                }
                return accept;
            }
            set { accept = value; }
        }

        

        public ICommand Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new RelayCommand(CancelExecute);
                }
                return cancel;
            }
            set { cancel = value; }
        }

        private void CancelExecute(object o)
        {
            DialogResult = false;
        }


        private bool AcceptCanExecute(object o)
        {
            return !String.IsNullOrWhiteSpace(LibName) && !String.IsNullOrWhiteSpace(((FolderPickerControl)o).SelectedPath);
        }

        private void AcceptExecute(object o)
        {
            MediaPlayer.Instance.AddLibrary(LibName, ((FolderPickerControl)o).SelectedPath);
            
            DialogResult = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IDataErrorInfo
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "LibName":
                        if (string.IsNullOrWhiteSpace(LibName))
                        {
                            return "Wprowadz nazwę.";
                        }
                        else if (!Regex.IsMatch(LibName, "^[a-zA-Z0-9 _]*$"))
                        {
                            return "Nazwa może zawierać wyłącznie litery, cyfry, spację oraz twardą spację.";
                        }                
                break;
                    case "SourceUrl":
                        if (string.IsNullOrWhiteSpace(SourceUrl))
                        {
                            return "Wybierz ścieżkę.";
                        }
                        else if (!System.IO.Directory.Exists(SourceUrl))
                        {
                            return "Wybrany folder nie istnieje.";
                        }
                        break;
                }
                return string.Empty;
            }
        }

        public string Error { get; }
        #endregion
    }
}
