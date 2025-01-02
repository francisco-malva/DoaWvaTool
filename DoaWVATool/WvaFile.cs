using System.Runtime.InteropServices;
using System.Text.Json;

namespace DoaWVATool
{
    internal class WvaFile
    {
        public readonly List<SoundEffect?> SoundEffects = [];
        public string Name { get; private init; } = string.Empty;



        public WvaFile()
        {

        }
        private WvaFile(BinaryReader br)
        {
            var soundEffectCount = br.ReadInt32();


            for (var i = 0; i < soundEffectCount; i++)
            {
                var offset = br.ReadInt32();

                if (offset == 0)
                {
                    SoundEffects.Add(null);
                    continue;
                }

               

                var curOffset = br.BaseStream.Position;

                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                SoundEffects.Add(SoundEffect.FromPackedWva(br));
                br.BaseStream.Seek(curOffset, SeekOrigin.Begin);
            }
        }


        public void Pack(BinaryWriter bw)
        {
            Console.WriteLine($"Packing {SoundEffects.Count} sound effects...");
            bw.Write(SoundEffects.Count);


            for (var i = 0; i < SoundEffects.Count; i++)
            {
                bw.Write(0);
            }

            for (var i = 0; i < SoundEffects.Count; i++)
            {
                bw.BaseStream.Seek(0, SeekOrigin.End);

                var writtenOffset = SoundEffects[i]?.Write(bw) ?? 0;

                bw.BaseStream.Position = 4 + i * 4;
                bw.Write((int)writtenOffset);


                Console.WriteLine(writtenOffset != 0 ? $"Packed sound effect {i} into WVA!" : $"The manifest contained no file for slot {i}, putting in blank entry (this is safe, the game is programmed to ignore it).");
            }
        }


        public void PackToFile(string path)
        {
            using var binaryWriter = new BinaryWriter(File.Create(path));
            Pack(binaryWriter);
        }


        public void UnpackToDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var manifest = new string?[SoundEffects.Count];

            for (var i = 0; i < SoundEffects.Count; ++i)
            {
                var fullPath = $"{path}/{Name}_{i + 1}.wav";

                SoundEffects[i]?.WriteToWavFile(fullPath);
                manifest[i] = SoundEffects[i] == null ? null : Path.GetRelativePath(path, fullPath);


                Console.WriteLine(manifest[i] == null ? $"Unpacked sound effect from slot {i} into {fullPath}" : $"Slot {i}contained no sound effect, skipping...");
            }

            File.WriteAllText($"{path}/{Name}.json", JsonSerializer.Serialize(manifest,
                new JsonSerializerOptions(JsonSerializerOptions.Default)
                {
                    WriteIndented = true
                }));
        }

        public static WvaFile FromFile(string path)
        {
            using var binaryReader = new BinaryReader(File.OpenRead(path));

            var fileInfo = new FileInfo(path);

            var wvaFile = new WvaFile(binaryReader)
            {
                Name = Path.GetFileNameWithoutExtension(path)
            };

            return wvaFile;
        }

        public static WvaFile FromManifest(string manifestPath)
        {

            var wvaFile = new WvaFile();

            var manifestDir = Path.GetFullPath(Path.GetDirectoryName(manifestPath));

            var soundEffects = JsonSerializer.Deserialize<string?[]>(File.ReadAllText(manifestPath));


            foreach (var soundEffectPath in soundEffects)
            {
                wvaFile.SoundEffects.Add(soundEffectPath == null
                    ? null
                    : SoundEffect.FromWavFile(Path.GetFullPath(soundEffectPath, manifestDir)));
            }

            return wvaFile;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WavFormatData
    {
        public WaveFormatEx WaveFormat;
        public ushort ExtraData;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SoundEffectHeader
    {
        [FieldOffset(0)] public int DataSize;
        [FieldOffset(4)] public WavFormatData WaveFormatData;
        [FieldOffset(4)] public unsafe fixed byte WaveFormatDataBuffer[28];
    }

    internal class SoundEffect
    {
        private SoundEffectHeader _header;
        private byte[] _data;
        public string Name;



        public static SoundEffect FromPackedWva(BinaryReader br)
        {
            var se = new SoundEffect
            {
                _header = br.ReadUnmanaged<SoundEffectHeader>()
            };

            se._data = br.ReadBytes(se._header.DataSize);

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
                        unsafe
                        {
                            var sz = br.ReadInt32();
                            fixed (byte* ptr = se._header.WaveFormatDataBuffer)
                            {
                                _ = br.Read(new Span<byte>(ptr, sz));
                            }
                            break;
                        }
                    case "data":
                        se._header.DataSize = br.ReadInt32();
                        se._data = br.ReadBytes(se._header.DataSize);
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
            var begin = bw.BaseStream.Position;
            bw.Write4Cc("RIFF");
            bw.Write(0);

            bw.Write4Cc("WAVE");
            bw.Write4Cc("fmt ");
            bw.Write(28);
            unsafe
            {
                fixed (byte* ptr = _header.WaveFormatDataBuffer)
                {
                    bw.Write(new Span<byte>(ptr, 28));
                }
                
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

        public long Write(BinaryWriter bw)
        {
            var offset = bw.BaseStream.Position;
            bw.WriteUnmanaged(_header);
            bw.Write(_data);

            return offset;
        }
    }
}
