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

                var writtenOffset = SoundEffects[i]?.WriteAsPartOfWva(bw) ?? 0;

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



        private static readonly JsonSerializerOptions JsonSettings =
            new(JsonSerializerOptions.Default)
            {
                WriteIndented = true
            };

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

            File.WriteAllText($"{path}/{Name}.json", JsonSerializer.Serialize(manifest, JsonSettings));

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

            var manifestDir = Path.GetFullPath(Path.GetDirectoryName(manifestPath) ?? throw new InvalidOperationException("Manifest path resolved to null!"));

            var soundEffects = JsonSerializer.Deserialize<string?[]>(File.ReadAllText(manifestPath)) ?? throw new InvalidOperationException("Manifest resolved to null!");


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
}
