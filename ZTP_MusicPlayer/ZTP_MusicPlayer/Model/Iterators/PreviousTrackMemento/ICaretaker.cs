namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    internal interface ICaretaker
    {
        void SaveState(Originator orig);
        void RestoreState(Originator orig);
        bool IsStackEmpty();
    }
}