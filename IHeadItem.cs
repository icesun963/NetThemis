using System;

namespace NetThemis
{
    public interface IHeadItem
    {
        int Tid { get; set; }
        string Name { get; set; }

        Type Type { get; set; }
    }
}