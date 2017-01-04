using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using WMPLib;
using ZTP_MusicPlayer.Model;
using ZTP_MusicPlayer.Model.Iterators;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model
{
    public class MediaPlayer : INotifyPropertyChanged
    {
        #region Singleton

        private static MediaPlayer instance = null;

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

        private readonly string fileUrl = Directory.GetCurrentDirectory() + "\\config.dat";
        private WindowsMediaPlayer mPlayer;
        private IWMPPlaylist _currentPlaylist;
        private List<string> currentPlaylistSongUrl;
        private IWMPPlaylist allLibrariesPlaylist;
        private string allLibrariesPlaylistName;
        //private ObservableCollection<Song> CurrentSongs;
        private CurrentSongsCollection currentSongsCollection;
        private IAbstractIterator currentSongsIterator;
        private bool sortAsc;
        private string lastSortParam = string.Empty;

        private bool libraryCurrentlyPlaying;
        private bool randomPlay;
        private bool repeatAll;

        #endregion

        #region Properties

        public string FileUrl
        {
            get { return fileUrl; }
        }

        public WindowsMediaPlayer MPlayer
        {
            get { return mPlayer; }
        }

        public IWMPPlaylist CurrentPlaylist
        {
//            get { return mPlayer.currentPlaylist; }
//            set { mPlayer.currentPlaylist = value; }
            get { return _currentPlaylist; }
            set { _currentPlaylist = value; }
        }

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
                    mPlayer.playlistCollection.remove(CurrentPlaylist);
                }
                libraryCurrentlyPlaying = value;
            }
        }

        public int CurrentVolume
        {
            get { return mPlayer.settings.volume; }
            set { mPlayer.settings.volume = value; }
        }

        public string CurrentPositionToString
        {
            get
            {
                return String.IsNullOrWhiteSpace(mPlayer.controls.currentPositionString)
                    ? "00:00"
                    : mPlayer.controls.currentPositionString;
            }
        }

        public double CurrentPosition
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentPosition : 0; }
            set
            {
                if (mPlayer.controls.currentItem != null)
                {
                    mPlayer.controls.currentPosition = value;
                }
            }
        }

        public string DurationToString
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentItem.durationString : "00:00"; }           
        }

        public double Duration
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentItem.duration : 0; }
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
                    return Path.GetFileNameWithoutExtension(mPlayer.currentMedia.sourceURL);
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
            
            mPlayer = new WindowsMediaPlayer();           
            mPlayer.PlayStateChange += MPlayer_PlayStateChange;
            mPlayer.settings.autoStart = true;
            SongInfo = new Dictionary<string, Song>();
            LoadMediaInfo();
            currentPlaylistSongUrl = new List<string>();
            PlaylistsUrl = new Dictionary<string, List<string>>();
            Playlists = new ObservableCollection<IWMPPlaylist>();
            Libraries = new ObservableCollection<Library>();
            allLibrariesPlaylistName = "allLibrariesPlaylist";
            allLibrariesPlaylist = GetPlaylistFromMediaCollection(allLibrariesPlaylistName);
            CurrentSongs = new ObservableCollection<Song>();
            //currentSongsCollection = new CurrentSongsCollection();

        }

        private void MPlayer_PlayStateChange(int NewState)
        {
            if (mPlayer.playState == WMPPlayState.wmppsMediaEnded)
            {
                Song newTrack = currentSongsIterator.Next();
                LoadCurrentSong(newTrack);
                //Play();
            }
            if (mPlayer.playState == WMPPlayState.wmppsReady)
            {
                Play();
            }
        }

        #endregion

        #region MediaPlayerControls

        public void PlayPause()
        {
            if (mPlayer.playState.Equals(WMPPlayState.wmppsPlaying))
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
            mPlayer.controls.play();
        }

        public void Pause()
        {
            mPlayer.controls.pause();
        }

        public void Stop()
        {
            mPlayer.controls.stop();
        }

        public bool CanNextTrack()
        {
            return currentSongsIterator.CanNext();
        }
        public void NextTrack()
        {
            Song newTrack = currentSongsIterator.Next();
            LoadCurrentSong(newTrack);
            //mPlayer.controls.next();

        }

        public bool CanPreviousTrack()
        {
            return currentSongsIterator.CanPrevious();
        }

        public void PreviousTrack()
        {
            Song newTrack = currentSongsIterator.Previous();
            LoadCurrentSong(newTrack);
        }

        public int VolumeUp()
        {
            if (MPlayer.settings.volume < 100)
            {
                MPlayer.settings.volume = MPlayer.settings.volume + 10;
            }
            return mPlayer.settings.volume;
        }

        public int VolumeDown()
        {
            if (MPlayer.settings.volume > 0)
            {
                MPlayer.settings.volume = MPlayer.settings.volume - 10;
            }
            return mPlayer.settings.volume;
        }

        public bool ChangeRandomPlayStatement()
        {
            randomPlay = !randomPlay;
            //mPlayer.settings.setMode("shuffle", randomPlay);
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
            mPlayer.settings.setMode("loop", repeatAll);
            return repeatAll;
        }

        public void MoveTrack(int songIndex, int newIndex)
        {
            mPlayer.currentPlaylist.moveItem(songIndex, newIndex);
        }



        #endregion

        #region Getters
        public IWMPPlaylist GetPlaylistFromMediaCollection(string playlistName)
        {
            IWMPPlaylist playlist;
            if (mPlayer.mediaCollection.getByName(playlistName).count > 0)
            {
                playlist = mPlayer.mediaCollection.getByName(playlistName);
                playlist.clear();
            }
            else
            {
                playlist = mPlayer.playlistCollection.newPlaylist(playlistName);
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
            //CurrentSong = CurrentSongs[0];
            LoadCurrentSong(currentSongsIterator.First());
            CurrentPlaylist = temporaryPlaylist;            
        }

        private void SetCurrentPlaylistSongUrl(IWMPPlaylist playlist)
        {
            var urls = new List<string>();
            for (int i = 0; i < playlist.count; i++)
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
            currentPlaylistSongUrl.ForEach((Action<string>)(x =>
            {
                var song = SongInfo[x.ToLower()];
                this.CurrentSongs.Add(song);
            }));
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
            //var newCurrentLib = Libraries.Find(x => x.Name.Equals(newCurrentLibrary));
            var newCurrentLib = Libraries.SingleOrDefault(x => x.Name.Equals(newCurrentLibrary));
            var playlist = newCurrentLib.Playlist;
            if (!PlaylistsUrl.ContainsKey(playlist.name))
            {
                SetCurrentPlaylistSongUrl(playlist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[playlist.name];
            SetCurrentSongs();
            CurrentPlaylist = playlist;
            //LoadCurrentSong(CurrentSongs[0]);
            LoadCurrentSong(currentSongsIterator.First());
        }     

        internal void LoadCurrentSong(Song track)
        {
            if (track != null)
            {
                CurrentSong = track;
                mPlayer.URL = track.Name;
            }
            //currentSongsIterator.SetCurrentIndex(track);
        }

        internal void LoadCurrentSong()
        {
            mPlayer.URL = CurrentSong.Name;
        }

        #endregion

        #region Remove

        public void RemoveTrack(int index)
        {
            var media = mPlayer.currentPlaylist.Item[index];
            var count = CurrentPlaylist.count;
            CurrentPlaylist.removeItem(media);
            var countafter = CurrentPlaylist.count;
            SetCurrentPlaylistSongUrl(CurrentPlaylist);
        }

        //return: true - current playlist was removed;
        public bool RemovePlaylist(string name)
        {
            bool removedCurrentPlaylist = CurrentPlaylist.name.Equals(name);
            if (removedCurrentPlaylist)
            {
                LoadLibraryMediaPlaylist();
            }
            var playlists = new List<IWMPPlaylist>();
            for (int i = 0; i < mPlayer.playlistCollection.getByName(name).count; i++)
            {
                playlists.Add(mPlayer.playlistCollection.getByName(name).Item(i));
            }
            playlists.ForEach(x => mPlayer.playlistCollection.remove(x));
            Playlists.Remove(Playlists.SingleOrDefault(x => x.name.Equals(name)));
            return removedCurrentPlaylist;
        }

        public bool RemoveLibrary(string name)
        {
            //            var library = Libraries.Find(x => x.Name.Equals(name));
            var library = Libraries.SingleOrDefault(x => x.Name.Equals(name));
            var libraryPlaylistName = library.Playlist.name;
            bool removedCurrentLibrary = CurrentPlaylist.name.Equals(libraryPlaylistName);

            var removePlaylists = new List<IWMPPlaylist>();
            for (int i = 0; i < mPlayer.playlistCollection.getByName(libraryPlaylistName).count; i++)
            {
                var media = mPlayer.playlistCollection.getByName(libraryPlaylistName).Item(i);
                removePlaylists.Add(media);
            }
            removePlaylists.ForEach(x => mPlayer.playlistCollection.remove(x));
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
            for (int i = allLibrariesPlaylist.count - 1; i >= 0; i--)
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

        #region Library

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

                var audio = mPlayer.mediaCollection.getByAttribute("MediaType", "audio");
                for (var i = 0; i < audio.count; i++)
                {
                    var media = audio.Item[i];
                    if (media.sourceURL.ToLower().Contains(url.ToLower()))
                    {
                        var count = mPlayer.mediaCollection.getAll().count;
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
                    mPlayer.mediaCollection.add(x.ToLower());
                    var song = Song.Create(x.ToLower());
                    SongInfo.Add(x.ToLower(), song);
                }
            });
        }

        public void AddTrackToPlaylist(int trackIndex, string playlistName)
        {
            var song = mPlayer.currentPlaylist.Item[trackIndex];
            var playlists = mPlayer.playlistCollection.getByName(playlistName);
            for (int i = 0; i < playlists.count; i++)
            {
                mPlayer.playlistCollection.getByName(playlistName).Item(i).appendItem(song);
            }
            SetCurrentPlaylistSongUrl(playlists.Item(0));
        }

        public void CreatePlaylist(string name)
        {
            Playlists.Add(mPlayer.playlistCollection.newPlaylist(name));
            ConfigFile.SaveNewPlaylist(name);
        }

        public void AddPlaylist(string name)
        {
            if (mPlayer.playlistCollection.getByName(name).count > 0)
            {
                Playlists.Add(mPlayer.playlistCollection.getByName(name).Item(0));
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
            var mediaCollection = mPlayer.mediaCollection.getByAttribute("MediaType", "audio");
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

            for (int i = 0; i < sortableList.Count; i++)
            {
                var index = CurrentSongs.IndexOf(sortableList[i]);
                CurrentSongs.Move(index, i);
                CurrentPlaylist.moveItem(index, i);
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateSourceTrigger(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
