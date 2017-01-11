using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using FolderPickerLib;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;

namespace ZTP_MusicPlayer.ViewModel
{
    internal class CreateNewLibraryViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Members
        private string _libName;
        private ICommand accept, cancel;

        private bool? dialogResult;
        private TreeItem selectedUrl;
        #endregion
        #region Properties
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                OnPropertyChanged("DialogResult");
            }
        }


        public string LibName
        {
            get { return _libName; }
            set
            {
                _libName = value;
                OnPropertyChanged("LibName");
            }
        }

        public string SourceUrl
        {
            get { return selectedUrl.GetFullPath(); }
        }

        public TreeItem SelectedUrl
        {
            get { return selectedUrl; }
            set
            {
                selectedUrl = value;
                OnPropertyChanged("SelectedUrl");
            }
        }

        public ICommand Accept
        {
            get
            {
                if (accept == null)
                {
                    accept = new RelayCommand(AcceptExecute, AcceptCanExecute);
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

        #endregion
        #region Command(Can)Execute
        private void CancelExecute(object o)
        {
            DialogResult = false;
        }


        private bool AcceptCanExecute(object o)
        {
            return !string.IsNullOrWhiteSpace(LibName) &&
                   !string.IsNullOrWhiteSpace(((FolderPickerControl) o).SelectedPath);
        }

        private void AcceptExecute(object o)
        {
            MediaPlayer.Instance.AddLibrary(LibName, ((FolderPickerControl) o).SelectedPath);

            DialogResult = true;
        }
        #endregion
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
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
                        if (!Regex.IsMatch(LibName, "^[a-zA-Z0-9 _]*$"))
                        {
                            return "Nazwa może zawierać wyłącznie litery, cyfry, spację oraz twardą spację.";
                        }
                        break;
                    case "SourceUrl":
                        if (string.IsNullOrWhiteSpace(SourceUrl))
                        {
                            return "Wybierz ścieżkę.";
                        }
                        if (!Directory.Exists(SourceUrl))
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