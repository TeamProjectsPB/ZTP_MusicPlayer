using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    internal class Iterator : IAbstractIterator
    {
        private readonly int _step = 1;
        private readonly CurrentSongsCollection collection;

        public Iterator(CurrentSongsCollection collection)
        {
            this.collection = collection;
        }

        public Song First()
        {
            var _current = 0;
            return collection[_current];
        }

        public Song Next()
        {
            var _current = collection.CurrentSongIndex();
            _current = (_current + _step)%collection.Count;
            return collection[_current];
        }

        public bool CanPrevious()
        {
            return true;
        }

        public Song Previous()
        {
            var _current = collection.CurrentSongIndex();
            _current -= _step;
            if (_current < 0)
            {
                _current = collection.Count - 1;
            }
            return collection[_current];
        }

        public bool CanNext()
        {
            return true;
        }
    }
}