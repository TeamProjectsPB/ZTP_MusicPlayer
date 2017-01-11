using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //MementoPattern
    internal class Originator
    {
        public Song State { get; set; }

        public Memento CreateMemento()
        {
            return new Memento(State);
        }

        public void SetMemento(Memento m)
        {
            State = m.GetState();
        }
    }
}