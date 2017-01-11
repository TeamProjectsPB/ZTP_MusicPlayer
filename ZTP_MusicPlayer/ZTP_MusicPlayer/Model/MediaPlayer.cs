using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using WMPLib;
using ZTP_MusicPlayer.Model.Iterators;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model
{
    public class MediaPlayer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        #region Singleton

        private static MediaPlayer instance;

        public static MediaPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MediaPlayer();
                }
                return instance;
            }
        }

        #endregion
        #region Members

        private static readonly string[] mediaExtensions =
        {
            ".WAV", ".WMA", ".MP3"
        };

        private List<string> currentPlaylistSongUrl;
        private IWMPPlaylist allLibrariesPlaylist;
        private readonly string allLibrariesPlaylistName;
        private readonly CurrentSongsCollection currentSongsCollection;
        private IAbstractIterator currentSongsIterator;
        private bool sortAsc;
        private string lastSortParam = string.Empty;

        private bool libraryCurrentlyPlaying;
        private bool randomPlay;
        private bool repeatAll;
        private bool isNewSongLoaded;
        private readonly DispatcherTimer songChangeTimer;

        #endregion
        #region Properties

        public WindowsMediaPlayer MPlayer { get; }

        public IWMPPlaylist CurrentPlaylist { get; set; }

        public static Dictionary<string, Song> SongInfo { get; set; }

        public ObservableCollection<IWMPPlaylist> Playlists { get; set; }

        public Dictionary<string, List<string>> PlaylistsUrl { get; set; }

        public ObservableCollection<Library> Libraries { get; set; }

        public bool LibraryCurrentlyPlaying
        {
            get { return libraryCurrentlyPlaying; }
            set
            {
                if (libraryCurrentlyPlaying)
                {
                    MPlayer.playlistCollection.remove(CurrentPlaylist);
                }
                libraryCurrentlyPlaying = value;
            }
        }

        public int CurrentVolume
        {
            get { return MPlayer.settings.volume; }
            set { MPlayer.settings.volume = value; }
        }

        public string CurrentPositionToString
        {
            get
            {
                return string.IsNullOrWhiteSpace(MPlayer.controls.currentPositionString)
                    ? "00:00"
                    : MPlayer.controls.currentPositionString;
            }
        }

        public double CurrentPosition
        {
            get { return MPlayer.controls.currentItem != null ? MPlayer.controls.currentPosition : 0; }
            set
            {
                if (MPlayer.controls.currentItem != null)
                {
                    MPlayer.controls.currentPosition = value;
                }
            }
        }

        public string DurationToString
        {
            get { return MPlayer.controls.currentItem != null ? MPlayer.controls.currentItem.durationString : "00:00"; }
        }

        public double Duration
        {
            get { return MPlayer.controls.currentItem != null ? MPlayer.controls.currentItem.duration : 0; }
        }


        public Song CurrentSong
        {
            get { return currentSongsCollection.CurrentSong; }
            set { currentSongsCollection.CurrentSong = value; }
        }

        public ObservableCollection<Song> CurrentSongs
        {
            get { return currentSongsCollection.CurrentSongs; }
            set { currentSongsCollection.CurrentSongs = value; }
        }

        public string CurrentSongString
        {
            get
            {
                try
                {
                    return Path.GetFileNameWithoutExtension(MPlayer.currentMedia.sourceURL);
                }
                catch
                {
                    return "Aktualna piosenka";
                }
            }
        }

        #endregion 
        #region  Constructors

        public MediaPlayer()
        {
            currentSongsCollection = new CurrentSongsCollection();
            currentSongsIterator = currentSongsCollection.CreateNormalIterator();

            MPlayer = new WindowsMediaPlayer();
            MPlayer.PlayStateChange += MPlayer_PlayStateChange;
            MPlayer.settings.autoStart = true;
            SongInfo = new Dictionary<string, Song>();
            LoadMediaInfo();
            currentPlaylistSongUrl = new List<string>();
            PlaylistsUrl = new Dictionary<string, List<string>>();
            Playlists = new ObservableCollection<IWMPPlaylist>();
            Libraries = new ObservableCollection<Library>();
            allLibrariesPlaylistName = "allLibrariesPlaylist";
            allLibrariesPlaylist = GetPlaylistFromMediaCollection(allLibrariesPlaylistName);
            CurrentSongs = new ObservableCollection<Song>();
            songChangeTimer = new DispatcherTimer();
            songChangeTimer.Interval = TimeSpan.FromMilliseconds(500);
            songChangeTimer.Tick += SongChangeTimer_Tick;
            songChangeTimer.Start();
        }

        #endregion
        #region Player
        private void SongChangeTimer_Tick(object sender, EventArgs e)
        {
            if (isNewSongLoaded)
            {
                Play();
                isNewSongLoaded = false;
            }
        }

        private void MPlayer_PlayStateChange(int NewState)
        {
            if (MPlayer.playState == WMPPlayState.wmppsMediaEnded)
            {
                var changeTrack =
                    !(currentSongsCollection.CurrentSongIndex() == currentSongsCollection.Count - 1 && !repeatAll);
                if (changeTrack)
                {
                    var newTrack = currentSongsIterator.Next();
                    LoadCurrentSong(newTrack);
                    isNewSongLoaded = true;
                }
            }
        }
        #endregion
        #region MediaPlayerControls

        public void PlayPause()
        {
            if (MPlayer.playState.Equals(WMPPlayState.wmppsPlaying))
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Play()
        {
            MPlayer.controls.play();
        }

        public void Pause()
        {
            MPlayer.controls.pause();
        }

        public void Stop()
        {
            MPlayer.controls.stop();
        }

        public bool CanNextTrack()
        {
            return currentSongsIterator.CanNext();
        }

        public void NextTrack()
        {
            var newTrack = currentSongsIterator.Next();
            LoadCurrentSong(newTrack);
        }

        public bool CanPreviousTrack()
        {
            return currentSongsIterator.CanPrevious();
        }

        public void PreviousTrack()
        {
            var newTrack = currentSongsIterator.Previous();
            LoadCurrentSong(newTrack);
        }

        public int VolumeUp()
        {
            if (MPlayer.settings.volume < 100)
            {
                MPlayer.settings.volume = MPlayer.settings.volume + 10;
            }
            return MPlayer.settings.volume;
        }

        public int VolumeDown()
        {
            if (MPlayer.settings.volume > 0)
            {
                MPlayer.settings.volume = MPlayer.settings.volume - 10;
            }
            return MPlayer.settings.volume;
        }

        public bool ChangeRandomPlayStatement()
        {
            randomPlay = !randomPlay;
            if (randomPlay)
            {
                currentSongsIterator = currentSongsCollection.CreateRandomIterator();
            }
            else
            {
                currentSongsIterator = currentSongsCollection.CreateNormalIterator();
            }
            return randomPlay;
        }

        public bool ChangeRepeatAllStatement()
        {
            repeatAll = !repeatAll;
            MPlayer.settings.setMode("loop", repeatAll);
            return repeatAll;
        }

        public void MoveTrack(int songIndex, int newIndex)
        {
            MPlayer.currentPlaylist.moveItem(songIndex, newIndex);
        }

        #endregion
        #region Getters

        public IWMPPlaylist GetPlaylistFromMediaCollection(string playlistName)
        {
            IWMPPlaylist playlist;
            if (MPlayer.mediaCollection.getByName(playlistName).count > 0)
            {
                playlist = MPlayer.mediaCollection.getByName(playlistName);
                playlist.clear();
            }
            else
            {
                playlist = MPlayer.playlistCollection.newPlaylist(playlistName);
            }
            return playlist;
        }

        #endregion
        #region Setters

        public void LoadCurrentPlaylist(string newCurrentPlaylist)
        {
            libraryCurrentlyPlaying = false;
            var temporaryPlaylist = Playlists.SingleOrDefault(x => x.name.Equals(newCurrentPlaylist));
            SetCurrentPlaylistSongUrl(temporaryPlaylist);
            currentPlaylistSongUrl = PlaylistsUrl[newCurrentPlaylist];
            SetCurrentSongs();
            LoadCurrentSong(currentSongsIterator.First());
            CurrentPlaylist = temporaryPlaylist;
        }

        private void SetCurrentPlaylistSongUrl(IWMPPlaylist playlist)
        {
            var urls = new List<string>();
            for (var i = 0; i < playlist.count; i++)
            {
                urls.Add(playlist.Item[i].sourceURL);
            }
            if (PlaylistsUrl.ContainsKey(playlist.name))
            {
                PlaylistsUrl[playlist.name] = urls;
            }
            else
            {
                PlaylistsUrl.Add(playlist.name, urls);
            }
            currentPlaylistSongUrl = PlaylistsUrl[playlist.name];
            SetCurrentSongs();
        }

        private void SetCurrentSongs()
        {
            CurrentSongs.Clear();
            currentPlaylistSongUrl.ForEach(x =>
            {
                var song = SongInfo[x.ToLower()];
                CurrentSongs.Add(song);
            });
        }

        #endregion
        #region Loaders

        public void LoadLibraryMediaPlaylist()
        {
            if (!PlaylistsUrl.ContainsKey(allLibrariesPlaylistName))
            {
                SetCurrentPlaylistSongUrl(allLibrariesPlaylist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[allLibrariesPlaylistName];
            SetCurrentSongs();
            CurrentPlaylist = allLibrariesPlaylist;
        }


        public void LoadCurrentLibrary(string newCurrentLibrary)
        {
            libraryCurrentlyPlaying = true;
            var newCurrentLib = Libraries.SingleOrDefault(x => x.Name.Equals(newCurrentLibrary));
            var playlist = newCurrentLib.Playlist;
            if (!PlaylistsUrl.ContainsKey(playlist.name))
            {
                SetCurrentPlaylistSongUrl(playlist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[playlist.name];
            SetCurrentSongs();
            CurrentPlaylist = playlist;
            LoadCurrentSong(currentSongsIterator.First());
        }

        internal void LoadCurrentSong(Song track)
        {
            if (track != null)
            {
                CurrentSong = track;
                MPlayer.URL = track.Name;
            }
        }
        #endregion
        #region Remove

        public void RemoveTrack(int index)
        {
            var media = MPlayer.currentPlaylist.Item[index];
            var count = CurrentPlaylist.count;
            CurrentPlaylist.removeItem(media);
            var countafter = CurrentPlaylist.count;
            SetCurrentPlaylistSongUrl(CurrentPlaylist);
        }

        public void RemoveTrack(Song song)
        {
            var index = CurrentSongs.IndexOf(song);
            currentSongsCollection.CurrentSongs.Remove(song);
            CurrentPlaylist.removeItem(CurrentPlaylist.Item[index]);
        }

        //return: true - current playlist was removed;
        public bool RemovePlaylist(string name)
        {
            var removedCurrentPlaylist = CurrentPlaylist.name.Equals(name);
            if (removedCurrentPlaylist)
            {
                LoadLibraryMediaPlaylist();
            }
            var playlists = new List<IWMPPlaylist>();
            for (var i = 0; i < MPlayer.playlistCollection.getByName(name).count; i++)
            {
                playlists.Add(MPlayer.playlistCollection.getByName(name).Item(i));
            }
            playlists.ForEach(x => MPlayer.playlistCollection.remove(x));
            Playlists.Remove(Playlists.SingleOrDefault(x => x.name.Equals(name)));
            return removedCurrentPlaylist;
        }

        public bool RemoveLibrary(string name)
        {
            var library = Libraries.SingleOrDefault(x => x.Name.Equals(name));
            var libraryPlaylistName = library.Playlist.name;
            var removedCurrentLibrary = CurrentPlaylist.name.Equals(libraryPlaylistName);

            var removePlaylists = new List<IWMPPlaylist>();
            for (var i = 0; i < MPlayer.playlistCollection.getByName(libraryPlaylistName).count; i++)
            {
                var media = MPlayer.playlistCollection.getByName(libraryPlaylistName).Item(i);
                removePlaylists.Add(media);
            }
            removePlaylists.ForEach(x => MPlayer.playlistCollection.remove(x));
            Playlists.Remove(Playlists.SingleOrDefault(x => x.name.Equals(name)));
            Libraries.Remove(library);
            RemoveLibraryTracksFromAllLibrariesPlaylist(library.Url);
            ConfigFile.RemoveLibrary(name);
            if (removedCurrentLibrary)
            {
                allLibrariesPlaylist = GetPlaylistFromMediaCollection(allLibrariesPlaylistName);
            }
            return removedCurrentLibrary;
        }

        private void RemoveLibraryTracksFromAllLibrariesPlaylist(string libUrl)
        {
            for (var i = allLibrariesPlaylist.count - 1; i >= 0; i--)
            {
                var item = allLibrariesPlaylist.Item[i];
                if (item.sourceURL.ToLower().Contains(libUrl.ToLower()))
                {
                    allLibrariesPlaylist.removeItem(item);
                }
            }
            SetCurrentPlaylistSongUrl(allLibrariesPlaylist);
        }

        #endregion
        #region Library/Playlist

        public void CreateLibrary(string name, string url)
        {
            AddLibrary(name, url);
            ConfigFile.SaveNewLibrary(name, url);
        }

        public void AddLibrary(string name, string url)
        {
            if (Directory.Exists(url))
            {
                LoadMediaInfoFromNewLibrary(url);
                var playlistname = "lib_" + name;
                var playlist = GetPlaylistFromMediaCollection(playlistname);

                var audio = MPlayer.mediaCollection.getByAttribute("MediaType", "audio");
                for (var i = 0; i < audio.count; i++)
                {
                    var media = audio.Item[i];
                    if (media.sourceURL.ToLower().Contains(url.ToLower()))
                    {
                        var count = MPlayer.mediaCollection.getAll().count;
                        playlist.appendItem(media);
                        allLibrariesPlaylist.appendItem(media);
                    }
                }
                Libraries.Add(new Library(name, url, playlist));
            }
        }

        private void LoadMediaInfoFromNewLibrary(string url)
        {
            var songsUrl = new List<string>(Directory.EnumerateFiles(url, "*.*", SearchOption.AllDirectories).
                Where(
                    s => mediaExtensions.Contains(Path.GetExtension(s), StringComparer.OrdinalIgnoreCase)));
            songsUrl.ForEach(x =>
            {
                if (!SongInfo.ContainsKey(x.ToLower()))
                {
                    MPlayer.mediaCollection.add(x.ToLower());
                    var song = Song.Create(x.ToLower());
                    SongInfo.Add(x.ToLower(), song);
                }
            });
        }

        public void AddTrackToPlaylist(int trackIndex, string playlistName)
        {
            var song = CurrentPlaylist.Item[trackIndex];
            var playlists = MPlayer.playlistCollection.getByName(playlistName);
            for (var i = 0; i < playlists.count; i++)
            {
                MPlayer.playlistCollection.getByName(playlistName).Item(i).appendItem(song);
            }
        }

        public void CreatePlaylist(string name)
        {
            Playlists.Add(MPlayer.playlistCollection.newPlaylist(name));
            ConfigFile.SaveNewPlaylist(name);
        }

        public void AddPlaylist(string name)
        {
            if (MPlayer.playlistCollection.getByName(name).count > 0)
            {
                Playlists.Add(MPlayer.playlistCollection.getByName(name).Item(0));
            }
        }

        private void EditSongNullableMetadata(Song song)
        {
            if (string.IsNullOrWhiteSpace(song.Tag.Title))
            {
                var path = Path.GetFileNameWithoutExtension(song.Name);
                song.Tag.Title = path;
                song.Save();
            }
        }

        private void LoadMediaInfo()
        {
            var mediaCollection = MPlayer.mediaCollection.getByAttribute("MediaType", "audio");
            var count = mediaCollection.count;
            for (var i = 0; i < mediaCollection.count; i++)
            {
                var media = mediaCollection.Item[i];
                var url = media.sourceURL;
                var song = Song.Create(media.sourceURL);
                EditSongNullableMetadata(song);
                SongInfo.Add(media.sourceURL.ToLower(), song);
            }
        }

        #endregion
        #region SortPlaylist

        private List<Song> CreateSortedList(string param)
        {
            List<Song> sortableList = null;
            if (lastSortParam.Equals(param))
            {
                sortAsc = !sortAsc;
            }
            else
            {
                sortAsc = true;
            }
            if (param.Equals("Tag.FirstPerformer"))
            {
                sortableList = sortAsc
                    ? new List<Song>(CurrentSongs.OrderBy(i => i.Tag.FirstPerformer))
                    : new List<Song>(CurrentSongs.OrderByDescending(i => i.Tag.FirstPerformer));
            }
            else if (param.Equals("Tag.Title"))
            {
                sortableList = sortAsc
                    ? new List<Song>(CurrentSongs.OrderBy(i => i.Tag.Title))
                    : new List<Song>(CurrentSongs.OrderByDescending(i => i.Tag.Title));
            }
            else if (param.Equals("Tag.Album"))
            {
                sortableList = sortAsc
                    ? new List<Song>(CurrentSongs.OrderBy(i => i.Tag.Album))
                    : new List<Song>(CurrentSongs.OrderByDescending(i => i.Tag.Album));
            }
            lastSortParam = param;
            return sortableList;
        }

        public void Sort(string property)
        {
            var sortableList = CreateSortedList(property);

            for (var i = 0; i < sortableList.Count; i++)
            {
                var index = CurrentSongs.IndexOf(sortableList[i]);
                CurrentSongs.Move(index, i);
                CurrentPlaylist.moveItem(index, i);
            }
        }

        #endregion        
        #region InotifyPropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateSourceTrigger(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}