using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    interface ICaretaker
    {
        void SaveState(Originator orig);
        void RestoreState(Originator orig);
        bool IsStackEmpty();
    }
}
