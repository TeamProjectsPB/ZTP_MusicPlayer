using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Song = TagLib.File;
namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //MementoPattern
    public class Memento
    {     
        private Song state;

        public Memento(Song state)
        {
            this.state = state;
        }

        public Song GetState() { return state; }

        public void SetState(Song state)
        {
            this.state = state;
        }
    }
}
