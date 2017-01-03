using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;

namespace ZTP_MusicPlayer.ViewModel
{
    class AddNewPlaylistViewModel : INotifyPropertyChanged
    {
        private ICommand okCommand, cancelCommand;
        private string playlistName;
        private bool? dialogResult;

        public string PlaylistName
        {
            get { return playlistName; }
            set { playlistName = value; OnPropertyChanged("PlaylistName");}
        }
        public ICommand OkCommand
        {
            get
            {
                if (okCommand == null)
                {
                    okCommand = new RelayCommand(OkExecute, OkCanExecute);
                }
                return okCommand;
            }
            set { okCommand = value; }
        }


        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new RelayCommand(CancelExecute, CancelCanExecute);
                }
                return cancelCommand;
            }
            set { cancelCommand = value; }
        }

        public bool? DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        private bool OkCanExecute(object o)
        {
            return !String.IsNullOrWhiteSpace(playlistName);
        }

        private void OkExecute(object o)
        {
            MediaPlayer.Instance.CreatePlaylist(playlistName);            
            DialogResult = true;
        }

        private bool CancelCanExecute(object o)
        {
            return true;
        }

        private void CancelExecute(object o)
        {
            DialogResult = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
