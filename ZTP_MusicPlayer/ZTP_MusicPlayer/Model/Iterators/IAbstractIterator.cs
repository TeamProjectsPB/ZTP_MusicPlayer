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
        Song Next();
        Song CurrentItem { get; }
        //void SetCurrentIndex(Song track);
    }
}
