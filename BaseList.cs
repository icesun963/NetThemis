using System;

namespace NetThemis
{
    public class BaseList
    {
        private object[] _list = null;

        public object this[int index]
        {
            get { return _list[index]; }
        }

        public BaseList(object input,Func<object,object[]> convertFunc = null )
        {
            
            if (convertFunc != null)
            {
                _list = convertFunc(input);
                Lenght = _list.Length;
            }
        }

        public int Lenght;

        public static BaseList Build(object item, Func<object, object[]> convertFunc)
        {
            return new BaseList(item, convertFunc);
        }
    }
}