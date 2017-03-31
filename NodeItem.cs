using System;

namespace NetThemis
{
    public class NodeItem : HeadItem
    {
        public  int DataHead { get; set; }

        public Type DataType
        {
            get { return Type; }
            set { Type = value; }
        }
    }
}