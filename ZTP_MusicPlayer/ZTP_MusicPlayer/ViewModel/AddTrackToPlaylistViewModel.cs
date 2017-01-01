using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WMPLib;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;

namespace ZTP_MusicPlayer.ViewModel
{
    class AddTrackToPlaylistViewModel : INotifyPropertyChanged
    {
        private bool? dialogResult;

        public bool? DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        public IWMPPlaylist SelectedPlaylist { get; set; }
        public ObservableCollection<IWMPPlaylist> Playlists
        {
            get { return MediaPlayer.Instance.Playlists; }
        }

        private ICommand cancel, addToPlaylist;

        public ICommand Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new RelayCommand(CancelExecute, CancelCanExecute);
                }
                return cancel;
            }
            set { cancel = value; }
        }


        public ICommand AddToPlaylist
        {
            get
            {
                if (addToPlaylist == null)
                {
                    addToPlaylist = new RelayCommand(AddToPlaylistExecute, AddToPlaylistCanExecute);
                }
                return addToPlaylist;
            }
            set { addToPlaylist = value; }
        }

        private bool AddToPlaylistCanExecute(object o)
        {
            return SelectedPlaylist != null;
        }

        private void AddToPlaylistExecute(object o)
        {            
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
