using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WMPLib;
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
        private List<string> currentPlaylistSongUrl;
        private IWMPPlaylist allLibrariesPlaylist;
        private string allLibrariesPlaylistName;
        private ObservableCollection<Song> _currentSongs;

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
            get { return mPlayer.currentPlaylist; }
            set { mPlayer.currentPlaylist = value; }
        }

        public static Dictionary<string, Song> SongInfo { get; set; }
        public ObservableCollection<IWMPPlaylist> Playlists { get; set; }

        public Dictionary<string, List<string>> PlaylistsUrl { get; set; }
        //        public List<Library> Libraries { get; set; }
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
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentPositionString : "00:00"; }
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



        public ObservableCollection<Song> CurrentSongs
        {
            get { return _currentSongs; }
            set { _currentSongs = value; }
        }

        public string CurrentSong
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
            mPlayer = new WindowsMediaPlayer();
            SongInfo = new Dictionary<string, Song>();
            LoadMediaInfo();
            currentPlaylistSongUrl = new List<string>();
            PlaylistsUrl = new Dictionary<string, List<string>>();
            Playlists = new ObservableCollection<IWMPPlaylist>();
            Libraries = new ObservableCollection<Library>();
            allLibrariesPlaylistName = "allLibrariesPlaylist";
            allLibrariesPlaylist = GetPlaylistFromMediaCollection(allLibrariesPlaylistName);
            CurrentSongs = new ObservableCollection<Song>();
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

        public void NextTrack()
        {
            mPlayer.controls.next();
        }

        public void PreviousTrack()
        {
            mPlayer.controls.previous();
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
            mPlayer.settings.setMode("shuffle", randomPlay);
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
            if (!PlaylistsUrl.ContainsKey(newCurrentPlaylist))
            {
                SetCurrentPlaylistSongUrl(temporaryPlaylist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[newCurrentPlaylist];
            SetCurrentSongs();
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
        }

        public void LoadCurrentSong(int index)
        {
            mPlayer.controls.playItem(mPlayer.currentPlaylist.Item[index]);
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

        private void LoadMediaInfo()
        {
            var mediaCollection = mPlayer.mediaCollection.getByAttribute("MediaType", "audio");
            var count = mediaCollection.count;
            for (var i = 0; i < mediaCollection.count; i++)
            {
                var media = mediaCollection.Item[i];
                var url = media.sourceURL;
                var mediaInfo = Song.Create(media.sourceURL);
                SongInfo.Add(media.sourceURL.ToLower(), mediaInfo);
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
