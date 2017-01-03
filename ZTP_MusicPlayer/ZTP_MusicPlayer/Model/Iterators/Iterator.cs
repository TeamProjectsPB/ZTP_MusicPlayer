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
        private int _current = 0;
        private int _step = 1;

        public Iterator(CurrentSongsCollection collection)
        {
            this.collection = collection;
        }

        public Song First()
        {
            _current = 0;
            return collection[_current];
        }

        public Song Next()
        {
            _current = collection.CurrentSongIndex();
            _current += _step;
            if (_current >= collection.Count)
            {
                _current = 0;
            }
            return collection[_current];
        }

        public Song CurrentItem { get { return collection[_current]; } }

    }
}
