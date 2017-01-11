using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    class Iterator : IAbstractIterator
    {
        CurrentSongsCollection collection;
        private int _step = 1;

        public Iterator(CurrentSongsCollection collection)
        {
            this.collection = collection;
        }

        public Song First()
        {
            int _current = 0;
            return collection[_current];
        }

        public Song Next()
        {
            int _current = collection.CurrentSongIndex();
            _current = (_current + _step)%collection.Count;
            return collection[_current];
        }

        public bool CanPrevious()
        {
            return true;
        }

        public Song Previous()
        {
            int _current = collection.CurrentSongIndex();
            _current -= _step;
            if (_current < 0)
            {
                _current = collection.Count-1;
            }
            return collection[_current];
        }

        public bool CanNext()
        {
            return true;
        }

    }
}
