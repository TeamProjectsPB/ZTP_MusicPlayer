using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //MementoPattern
    class Originator
    {
        private Song state;

        public Song State { get { return state; } set { state = value; } }

        public Memento CreateMemento()
        {
            //Memento m = new Memento(state);
            //m.SetState(state);
            //return m;
            return new Memento(state);
        }

        public void SetMemento(Memento m)
        {
            State = m.GetState();
        }

        /*public void SetState(Song state)
        {
            this.state = state;
        }*/
    }
}
