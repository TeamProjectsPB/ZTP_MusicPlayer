using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento;

namespace ZTP_MusicPlayer.Model.Iterators
{
    class RandomIterator : IAbstractIterator
    {
        Random rand;
        CurrentSongsCollection collection;
        private int _current = 0;



        public RandomIterator(CurrentSongsCollection collection)
        {
            this.collection = collection;
            rand = new Random();
        }

        public File First()
        {
            _current = 0;
            return collection[_current];
        }

        public File Next()
        {
            _current = collection.CurrentSongIndex();
            var range = Enumerable.Range(0, collection.Count).Where(i => i != _current);
            int index = rand.Next(0, collection.Count - 1);
            _current = range.ElementAt(index);
            return collection[_current];
        }

        public void SetCurrentIndex(File track)
        {
            _current = collection.CurrentSongs.IndexOf(track);
        }

        public bool CanPrevious()
        {
            return !collection.IsCareTakerStackEmpty();
        }

        public File Previous()
        {
            //throw new NotImplementedException();
            return collection.GetPreviousSong();
        }

        public bool CanNext()
        {
            //throw new NotImplementedException();
            return true;
        }
    }
}
