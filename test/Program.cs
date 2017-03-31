using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NetThemis
{

    public class MyClass
    {
        public  string Name { get; set; }
        public  int Value { get; set; }

        public  int Id { get; set; }

        public int [] Ids { get; set; } = new int[10];

       // public object[] Ods { get; set; } = new object[10];

        public List<SubClass> Classes { get; set; } = new List<SubClass>();

        public SubClass SubClass { get; set; } = new SubClass();
    }

    public class SubClass
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public int Id { get; set; }
    }



    class Program
    {



        static void Main(string[] args)
        {

            var fun = SerializerOption.BuildArrayConvert(typeof(string[]));

            var fun1 = SerializerOption.BuildListConvert(typeof(List<string>));
            var fun2 = SerializerOption.BuildListConvert(typeof(List<SubClass>));
            var fun3 = SerializerOption.BuildArrayConvertBack(typeof(int[]));
            var fun4 = SerializerOption.BuildListConvertBack(typeof(List<string>));

            var result = fun3(new List<object>(new object[] {0}));
            result = fun4(new List<object>(new object[] { "0" }));
            var sw = new Stopwatch();




            SerializerOption.Define.AddType(typeof(MyClass));
            SerializerOption.Define.AddType(typeof(SubClass));

            {
                var item = new MyClass();
                //item.Classes.Add(new SubClass());
                item.Name = "XXXX";
                //item.Ods[0] = 1;
                var mstr = new MemoryStream();
                Instance.Serialize(mstr, item);
                mstr.Position = 0;
                Instance.Deserialize<MyClass>(mstr);
            }

       
            sw.Reset();
            sw.Restart();

            for (int i = 0; i < 10000*30; i++)
            {
                var item = new MyClass();

                item.Name = "XXXX";
                //item.SubClass.Name = "SSSS";
                var mstr = new MemoryStream();
                Instance.Serialize(mstr, item);
                mstr.Position = 0;
                Instance.Deserialize<MyClass>(mstr);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadLine();
        }

    }
}
