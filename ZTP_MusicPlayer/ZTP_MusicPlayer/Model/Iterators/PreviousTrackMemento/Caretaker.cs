using System.Collections.Generic;

namespace ZTP_MusicPlayer.Model.Iterators.PreviousTrackMemento
{
    //ProxyPattern
    //MementoPattern
    internal class Caretaker : ICaretaker
    {
        private readonly Stack<Memento> mementoStack = new Stack<Memento>();

        public void SaveState(Originator orig)
        {
            if (mementoStack.Count == 0 || orig.State != mementoStack.Peek().GetState())
            {
                mementoStack.Push(orig.CreateMemento());
            }
        }

        public void RestoreState(Originator orig)
        {
            orig.SetMemento(mementoStack.Pop());
        }

        public bool IsStackEmpty()
        {
            return mementoStack.Count == 0;
        }
    }
}