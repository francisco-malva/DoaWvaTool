using DoaWVATool.Wva.Se;
using System.Runtime.InteropServices;

namespace DoaWVATool.Wva;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WavFormatData
{
    public WaveFormatEx WaveFormat;
    public ushort ExtraData;
}