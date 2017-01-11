namespace ZTP_MusicPlayer.Model.Iterators
{
    internal interface IAbstractCollection
    {
        Iterator CreateNormalIterator();
        RandomIterator CreateRandomIterator();
    }
}