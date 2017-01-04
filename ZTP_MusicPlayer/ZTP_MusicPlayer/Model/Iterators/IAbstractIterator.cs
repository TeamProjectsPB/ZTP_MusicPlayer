using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Song = TagLib.File;
namespace ZTP_MusicPlayer.Model.Iterators
{
    interface IAbstractIterator
    {
        Song First();
        bool CanPrevious();
        Song Previous();
        bool CanNext();
        Song Next();
        //void SetCurrentIndex(Song track);
    }
}
