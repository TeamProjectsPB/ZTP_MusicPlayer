using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    //MementoPattern
    class LastSongs : ICaretaker
    {
        
        Caretaker caretaker = new Caretaker();
        public bool IsStackEmpty()
        {
            return caretaker.IsStackEmpty();
        }

        public void RestoreState(Originator orig)
        {
            caretaker.RestoreState(orig);
        }

        public void SaveState(Originator orig)
        {
            caretaker.SaveState(orig);
        }
    }
}
