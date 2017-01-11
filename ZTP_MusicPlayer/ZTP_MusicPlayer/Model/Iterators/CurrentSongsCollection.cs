using System.Collections.ObjectModel;
using ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    internal class CurrentSongsCollection : IAbstractCollection
    {
        //MementoPattern
        private readonly Originator originator = new Originator();
        private ICaretaker lastSongs = new LastSongs();


        public CurrentSongsCollection()
        {
        }


        public CurrentSongsCollection(ObservableCollection<Song> currentSongs)
        {
            CurrentSongs = currentSongs;
        }

        //endofMementoPattern
        public ObservableCollection<Song> CurrentSongs { get; set; }

        public Song CurrentSong
        {
            get { return originator.State; }
            set
            {
                if (originator.State != null && originator.State != value)
                {
                    lastSongs.SaveState(originator);
                }
                originator.State = value;
            }
        }

        public int Count
        {
            get { return CurrentSongs.Count; }
        }

        public Song this[int index]
        {
            get { return index >= CurrentSongs.Count ? null : CurrentSongs[index]; }
            set { CurrentSongs.Add(value); }
        }

        public Iterator CreateNormalIterator()
        {
            lastSongs = new LastSongs();
            return new Iterator(this);
        }

        public RandomIterator CreateRandomIterator()
        {
            lastSongs = new LastSongs();
            return new RandomIterator(this);
        }

        public int CurrentSongIndex()
        {
            return CurrentSongs.IndexOf(CurrentSong);
        }

        public Song GetPreviousSong()
        {
            lastSongs.RestoreState(originator);
            return CurrentSong;
        }

        public bool IsCareTakerStackEmpty()
        {
            return lastSongs.IsStackEmpty();
        }
    }
}