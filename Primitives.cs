/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace NetThemis
{
    public  class Primitives : IDataPrimitives
    {
        public  string ReadString(Stream stream)
        {
            var head = "";
            ReadPrimitive(stream, out head);
            return head;
        }
        public  int ReadInt(Stream stream)
        {
            int result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  bool ReadBool(Stream stream)
        {
            bool result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  byte ReadByte(Stream stream)
        {
            byte result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  byte[] ReadBytes(Stream stream)
        {
            byte[] result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  char ReadChar(Stream stream)
        {
            char result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  DateTime ReadDateTime(Stream stream)
        {
            DateTime result;
            ReadPrimitive(stream, out result);
            return result;
        }
        public  double ReadDouble(Stream stream)
        {
            double result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  long ReadLong(Stream stream)
        {
            long result;
            ReadPrimitive(stream, out result);
            return result;
        }


        public  float ReadFloat(Stream stream)
        {
            float result;
            ReadPrimitive(stream, out result);
            return result;
        }


        public  SByte ReadSbyte(Stream stream)
        {
            SByte result;
            ReadPrimitive(stream, out result);
            return result;
        }


        public  short ReadShort(Stream stream)
        {
            short result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  UInt16 ReadUint(Stream stream)
        {
            UInt16 result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  ulong ReadUlong(Stream stream)
        {
            ulong result;
            ReadPrimitive(stream, out result);
            return result;
        }

        public  ushort ReadUshort(Stream stream)
        {
            ushort result;
            ReadPrimitive(stream, out result);
            return result;
        }

        internal  MethodInfo GetWritePrimitive(Type type)
		{
			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			MethodInfo writer = typeof(Primitives).GetMethod("WritePrimitive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null,
				new Type[] { typeof(Stream), type }, null);

			if (writer != null)
				return writer;

			if (type.IsGenericType)
			{
				var genType = type.GetGenericTypeDefinition();

				var mis = typeof(Primitives).GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.Where(mi => mi.IsGenericMethod && mi.Name == "WritePrimitive");

				foreach (var mi in mis)
				{
					var p = mi.GetParameters();

					if (p.Length != 2)
						continue;

					if (p[0].ParameterType != typeof(Stream))
						continue;

					var paramType = p[1].ParameterType;

					if (paramType.IsGenericType == false)
						continue;

					var genParamType = paramType.GetGenericTypeDefinition();

					if (genType == genParamType)
						return mi;
				}
			}

			return null;
		}

		internal  MethodInfo GetReadPrimitive(Type type)
		{
			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			var reader = typeof(Primitives).GetMethod("ReadPrimitive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null,
				new Type[] { typeof(Stream), type.MakeByRefType() }, null);

			if (reader != null)
				return reader;

			if (type.IsGenericType)
			{
				var genType = type.GetGenericTypeDefinition();

				var mis = typeof(Primitives).GetMethods(BindingFlags.Instance | BindingFlags.Public)
					.Where(mi => mi.IsGenericMethod && mi.Name == "ReadPrimitive");

				foreach (var mi in mis)
				{
					var p = mi.GetParameters();

					if (p.Length != 2)
						continue;

					if (p[0].ParameterType != typeof(Stream))
						continue;

					var paramType = p[1].ParameterType;

					if (paramType.IsByRef == false)
						continue;

					paramType = paramType.GetElementType();

					if (paramType.IsGenericType == false)
						continue;

					var genParamType = paramType.GetGenericTypeDefinition();

					if (genType == genParamType)
						return mi;
				}
			}

			return null;
		}

		 uint EncodeZigZag32(int n)
		{
			return (uint)((n << 1) ^ (n >> 31));
		}

		 ulong EncodeZigZag64(long n)
		{
			return (ulong)((n << 1) ^ (n >> 63));
		}

		 int DecodeZigZag32(uint n)
		{
			return (int)(n >> 1) ^ -(int)(n & 1);
		}

		 long DecodeZigZag64(ulong n)
		{
			return (long)(n >> 1) ^ -(long)(n & 1);
		}

		 uint ReadVarint32(Stream stream)
		{
			int result = 0;
			int offset = 0;

			for (; offset < 32; offset += 7)
			{
				int b = stream.ReadByte();
				if (b == -1)
					throw new EndOfStreamException();

				result |= (b & 0x7f) << offset;

				if ((b & 0x80) == 0)
					return (uint)result;
			}

			throw new InvalidDataException();
		}

		 void WriteVarint32(Stream stream, uint value)
		{
			for (; value >= 0x80u; value >>= 7)
				WriteByte(stream,(byte)(value | 0x80u));

			WriteByte(stream,(byte)value);
		}

		 ulong ReadVarint64(Stream stream)
		{
			long result = 0;
			int offset = 0;

			for (; offset < 64; offset += 7)
			{
				int b = stream.ReadByte();
				if (b == -1)
					throw new EndOfStreamException();

				result |= ((long)(b & 0x7f)) << offset;

				if ((b & 0x80) == 0)
					return (ulong)result;
			}

			throw new InvalidDataException();
		}

		 void WriteVarint64(Stream stream, ulong value)
		{
			for (; value >= 0x80u; value >>= 7)
				WriteByte(stream,(byte)(value | 0x80u));

			WriteByte(stream,(byte)value);
		}


		public  void WritePrimitive(Stream stream, bool value)
		{
			WriteByte(stream,value ? (byte)1 : (byte)0);
		}

		public  void ReadPrimitive(Stream stream, out bool value)
		{
			var b = stream.ReadByte();
			value = b != 0;
		}

		public  void WritePrimitive(Stream stream, byte value)
		{
			WriteByte(stream,value);
		}

		public  void ReadPrimitive(Stream stream, out byte value)
		{
			value = (byte)stream.ReadByte();
		}

		public  void WritePrimitive(Stream stream, sbyte value)
		{
			WriteByte(stream,(byte)value);
		}

		public  void ReadPrimitive(Stream stream, out sbyte value)
		{
			value = (sbyte)stream.ReadByte();
		}

		public  void WritePrimitive(Stream stream, char value)
		{
			WriteVarint32(stream, value);
		}

		public  void ReadPrimitive(Stream stream, out char value)
		{
			value = (char)ReadVarint32(stream);
		}

		public  void WritePrimitive(Stream stream, ushort value)
		{
			WriteVarint32(stream, value);
		}

		public  void ReadPrimitive(Stream stream, out ushort value)
		{
			value = (ushort)ReadVarint32(stream);
		}

		public  void WritePrimitive(Stream stream, short value)
		{
			WriteVarint32(stream, EncodeZigZag32(value));
		}

		public  void ReadPrimitive(Stream stream, out short value)
		{
			value = (short)DecodeZigZag32(ReadVarint32(stream));
		}

		public  void WritePrimitive(Stream stream, uint value)
		{
			WriteVarint32(stream, value);
		}

		public  void ReadPrimitive(Stream stream, out uint value)
		{
			value = ReadVarint32(stream);
		}

		public  void WritePrimitive(Stream stream, int value)
		{
			WriteVarint32(stream, EncodeZigZag32(value));
		}

		public  void ReadPrimitive(Stream stream, out int value)
		{
			value = DecodeZigZag32(ReadVarint32(stream));
		}

		public  void WritePrimitive(Stream stream, ulong value)
		{
			WriteVarint64(stream, value);
		}

		public  void ReadPrimitive(Stream stream, out ulong value)
		{
			value = ReadVarint64(stream);
		}

		public  void WritePrimitive(Stream stream, long value)
		{
			WriteVarint64(stream, EncodeZigZag64(value));
		}

		public  void ReadPrimitive(Stream stream, out long value)
		{
			value = DecodeZigZag64(ReadVarint64(stream));
		}
        public  void WriteByte(Stream stream, byte s)
        {

            stream.WriteByte(s);
        }

#if !NO_UNSAFE
		public  unsafe void WritePrimitive(Stream stream, float value)
		{
            //uint v = *(uint*)(&value);
            //WriteVarint32(stream, v);
		    var buff = BitConverter.GetBytes(value);
		    foreach (var b in buff)
		    {
		        WriteByte(stream,b);
		    }
		
		}

    

		public  unsafe void ReadPrimitive(Stream stream, out float value)
		{
            //uint v = ReadVarint32(stream);
            //value = *(float*)(&v);
		    var buff = new byte[4];
		    stream.Read(buff, 0, buff.Length);
		    value = BitConverter.ToSingle(buff, 0);
		}

		public  unsafe void WritePrimitive(Stream stream, double value)
		{
			ulong v = *(ulong*)(&value);
			WriteVarint64(stream, v);
		}

		public  unsafe void ReadPrimitive(Stream stream, out double value)
		{
			ulong v = ReadVarint64(stream);
			value = *(double*)(&v);
		}
#else
		public  void WritePrimitive(Stream stream, float value)
		{
			WritePrimitive(stream, (double)value);
		}

		public  void ReadPrimitive(Stream stream, out float value)
		{
			double v;
			ReadPrimitive(stream, out v);
			value = (float)v;
		}

		public  void WritePrimitive(Stream stream, double value)
		{
			ulong v = (ulong)BitConverter.DoubleToInt64Bits(value);
			WriteVarint64(stream, v);
		}

		public  void ReadPrimitive(Stream stream, out double value)
		{
			ulong v = ReadVarint64(stream);
			value = BitConverter.Int64BitsToDouble((long)v);
		}
#endif

		public  void WritePrimitive(Stream stream, DateTime value)
		{
			long v = value.ToBinary();
			WritePrimitive(stream, v);
		}

		public  void ReadPrimitive(Stream stream, out DateTime value)
		{
			long v;
			ReadPrimitive(stream, out v);
			value = DateTime.FromBinary(v);
		}
         UTF8Encoding encoding = new UTF8Encoding(false, true);

        public  
#if !NO_UNSAFE
            unsafe
#endif
            void WritePrimitive(Stream stream, string value)
		{
			if (value == null)
			{
				WritePrimitive(stream, (uint)0);
				return;
			}

#if GC_NICE_VERSION
			WritePrimitive(stream, (uint)value.Length + 1);

			foreach (char c in value)
				WritePrimitive(stream, c);
#else



            //int len = encoding.GetByteCount(value) * 2;


            var buf = new byte[value.Length * 2];

            int len = buf.Length;
            // 
#if !NO_UNSAFE && FALSE
            fixed (void* ptr = value)
            {
                System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), buf, 0, len);
            }

#else
            len = encoding.GetBytes(value, 0, value.Length, buf, 0);
#endif
            WritePrimitive(stream, (uint)len);
            stream.Write(buf, 0, len);
#endif
        }

	    public 
#if !NO_UNSAFE
	        unsafe
#endif
	        void ReadPrimitive(Stream stream, out string value)
	    {
	        uint len;
	        ReadPrimitive(stream, out len);

	        if (len == 0)
	        {
	            value = "";
	            return;
	        }
	        else if (len == 1)
	        {
	            //value = string.Empty;
	            //return;
	        }

	        //len -= 1;

#if GC_NICE_VERSION
			var arr = new char[len];
			for (uint i = 0; i < len; ++i)
				ReadPrimitive(stream, out arr[i]);

			value = new string(arr);
#else

	        // encoding = new UTF8Encoding(false, true);

	        var buf = new byte[len];

	        int l = 0;

	        while (l < len)
	        {
	            int r = stream.Read(buf, l, (int) len - l);
	            if (r == 0)
	                throw new EndOfStreamException();
	            l += r;
	        }



#if !NO_UNSAFE && FALSE
	        int offset = 0;
	 
	        fixed (byte* bptr = buf)
	        {
	            char* cptr = (char*) (bptr + offset);
	            value = new string(cptr, 0, (int)len / 2);
	        }
#else
            value = encoding.GetString(buf);
#endif
#endif
	    }

	    public  void WritePrimitive(Stream stream, byte[] value)
		{
			if (value == null)
			{
				WritePrimitive(stream, (uint)0);
				return;
			}

			WritePrimitive(stream, (uint)value.Length + 1);

			stream.Write(value, 0, value.Length);
		}

		 readonly byte[] s_emptyByteArray = new byte[0];

		public  void ReadPrimitive(Stream stream, out byte[] value)
		{
			uint len;
			ReadPrimitive(stream, out len);

			if (len == 0)
			{
				value = null;
				return;
			}
			else if (len == 1)
			{
				value = s_emptyByteArray;
				return;
			}

			len -= 1;

			value = new byte[len];
			int l = 0;

			while (l < len)
			{
				int r = stream.Read(value, l, (int)len - l);
				if (r == 0)
					throw new EndOfStreamException();
				l += r;
			}
		}
        /*

		public  void WritePrimitive<TKey, TValue>(Stream stream, Dictionary<TKey, TValue> value)
		{
			var kvpArray = new KeyValuePair<TKey, TValue>[value.Count];

			int i = 0;
			foreach (var kvp in value)
				kvpArray[i++] = kvp;

			NetSerializer.Serializer.Serialize(stream, kvpArray);
		}

		public  void ReadPrimitive<TKey, TValue>(Stream stream, out Dictionary<TKey, TValue> value)
		{
			var kvpArray = (KeyValuePair<TKey, TValue>[])NetSerializer.Serializer.Deserialize(stream);

			value = new Dictionary<TKey, TValue>(kvpArray.Length);

			foreach (var kvp in kvpArray)
				value.Add(kvp.Key, kvp.Value);
		}
        */
	}
}
