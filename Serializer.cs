using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetThemis
{
    public class Serializer
    {

        public SerializerOption Option { get; set; } = SerializerOption.Define;

        public IDataPrimitives Primitives = new Primitives();

        public void WriterToStream(Stream stream, object data, string name = null, SchemaDefine schema = null,
            Action<Stream, object> serialize = null)
        {
            Type dtype = schema.GetType;

            var head = schema.HeadDefine;



            //获取头Id
            if (name != null)
            {
                if (head > 0)
                {
                 
                    //写入数值类型
                    if (data == null)
                    {
                        Primitives.WritePrimitive(stream, head + 1);

                        return;
                    }
                    else
                    {
                        Primitives.WritePrimitive(stream, head);

                    }
                }
                else
                {
                    //写入Bson标记
                    Primitives.WritePrimitive(stream, (int) DataHead.Bson);
                    //写入Name
                    Primitives.WritePrimitive(stream, name);
                    //写入数值类型
                    if (data == null)
                    {
                        Primitives.WritePrimitive(stream, (int) DataHead.Null);
                        return;
                    }
                    var dataType = dtype;
                    var typeDefine = schema.HeadItem;

                    if (typeDefine != null) //预定于类型
                    {
                        Primitives.WritePrimitive(stream, typeDefine.Tid);
                    }
                    else //非预定义类型
                    {
                        if (data is Array || data is IList)
                        {
                            WriterArray(stream, data, dataType, schema, serialize);
                            return;
                        }

                        serialize(stream, data);
                        return;
                    }
                }
            }
            else
            {
                if (data == null)
                {
                    Primitives.WritePrimitive(stream, (int) DataHead.Null);
                    return;
                }
            }

            if (data is Array || data is IList)
            {
                WriterArray(stream, data, dtype, schema, serialize);
            }
            else
            {
                if (schema.HeadItem == null)
                {
                    //如果未指定类型则写入类型
                    //if (data != null)
                    //{
                    //    dtype = data.GetType();
                    //}
                    //var titem = Option._DefineDataType.First(r => r.DataType == dtype);
                    //Primitives.WritePrimitive(stream, titem.Tid);
                    serialize(stream, data);
                }
                
                else
                {
                    WriterToStream(stream, data, dtype);
                }

            }
        }



        private void WriterArray(Stream stream, object data, Type type, SchemaDefine schema = null,
            Action<Stream, object> serialize = null)
        {
            bool isObject = false;
            var count = data is Array ? (data as Array).Length : (data as IList).Count;
            if (schema.SubDefine.HeadItem == null)
            {

                //写入无类型数组标记
                Primitives.WritePrimitive(stream, (int) DataHead.Array);
                //动态数组需要预读类型，所以无需长度
                isObject = true;
            }
            else
            {
                //写入类型数组标记
                Primitives.WritePrimitive(stream, (int) DataHead.ArrayT);

                //写长度
                Primitives.WritePrimitive(stream, count);
            }
            if (schema.arrayConvert != null)
            {
                var baseList = BaseList.Build(data, schema.arrayConvert);
                for (int i = 0; i < baseList.Lenght; i++)
                {
                    var item = baseList[i];
                    //写入类型
                    if (isObject && item != null)
                    {
                        var sdefine = Option.BuildTypeSubSchema(null, item.GetType());
                        if (sdefine.HeadItem != null)
                        {
                            WriterToStream(stream, sdefine.HeadItem.Tid, typeof(int));
                            WriterToStream(stream, item, null, sdefine, serialize);
                        }
                        else
                        {
                            WriterToStream(stream, item, null, schema.SubDefine, serialize);
                        }

                    }
                    else
                    {
                        WriterToStream(stream, item, null, schema.SubDefine, serialize);
                    }
                }
            }
            else
            {
                foreach (var item in data as IEnumerable)
                {
                    //写入类型
                    if (isObject && item != null)
                    {
                        var sdefine = Option.BuildTypeSubSchema(null, item.GetType());
                        if (sdefine.HeadItem != null)
                        {
                            WriterToStream(stream, sdefine.HeadItem.Tid, typeof(int));
                            WriterToStream(stream, item, null, sdefine, serialize);
                        }
                        else
                        {
                            WriterToStream(stream, item, null, schema.SubDefine, serialize);
                        }

                    }
                    else
                    {
                        WriterToStream(stream, item, null, schema.SubDefine, serialize);
                    }
                }
               
            }
            
         
            //写入数组结尾

            Primitives.WritePrimitive(stream, (int) DataHead.ArrayEnd);
        }

        internal List<object> ReadArray(Stream stream, DataHead subhead, SchemaDefine schema )
        {
            var subvalue = new List<object>();
            if (subhead == DataHead.ArrayT)
            {
                var len = (int) ReadObjectFromStream(stream, typeof(int));
                for (int i = 0; i < len; i++)
                {
                    var value = ReadObjectFromStream(stream, schema.SubDefine.SrcType);
                    subvalue.Add(value);
                }
                var arrayhead = ReadHead(stream);

                if (arrayhead != DataHead.ArrayEnd)
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                while (true)
                {
                    var arrayhead = ReadHead(stream);

                    if (arrayhead == DataHead.ArrayEnd)
                    {
                        break;
                    }

                    var value = ReadObjectFromStream(stream, schema.SubDefine.SrcType);
                    subvalue.Add(value);
                }
            }

            return subvalue;
        }

        internal void WriterToStream(Stream stream, object data, Type type)
        {
           
   
                if (type == typeof(string))
                {
                    Primitives.WritePrimitive(stream, (string) data);
                }
                else if (type == typeof(int))
                {
                    Primitives.WritePrimitive(stream, (int) data);
                }
                else if (type == typeof(bool))
                {
                    Primitives.WritePrimitive(stream, (bool) data);
                }
                else if (type == typeof(byte))
                {
                    Primitives.WritePrimitive(stream, (byte) data);
                }
                else if (type == typeof(byte[]))
                {
                    Primitives.WritePrimitive(stream, (byte[]) data);
                }
                else if (type == typeof(char))
                {
                    Primitives.WritePrimitive(stream, (char) data);
                }
                else if (type == typeof(DateTime))
                {
                    Primitives.WritePrimitive(stream, (DateTime) data);
                }
                else if (type == typeof(double))
                {
                    Primitives.WritePrimitive(stream, (double) data);
                }
                else if (type == typeof(float))
                {
                    Primitives.WritePrimitive(stream, (float) data);
                }
                else if (type == typeof(long))
                {
                    Primitives.WritePrimitive(stream, (long) data);
                }
                else if (type == typeof(sbyte))
                {
                    Primitives.WritePrimitive(stream, (sbyte) data);
                }
                else if (type == typeof(short))
                {
                    Primitives.WritePrimitive(stream, (short) data);
                }
                else if (type == typeof(uint))
                {
                    Primitives.WritePrimitive(stream, (uint) data);
                }
                else if (type == typeof(ulong))
                {
                    Primitives.WritePrimitive(stream, (ulong) data);
                }
                else if (type == typeof(ushort))
                {
                    Primitives.WritePrimitive(stream, (ushort) data);
                }

                else
                {
                    throw new NotImplementedException();
                }
    
        }


        public  string ReadString(Stream stream)
        {
            return Primitives.ReadString(stream);
        }

        public DataHead ReadHead(Stream stream)
        {
            return (DataHead) Primitives.ReadInt(stream);
        }

        internal object ReadObjectFromStream(Stream stream, Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (type == typeof(String))
            {
                return Primitives.ReadString(stream);
            }
            else if (type == typeof(int))
            {
                return Primitives.ReadInt(stream);
            }
            else if (type == typeof(bool))
            {
                return Primitives.ReadBool(stream);

            }
            else if (type == typeof(byte))
            {
                return Primitives.ReadByte(stream);
            }
            else if (type == typeof(byte[]))
            {
                return Primitives.ReadBytes(stream);
            }
            else if (type == typeof(char))
            {
                return Primitives.ReadChar(stream);
            }
            else if (type == typeof(DateTime))
            {
                return Primitives.ReadDateTime(stream);

            }
            else if (type == typeof(double))
            {
                return Primitives.ReadDouble(stream);
            }
   
            else if (type == typeof(float))
            {
                return Primitives.ReadFloat(stream);
            }
            else if (type == typeof(long))
            {
                return Primitives.ReadLong(stream);
            }
            else if (type == typeof(sbyte))
            {
                return Primitives.ReadSbyte(stream);
            }
            else if (type == typeof(short))
            {
                return Primitives.ReadShort(stream);
            }
            else if (type == typeof(uint))
            {
                return Primitives.ReadUint(stream);
            }
            else if (type == typeof(ulong))
            {
                return Primitives.ReadUlong(stream);
            }
            else if (type == typeof(ushort))
            {
                return Primitives.ReadUshort(stream);
            }
           
            else
            {
                throw new NotImplementedException();
            }

        }
    }
}