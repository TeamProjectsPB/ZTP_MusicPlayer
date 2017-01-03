using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    class CurrentSongsCollection : IAbstractCollection
    {
        private ObservableCollection<Song> _currentSongs;
        private Song _currentSong;

        public ObservableCollection<Song> CurrentSongs
        {
            get { return _currentSongs; }
            set { _currentSongs = value; }
        }

        public Song CurrentSong
        {
            get { return _currentSong; }
            set { _currentSong = value; }
        }

        public int Count
        {
            get { return _currentSongs.Count; }
        }
        public Song this[int index]
        {
            get { return _currentSongs[index]; }
            set { _currentSongs.Add(value); }
        }


        public CurrentSongsCollection()
        {
        }
        

        public CurrentSongsCollection(ObservableCollection<Song> currentSongs)
        {
            _currentSongs = currentSongs;
        }

        public int CurrentSongIndex()
        {
            return CurrentSongs.IndexOf(CurrentSong);
        }


       

        public Iterator CreateNormalIterator()
        {
            return new Iterator(this);
        }

        public RandomIterator CreateRandomIterator()
        {
            return new RandomIterator(this);
        }
    }
}
