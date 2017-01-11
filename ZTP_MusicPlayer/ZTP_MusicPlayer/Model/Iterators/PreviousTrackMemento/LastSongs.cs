namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    //MementoPattern
    internal class LastSongs : ICaretaker
    {
        private readonly Caretaker caretaker = new Caretaker();

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