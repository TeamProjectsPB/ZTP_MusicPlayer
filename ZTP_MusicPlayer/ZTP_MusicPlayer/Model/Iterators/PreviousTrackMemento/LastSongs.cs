
using System.Diagnostics;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    //MementoPattern
    internal class LastSongs : ICaretaker
    {
        private readonly Caretaker caretaker = new Caretaker();

        public bool IsStackEmpty()
        {
            if (caretaker.IsStackEmpty())
            {
                Trace.WriteLine("Stos ejst pusty");
            }
            else
            {
                Trace.WriteLine("Stos ma elementy!");
            }
            return caretaker.IsStackEmpty();
        }

        public void RestoreState(Originator orig)
        {
            Trace.WriteLine("State restoring.\nOld state: " + orig.State.Name);
            caretaker.RestoreState(orig);
            Trace.WriteLine("\nNew state: " + orig.State.Name);
        }

        public void SaveState(Originator orig)
        {
            Trace.WriteLine("State saving.\nOld state: " + orig.State.Name);
            caretaker.SaveState(orig);
            Trace.WriteLine("\nNew state: " + orig.State.Name);
        }
    }
}