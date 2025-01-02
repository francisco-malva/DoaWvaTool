using System.Text;

namespace DoaWVATool;

public static class BinaryUtils {

    public static T ReadUnmanaged<T>(this BinaryReader binaryReader, int? size = null) where T : unmanaged
    {
        unsafe
        {
            var sz = size ?? sizeof(T);
            Span<byte> span = stackalloc byte[sz];

            fixed (byte* ptr = span)
            {
                binaryReader.Read(span);
                return *(T*)ptr;
            }
        }

    }
    public static void WriteUnmanaged<T>(this BinaryWriter binaryWriter, T val) where T : unmanaged
    {
        unsafe
        {
            Span<byte> span = stackalloc byte[sizeof(T)];

            fixed (byte* ptr = span)
            {
                Buffer.MemoryCopy(&val, ptr, sizeof(T), sizeof(T));
            }

            binaryWriter.Write(span);
        }
    }


    public static void Write4Cc(this BinaryWriter writer, string riffStr)
    {
        Span<byte> riffBytes = stackalloc byte[riffStr.Length];
        Encoding.ASCII.GetBytes(riffStr, riffBytes);
        writer.Write(riffBytes);
    }


    public static string Read4Cc(this BinaryReader reader)
    {
        Span<byte> riffBytes = stackalloc byte[4];
        reader.Read(riffBytes);

        return Encoding.ASCII.GetString(riffBytes);
    }
}