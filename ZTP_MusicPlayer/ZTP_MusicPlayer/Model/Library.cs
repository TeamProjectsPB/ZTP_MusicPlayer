using WMPLib;

namespace ZTP_MusicPlayer.Model
{
    public class Library
    {
        #region Properties

        public string Name { get; set; }

        public string Url { get; set; }

        public IWMPPlaylist Playlist { get; set; }

        #endregion

        #region Constructor

        public Library(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public Library(string name, string url, IWMPPlaylist playlist)
        {
            Name = name;
            Url = url;
            Playlist = playlist;
        }

        #endregion
    }
}