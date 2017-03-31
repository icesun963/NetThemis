using System;

namespace NetThemis
{
    public class TypeSchema
    {
        public SchemaDefine Define;
        public Func<object, object> GetFunc;
        public string Name;
        public Action<object, object> SetFunc;


        public override string ToString()
        {
            return "[" + Name + "]" + Define;
        }
    }
}