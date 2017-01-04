using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    class CurrentSongsCollection : IAbstractCollection
    {
        private ObservableCollection<Song> _currentSongs;
        //private Song _currentSong;

        //MementoPattern
        Originator originator = new Originator();
//        Caretaker caretaker = new Caretaker();
        LastSongs lastSongs = new LastSongs();
        //endofMementoPattern
        public ObservableCollection<Song> CurrentSongs
        {
            get { return _currentSongs; }
            set { _currentSongs = value; }
        }

        public Song CurrentSong
        {
            //            get { return _currentSong; }
            //            set { _currentSong = value; }
            get { return originator.State; }
            set
            {
                if (originator.State != null && originator.State != value)
                {
                    //                    caretaker.SaveState(originator);                    
                    lastSongs.SaveState(originator);
                }
                originator.State = value;
            }
        }

        public int Count
        {
            get { return _currentSongs.Count; }
        }
        public Song this[int index]
        {
            get
            {
                return index >= CurrentSongs.Count ? null : _currentSongs[index];
                /*if (index >= CurrentSongs.Count)
                {
                    return null;
                }*/
            }
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

        public Song GetPreviousSong()
        {
            //originator.SetMemento(caretaker.);
            //caretaker.RestoreState(originator);
            lastSongs.RestoreState(originator);
            return CurrentSong;
        }
    
        public Iterator CreateNormalIterator()
        {
            //caretaker = new Caretaker();
            lastSongs = new LastSongs();
            return new Iterator(this);
        }

        public RandomIterator CreateRandomIterator()
        {
            //caretaker = new Caretaker();
            lastSongs = new LastSongs();
            return new RandomIterator(this);
        }

        public bool IsCareTakerStackEmpty()
        {
            //return caretaker.IsStackEmpty();
            return lastSongs.IsStackEmpty();
        }
    }
}
