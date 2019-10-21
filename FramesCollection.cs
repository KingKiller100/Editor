using System.Collections;
using System.Collections.Generic;

namespace Editor
{
    public class FramesCollection
    {
        public List<FramesList> childFrameList;

        public FramesCollection()
        {
            childFrameList = new List<FramesList>();
            childFrameList.Add(new FramesList());
        }

        public List<FramesList> ChildList()
        {
            return childFrameList;
        }
    }
}