using System;
using System.Linq;
using TagLib;

namespace ZTP_MusicPlayer.Model.Iterators
{
    internal class RandomIterator : IAbstractIterator
    {
        private int _current;
        private readonly CurrentSongsCollection collection;
        private readonly Random rand;


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
            var index = rand.Next(0, collection.Count - 1);
            _current = range.ElementAt(index);
            return collection[_current];
        }

        public bool CanPrevious()
        {
            return !collection.IsCareTakerStackEmpty();
        }

        public File Previous()
        {
            return collection.GetPreviousSong();
        }

        public bool CanNext()
        {
            return true;
        }

        public void SetCurrentIndex(File track)
        {
            _current = collection.CurrentSongs.IndexOf(track);
        }
    }
}