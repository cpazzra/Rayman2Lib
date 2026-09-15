using System.IO;
namespace Rayman2Lib
{
    public static class ExtenstionMethods
    {
        public static string ReadNullTermString(this BinaryReader r)
        {
            var str = "";
            char c;
            while ((c = r.ReadChar()) != 0x00)
                str += c;
            return str;
        }

        public static string ReadNullTermStringWithLength(this BinaryReader r)
        {
            var length = r.ReadInt16();
            var str = "";
            char c;
            int len = 0;
            while (len++ < length && (c = r.ReadChar()) != 0x00)
                str += c;
            r.ReadBytes(length - len);
            return str;
        }

        public static string ReadNullTermStringWithLength(this BinaryReader r, int length)
        {
            var str = "";
            char c;
            int len = 0;
            while (len++ < length && (c = r.ReadChar()) != 0x00)
                str += c;
            r.ReadBytes(length - len);
            return str;
        }
    }
}
