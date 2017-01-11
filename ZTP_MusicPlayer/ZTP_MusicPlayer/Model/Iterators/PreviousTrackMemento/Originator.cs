using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //MementoPattern
    internal class Originator
    {
        public Song State { get; set; }

        public Memento CreateMemento()
        {
            //Memento m = new Memento(state);
            //m.SetState(state);
            //return m;
            return new Memento(State);
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