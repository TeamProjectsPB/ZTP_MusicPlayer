using Song = TagLib.File;

namespace ZTP_MusicPlayer.Model.Iterators
{
    internal interface IAbstractIterator
    {
        Song First();
        bool CanPrevious();
        Song Previous();
        bool CanNext();
        Song Next();
    }
}