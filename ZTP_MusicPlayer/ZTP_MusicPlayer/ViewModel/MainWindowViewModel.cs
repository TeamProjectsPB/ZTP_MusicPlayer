using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WMPLib;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;
using ZTP_MusicPlayer.View;
using MediaPlayer = ZTP_MusicPlayer.Model.MediaPlayer;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constructor
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
        #endregion
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
        #region Members

        private static DispatcherTimer timer;
        private readonly MediaPlayer player = MediaPlayer.Instance;
        private bool repeatAll;
        private bool randomPlay;

        #endregion
        #region Properties
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
        #region Commands       
        public ICommand RemoveLibraryCommand { get; set; }
        public ICommand RemovePlaylistCommand { get; set; }
        public ICommand AddTrackToPlaylistCommand { get; set; }
        public ICommand RemoveTrackFromPlaylist { get; set; }
        public ICommand AddNewPlaylist { get; set; }
        public ICommand PlayCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand RunLibraryWindowCommand { get; set; }
        public ICommand PlayPlaylistCommand { get; set; }
        public ICommand PlayLibraryCommand { get; set; }
        public ICommand PlaySongCommand { get; set; }
        public ICommand RepeatAllCommand { get; set; }
        public ICommand RandomPlayCommand { get; set; }
        public ICommand SortCommand { get; set; }

        private void InitializeCommands()
        {
            RemoveLibraryCommand = new RelayCommand(RemoveLibraryExecute);
            RemovePlaylistCommand = new RelayCommand(RemovePlaylistExecute);
            RemoveTrackFromPlaylist = new RelayCommand(RemoveTrackFromPlaylistExecute, RemoveTrackFromPlaylistCanExecute);
            PlayLibraryCommand = new RelayCommand(PlayLibraryExecute);
            PlayPlaylistCommand = new RelayCommand(PlayPlaylistExecute);
            PlaySongCommand = new RelayCommand(PlaySongExecute, PlaySongCanExecute);
            AddNewPlaylist = new RelayCommand(AddNewPlaylistExecute);
            AddTrackToPlaylistCommand = new RelayCommand(AddTrackToPlaylistExecute, AddTrackToPlaylistCanExecute);
            PlayCommand = new RelayCommand(PlayExecute);
            StopCommand = new RelayCommand(StopExecute);
            PreviousCommand = new RelayCommand(PreviousExecute, PreviousCanExecute);
            NextCommand = new RelayCommand(NextExecute, NextCanExecute);
            RepeatAllCommand = new RelayCommand(RepeatAllExecute);
            RandomPlayCommand = new RelayCommand(RandomPlayExecute);
            RunLibraryWindowCommand = new RelayCommand(RunLibraryWindowExecute);
            SortCommand = new RelayCommand(SortExecute);
        }


        private void RemoveLibraryExecute(object obj)
        {
            player.RemoveLibrary((obj as Library).Name);
        }
        private void RemovePlaylistExecute(object o)
        {
            player.RemovePlaylist((o as IWMPPlaylist).name);
        }
        private bool RemoveTrackFromPlaylistCanExecute(object o)
        {
            return o != null;
        }
        private void RemoveTrackFromPlaylistExecute(object obj)
        {
            player.RemoveTrack(obj as Song);
        }
        private void PlayLibraryExecute(object o)
        {
            var name = ((Library)o).Name;
            player.LoadCurrentLibrary(name);
        }
        private void PlayPlaylistExecute(object o)
        {
            var name = ((IWMPPlaylist)o).name;
            player.LoadCurrentPlaylist(name);
        }
        private bool PlaySongCanExecute(object o)
        {
            return o != null;
        }
        private void PlaySongExecute(object o)
        {
            var track = o as Song;
            player.LoadCurrentSong(track);
        }
        private void AddNewPlaylistExecute(object o)
        {
            var dialog = new AddNewPlaylistWindow();
            dialog.ShowDialog();
        }
        private bool AddTrackToPlaylistCanExecute(object obj)
        {
            return obj != null;
        }

        private void AddTrackToPlaylistExecute(object obj)
        {
            var dialog = new AddTrackToPlaylistWindow();
            if (dialog.ShowDialog() == true)
            {
                var viewmodel = (AddTrackToPlaylistViewModel)dialog.DataContext;
                var index = CurrentSongs.IndexOf(obj as Song);
                var playlist = viewmodel.SelectedPlaylist;
                player.AddTrackToPlaylist(index, playlist);
            }
        }


        private bool NextCanExecute(object obj)
        {
            return player.CanNextTrack();
        }

        private bool PreviousCanExecute(object o)
        {
            return player.CanPreviousTrack();
        }
        private void PlayExecute(object obj)
        {
            player.PlayPause();
        }
        private void StopExecute(object obj)
        {
            player.Stop();
        }
        private void PreviousExecute(object obj)
        {
            player.PreviousTrack();
        }
        private void NextExecute(object obj)
        {
            player.NextTrack();
        }
        private void RepeatAllExecute(object o)
        {
            RepeatAll = player.ChangeRepeatAllStatement();
        }
        private void RandomPlayExecute(object o)
        {
            RandomPlay = player.ChangeRandomPlayStatement();
        }
        private void RunLibraryWindowExecute(object o)
        {
            var addLibraryWindow = new AddLibraryWindow();
            addLibraryWindow.ShowDialog();
        }
        private void SortExecute(object o)
        {
            player.Sort((string)o);
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
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

    }
}