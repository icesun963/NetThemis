using System;
using System.IO;

namespace NetThemis
{
    public interface IDataPrimitives
    {
        string ReadString(Stream stream);
        int ReadInt(Stream stream);
        bool ReadBool(Stream stream);
        byte ReadByte(Stream stream);
        byte[] ReadBytes(Stream stream);
        char ReadChar(Stream stream);
        DateTime ReadDateTime(Stream stream);
        double ReadDouble(Stream stream);
        long ReadLong(Stream stream);
        float ReadFloat(Stream stream);
        SByte ReadSbyte(Stream stream);
        short ReadShort(Stream stream);
        UInt16 ReadUint(Stream stream);
        ulong ReadUlong(Stream stream);
        ushort ReadUshort(Stream stream);
        void WritePrimitive(Stream stream, bool value);
        void WritePrimitive(Stream stream, byte value);
        void WritePrimitive(Stream stream, sbyte value);
        void WritePrimitive(Stream stream, char value);
        void WritePrimitive(Stream stream, ushort value);
        void WritePrimitive(Stream stream, short value);
        void WritePrimitive(Stream stream, uint value);
        void WritePrimitive(Stream stream, int value);
        void WritePrimitive(Stream stream, ulong value);
        void WritePrimitive(Stream stream, long value);
        unsafe void WritePrimitive(Stream stream, float value);
        unsafe void WritePrimitive(Stream stream, double value);
        void WritePrimitive(Stream stream, DateTime value);

        #if !NO_UNSAFE
            unsafe
#endif
            void WritePrimitive(Stream stream, string value);

        void WritePrimitive(Stream stream, byte[] value);
    }
}