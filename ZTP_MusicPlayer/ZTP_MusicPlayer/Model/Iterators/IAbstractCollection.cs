using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTP_MusicPlayer.Model.Iterators
{
    interface IAbstractCollection
    {
        Iterator CreateNormalIterator();
        RandomIterator CreateRandomIterator();
    }
}
