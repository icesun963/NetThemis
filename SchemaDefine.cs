using System;
using System.Collections.Generic;

namespace NetThemis
{
    public class SchemaDefine
    {
        public Type SrcType;

        public Type GetType;

        public IHeadItem HeadItem;

        public int HeadDefine;

        public bool IsHeadDefine;

        public bool IsArray;

        public bool IsList;

        public Func<object, object[]> arrayConvert;

        public Func<List<object>, object> arrayConvertBack;

   

        public SchemaDefine SubDefine;
    }
}