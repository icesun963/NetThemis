using System;

namespace NetThemis
{
    public class HeadItem : IHeadItem
    {
        public int Tid { get; set; }
        public string Name { get; set; }

        public Type Type { get; set; }
    }
}