using System.Collections.Generic;

namespace Editor
{
    public class FramesList
    {
        public List<Frame> framesList;

        public FramesList()
        {
            framesList = new List<Frame>();
        }

        public int Count()
        {
            return framesList.Count;
        }

        public void Add(Frame frame)
        {
            framesList.Add(frame);
        }

        public Frame this[int i]
        {
            get => framesList[i];
            set => framesList[i] = value;
        }
    }
}