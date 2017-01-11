using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WMPLib;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;
using ZTP_MusicPlayer.View;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MediaPlayer = ZTP_MusicPlayer.Model.MediaPlayer;
using MenuItem = System.Windows.Controls.MenuItem;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Members
        private static DispatcherTimer timer;
        private MediaPlayer player = MediaPlayer.Instance;     
        private bool repeatAll;
        private bool randomPlay;
        #endregion

        #region Properties

        public bool RepeatAll
        {
            get { return repeatAll; }
            set
            {
                repeatAll = value;
                OnPropertyChanged("RepeatAll");
                OnPropertyChanged("RepeatAllButtonBackground");
            }
        }

        public bool RandomPlay
        {
            get { return randomPlay; }
            set
            {
                randomPlay = value;
                OnPropertyChanged("RandomPlay");
                OnPropertyChanged("ShuffleButtonBackground");
            }
        }

        public Brush ShuffleButtonBackground
        {
            get { return randomPlay ? Brushes.Yellow : Brushes.White; }
        }

        public Brush RepeatAllButtonBackground
        {
            get { return repeatAll ? Brushes.Yellow : Brushes.White; }
        }
        #region CurrentPosition
        public double CurrentPosition
        {
            get { return player.CurrentPosition; }
            set { player.CurrentPosition = value; }
        }
        public string CurrentPositionToString
        {
            get { return player.CurrentPositionToString; }
        }
        #endregion

        #region Duration
        public double Duration
        {
            get { return player.Duration; }
        }

        public string DurationToString
        {
            get { return player.DurationToString; }
        }
        #endregion

        public ObservableCollection<Song> CurrentSongs
        {
            get { return player.CurrentSongs; }
        }

        public string CurrentSongString
        {
            get { return player.CurrentSongString; }
        }

        public ObservableCollection<Library> Libraries
        {
            get { return player.Libraries; }
        }

        public ObservableCollection<IWMPPlaylist> Playlists
        {
            get { return player.Playlists; }
        }

        public int CurrentVolume
        {
            get { return player.CurrentVolume; }
            set
            {
                player.CurrentVolume = value;
                ConfigFile.SaveVolume(value);
            }
        }
        #endregion
        public MainWindowViewModel()
        {
            InitializeCommands();
            if (ConfigFile.ConfigFileExists())
            { 
                LoadCurrentVolume();
                LoadLibraries();
                LoadPlaylists();
                LoadLibraryMediaPlaylist();
            }
            else
            {
                ConfigFile.CreateNewFile();
            }
            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        #region Timer
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("CurrentPositionToString");
            OnPropertyChanged("DurationToString");
            OnPropertyChanged("Duration");
            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentSongString");
        }
        #endregion
        #region Commands
        #region PrivateMembers

        private ICommand _runLibraryWindowCommand;
        private ICommand _playPlaylistCommand, _playLibraryCommand, _playSongCommand;
        private ICommand _removeLibraryCommand, _removePlaylistCommand;
        private ICommand addTrackToPlaylist, removeTrackFromPlaylist;
        private ICommand addNewPlaylist;
        private ICommand play, stop, previous, next;
        private ICommand _repeatAllCommand, _randomPlayCommand;
        private ICommand _sortCommand;

        #endregion
        #region Properties

        public ICommand RemoveLibraryCommand
        {
            get { return _removeLibraryCommand; }
            set { _removeLibraryCommand = value; }
        }

        public ICommand RemovePlaylistCommand
        {
            get { return _removePlaylistCommand; }
            set { _removePlaylistCommand = value; }
        }

        public ICommand AddTrackToPlaylistCommand
        {
            get { return addTrackToPlaylist; }
            set { addTrackToPlaylist = value; }
        }

        public ICommand RemoveTrackFromPlaylist
        {
            get { return removeTrackFromPlaylist; }
            set { removeTrackFromPlaylist = value; }
        }

        public ICommand AddNewPlaylist
        {
            get { return addNewPlaylist; }
            set { addNewPlaylist = value; }
        }

        public ICommand PlayCommand
        {
            get { return play; }
            set { play = value; }
        }

        public ICommand StopCommand
        {
            get { return stop; }
            set { stop = value; }
        }

        public ICommand PreviousCommand
        {
            get { return previous; }
            set { previous = value; }
        }

        public ICommand NextCommand
        {
            get { return next; }
            set { next = value; }
        }

        public ICommand RunLibraryWindowCommand
        {
            get { return _runLibraryWindowCommand; }
            set { _runLibraryWindowCommand = value; }
        }

        public ICommand PlayPlaylistCommand
        {
            get { return _playPlaylistCommand; }
            set { _playPlaylistCommand = value; }
        }

        public ICommand PlayLibraryCommand
        {
            get { return _playLibraryCommand; }
            set { _playLibraryCommand = value; }
        }

        public ICommand PlaySongCommand
        {
            get { return _playSongCommand; }
            set { _playSongCommand = value; }
        }

        public ICommand RepeatAllCommand
        {
            get { return _repeatAllCommand; }
            set { _repeatAllCommand = value; }
        }

        public ICommand RandomPlayCommand
        {
            get { return _randomPlayCommand; }
            set { _randomPlayCommand = value; }
        }

        public ICommand SortCommand
        {
            get { return _sortCommand; }
            set { _sortCommand = value; }
        }

        #endregion

        private void InitializeCommands()
        {

            _removeLibraryCommand = new RelayCommand(RemoveLibraryExecute, param => true);
            _removePlaylistCommand = new RelayCommand(RemovePlaylistExecute);
            _playLibraryCommand = new RelayCommand(PlayLibraryExecute);
            _playPlaylistCommand = new RelayCommand(PlayPlaylistExecute);
            _playSongCommand = new RelayCommand(PlaySongExecute, PlaySongCanExecute);
            addNewPlaylist = new RelayCommand(AddNewPlaylistExecute, param => true);
            addTrackToPlaylist = new RelayCommand(AddTrackToPlaylistExecute, CanAddTrackToPlaylistExecute);
            removeTrackFromPlaylist = new RelayCommand(RemoveTrackFromPlaylistExecute, CanRemoveTrackFromPlaylist);
            play = new RelayCommand(PlayExecute, param => true);
            stop = new RelayCommand(StopExecute, param => true);
            previous = new RelayCommand(PreviousExecute, CanPreviousExecute);
            next = new RelayCommand(NextExecute, CanNextExecute);
            _repeatAllCommand = new RelayCommand(RepeatAllExecute);
            _randomPlayCommand = new RelayCommand(RandomPlayExecute);
            _runLibraryWindowCommand = new RelayCommand(RunLibraryWindowExecute, RunLibraryWindowCanExecute);
            _sortCommand = new RelayCommand(SortExecute);
        }

       

        private bool CanNextExecute(object obj)
        {
            return player.CanNextTrack();
        }

        private bool CanPreviousExecute(object o)
        {
            return player.CanPreviousTrack();
        }

        private void SortExecute(object o)
        {
            player.Sort((string)o);
        }

        private void RandomPlayExecute(object o)
        {
            RandomPlay = player.ChangeRandomPlayStatement();
        }

        private void RepeatAllExecute(object o)
        {
            RepeatAll = player.ChangeRepeatAllStatement();
        }

        private bool PlaySongCanExecute(object o)
        {
            return o != null;
        }

        private void PlayPlaylistExecute(object o)
        {
            var name = ((IWMPPlaylist) o).name;
            player.LoadCurrentPlaylist(name);
        }

        private void PlayLibraryExecute(object o)
        {
            var name = ((Library) o).Name;
            player.LoadCurrentLibrary(name);
        }

        private void PlaySongExecute(object o)
        {
            var track = o as Song;
//            var index = CurrentSongs.IndexOf(track);
//            player.LoadCurrentSong(index);
            player.LoadCurrentSong(track);
        }


        private bool RunLibraryWindowCanExecute(object o)
        {
            return true;
        }

        private void RunLibraryWindowExecute(object o)
        {
            AddLibraryWindow addLibraryWindow = new AddLibraryWindow();
            addLibraryWindow.ShowDialog();
        }

        private void AddNewPlaylistExecute(object o)
        {
            AddNewPlaylistWindow dialog = new AddNewPlaylistWindow();
            dialog.ShowDialog();            
        }

        #region Execute
        private void RemoveLibraryExecute(object obj)
        {
            player.RemoveLibrary((obj as Library).Name);

        }
        private void RemovePlaylistExecute(object o)
        {
            player.RemovePlaylist((o as IWMPPlaylist).name);
        }

        
        private bool CanRemoveTrackFromPlaylist(object o)
        {
            return o != null;
        }
        private void RemoveTrackFromPlaylistExecute(object obj)
        {
            player.RemoveTrack(obj as Song);
        }

        private bool CanAddTrackToPlaylistExecute(object obj)
        {
            return obj != null;
        }
        private void AddTrackToPlaylistExecute(object obj)
        {
            AddTrackToPlaylistWindow dialog = new AddTrackToPlaylistWindow();
            if (dialog.ShowDialog() == true)
            {
                var viewmodel = (AddTrackToPlaylistViewModel) dialog.DataContext;
                var index = CurrentSongs.IndexOf(obj as Song);
                var playlist = viewmodel.SelectedPlaylist;                
                player.AddTrackToPlaylist(index, playlist);
            }
        }
        private void NextExecute(object obj)
        {
            player.NextTrack();
        }

        private void PreviousExecute(object obj)
        {
            player.PreviousTrack();
        }

        private void StopExecute(object obj)
        {
            player.Stop();
        }

        private void PlayExecute(object obj)
        {
            player.PlayPause();
        }
        #endregion
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion
        #region Add

        public void AddLibrary(string name, string url)
        {
            player.AddLibrary(name, url);
        }

        public void AddPlaylist(string name)
        {
            player.AddPlaylist(name);
        }
        #endregion
        #region Create

        public void CreatePlaylist(string name)
        {
            player.CreatePlaylist(name);
        }

        public void CreateLibrary(string name, string url)
        {
            player.CreateLibrary(name, url);
//            CreateTitleToFilesWithoutMetaData();
        }


        #endregion
        #region FileLoaders
        public void LoadCurrentVolume()
        {
            player.CurrentVolume = ConfigFile.GetVolume();
        }

        private void LoadLibraries()
        {
            var libraries = ConfigFile.GetLibraries();
            libraries.ForEach(x => AddLibrary(x.Item1, x.Item2));
        }

        private void LoadPlaylists()
        {
            var playlists = ConfigFile.GetPlaylists();
            playlists.ForEach(x => AddPlaylist(x));
        }

        public void LoadLibraryMediaPlaylist()
        {
            player.LoadLibraryMediaPlaylist();
        }
        #endregion
    }
}
