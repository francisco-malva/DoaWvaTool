using System.Runtime.InteropServices;

namespace DoaWVATool.Wva.Se;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SoundEffectHeader
{
    public int DataSize;
    public WavFormatData WaveFormatData;
    public uint LoopStart;
    public uint LoopEnd;


    public double LoopStartSeconds
    {
        get => LoopStart / (double)WaveFormatData.WaveFormat.nSamplesPerSec;
        set => LoopStart = (uint)(value * WaveFormatData.WaveFormat.nSamplesPerSec);
    }

    public double LoopEndSeconds
    {
        get => LoopEnd / (double)WaveFormatData.WaveFormat.nSamplesPerSec;
        set => LoopEnd = (uint)(value * WaveFormatData.WaveFormat.nSamplesPerSec);
    }
}