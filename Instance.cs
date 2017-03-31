using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetThemis
{
    public class Instance
    {
        private static readonly Dictionary<Type, TypeSchemas> SerializerSchemaMap =
            new Dictionary<Type, TypeSchemas>();

        static Serializer Serializer = new Serializer();

      
        public class  TypeSchemas
        {
            public TypeSchema[] tSchemata;
            public Func<object> Cor;
        }

        private static TypeSchemas BuildTypeSchema(Type type)
        {
            var plist = Serializer.Option.GetProperties(type);
            var result = new List<TypeSchema>();

            foreach (var myPropertyInfo in plist)
            {
                var name = myPropertyInfo.Name;
     
                var ptype = myPropertyInfo.PropertyType;

                var define =Serializer.Option.BuildTypeSubSchema(name, ptype);

                var get = fastJSON.Reflection.CreateGetMethod(type, myPropertyInfo);
                var set = fastJSON.Reflection.CreateSetMethod(type, myPropertyInfo);


                var newSer = new TypeSchema()
                {
                    Name = name,
                    Define = define,
                    GetFunc = (rx) => { return get(rx); },
                    SetFunc = (rx, rv) => { set(rx, rv); }
                };
                result.Add(newSer);
            }

   
            var config= new TypeSchemas()
            {
                Cor = SerializerOption.FastCreateInstanceBuild(type),
                tSchemata = result.ToArray()
            };
            SerializerSchemaMap[type] = config;
            return config;
        }

        public static void Serialize<T>(Stream stream, T data)
        {
            Serialize(stream, data, typeof(T));
        }

        public static void Serialize(Stream stream, object data,Type type)
        {

            TypeSchemas schemas = null;
            if (!SerializerSchemaMap.TryGetValue(type, out schemas))
            {
                schemas = BuildTypeSchema(type);
            }
          
            foreach (var schema in schemas.tSchemata)
            {
                if (schema.Define.HeadItem == null || schema.Define.IsArray)
                {
                    var subdata = schema.GetFunc(data);
                    if (schema.Define.IsArray)
                    {
                        Serializer.WriterToStream(stream, subdata, schema.Name, schema.Define,(stm,sd)=>
                        {
                            Serializer.WriterToStream(stream, (int)DataHead.Dic, typeof(int));
                            //写入头
                            Serializer.WriterToStream(stream, (int)DataHead.LvStart, typeof(int));
                            Serialize(stream, sd, sd.GetType());
                            //写入尾
                            Serializer.WriterToStream(stream, (int)DataHead.LvEnd, typeof(int));
                        });
                    }
                    else
                    {
                        Serializer.WriterToStream(stream, (int)DataHead.Dic, typeof(int));
                        //写入头
                        Serializer.WriterToStream(stream, (int)DataHead.LvStart, typeof(int));
                        Serialize(stream, subdata, subdata == null ? null : subdata.GetType());
                        //写入尾
                        Serializer.WriterToStream(stream, (int)DataHead.LvEnd, typeof(int));
                    }
                    
                }
                else
                {
                    Serializer.WriterToStream(stream, schema.GetFunc(data), schema.Name, schema.Define);
                }
            }
            
        }

        public static T Deserialize<T>(Stream stream)
        {
            return (T) Deserialize(stream, typeof(T));
        }

        public static object Deserialize(Stream stream,Type type)
        {
            TypeSchemas schemas = null;
            if (!SerializerSchemaMap.TryGetValue(type, out schemas))
            {
                schemas = BuildTypeSchema(type);
            }
            var data = schemas.Cor();

            foreach (var schema in schemas.tSchemata)
            {
                var head = Serializer.ReadHead(stream);
                if (head == DataHead.Bson)
                {
                    Serializer.ReadString(stream);
                    var subhead= Serializer.ReadHead(stream);
                    if (subhead == DataHead.Array || subhead == DataHead.ArrayT)
                    {
                        ReadArray(stream, subhead, schema, data);
                    }
                    else
                    {
                        var value = Serializer.ReadObjectFromStream(stream, schema.Define.SrcType);
                        schema.SetFunc(data, value);
                    }
                  
                }
                else if (head == DataHead.Dic)
                {
                    var subheads = Serializer.ReadHead(stream);
                    var value = Deserialize(stream, schema.Define.SrcType);
                    schema.SetFunc(data, value);
                    var subheade = Serializer.ReadHead(stream);
                }
                else
                {

                    if (schema.Define.HeadItem == null || schema.Define.IsArray)
                    {
                        var shead = Serializer.ReadHead(stream);
          
                        ReadArray(stream, shead, schema, data);
                    }
                    else
                    {
                        if ((int)head == schema.Define.HeadItem.Tid)
                        {
                            var value = Serializer.ReadObjectFromStream(stream, schema.Define.SrcType);
                            schema.SetFunc(data, value);
                        }
                        else
                        {
                            schema.SetFunc(data, null);
                        }
                       
                    }
                }
            }
           
            return data;
        }

        private static void ReadArray(Stream stream, DataHead subhead, TypeSchema schema, object data)
        {
            var listo = Serializer.ReadArray(stream, subhead, schema.Define);

            if (schema.Define.arrayConvertBack != null)
            {
                var value = schema.Define.arrayConvertBack(listo);
                schema.SetFunc(data, value);
            }
            else
            {
                if (schema.Define.IsList)
                {
                    var value = fastJSON.Reflection.Instance.FastCreateInstance(schema.Define.SrcType) as IList;
                    for (int i = 0; i < listo.Count; i++)
                    {
                        value.Add(listo[0]);
                    }
                    schema.SetFunc(data, value);
                }
                else
                {
                    var value = Array.CreateInstance(schema.Define.SubDefine.SrcType, listo.Count);
                    for (int i = 0; i < listo.Count; i++)
                    {
                        value.SetValue(listo[i], i);
                    }
                    schema.SetFunc(data, value);
                }
            }
        }
    }
}
