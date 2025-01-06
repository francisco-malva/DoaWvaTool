namespace DoaWVATool.Wva.Se;

internal class SoundEffect
{
    public SoundEffectHeader Header;
    private byte[]? _data;



    public static SoundEffect FromPackedWva(BinaryReader br)
    {
        var se = new SoundEffect
        {
            Header = br.ReadUnmanaged<SoundEffectHeader>()
        };

        se._data = br.ReadBytes(se.Header.DataSize);

        return se;
    }


    public static SoundEffect FromWav(BinaryReader br)
    {
        var se = new SoundEffect();

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            switch (br.Read4Cc())
            {
                case "RIFF":
                    br.ReadInt32(); //Consume file size
                    break;
                case "WAVE":
                    //Ignore
                    break;
                case "fmt ":
                    var sz = br.ReadInt32();
                    se.Header.WaveFormatData = br.ReadUnmanaged<WavFormatData>(sz);
                    break;
                case "data":
                    se.Header.DataSize = br.ReadInt32();
                    se._data = br.ReadBytes(se.Header.DataSize);
                    break;
            }

        }


        return se;
    }

    public static SoundEffect FromWavFile(string wavFilePath)
    {
        using var br = new BinaryReader(File.OpenRead(wavFilePath));
        return FromWav(br);
    }



    public void WriteAsWav(BinaryWriter bw)
    {

        if (_data == null)
        {
            throw new ArgumentException("_data should not be null!");
        }
        var begin = bw.BaseStream.Position;
        bw.Write4Cc("RIFF");
        bw.Write(0);

        bw.Write4Cc("WAVE");
        bw.Write4Cc("fmt ");
      
        unsafe
        {
            bw.Write(sizeof(WavFormatData));
            bw.WriteUnmanaged(Header.WaveFormatData);
        }

        bw.Write4Cc("data");
        bw.Write(_data.Length);
        bw.Write(_data);
        var end = bw.BaseStream.Position;


        var length = end - begin;
        bw.BaseStream.Position = begin + 4;
        bw.Write((int)(length - 8));
        bw.BaseStream.Position = end;
    }

    public void WriteToWavFile(string path)
    {
        using var binaryWriter = new BinaryWriter(File.Create(path));
        WriteAsWav(binaryWriter);
    }

    public long WriteAsPartOfWva(BinaryWriter bw)
    {
        if (_data == null)
        {
            throw new ArgumentException("_data should not be null!");
        }
        var offset = bw.BaseStream.Position;
        bw.WriteUnmanaged(Header);
        bw.Write(_data);

        return offset;
    }
}