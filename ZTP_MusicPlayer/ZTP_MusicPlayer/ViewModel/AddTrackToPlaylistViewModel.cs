using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;

namespace ZTP_MusicPlayer.ViewModel
{
    internal class AddTrackToPlaylistViewModel : INotifyPropertyChanged
    {
        #region Members
        private ICommand cancel, addToPlaylist;
        private bool? dialogResult;
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

        public string SelectedPlaylist { get; set; }

        public List<string> Playlists
        {
            get { return MediaPlayer.Instance.Playlists.Select(x => x.name).ToList(); }
        }

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
        #endregion
        #region Commands(Can)Execute
        private bool AddToPlaylistCanExecute(object o)
        {
            return o != null;
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
        #endregion
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}