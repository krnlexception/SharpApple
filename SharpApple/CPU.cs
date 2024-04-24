using System.Runtime.InteropServices;

namespace SharpApple;

/*
// cpu interface to 6502.dll
[StructLayout(LayoutKind.Sequential)]
public struct Z6502State
{
    public UInt16 pc;
    public byte s, p, a, x, y;
    public struct _internal
    {
        public byte irq;
        public byte nmi;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct M6502
{
    public nuint cycles;
    public void* context;
    public nint read;
    public nint write;
    public Z6502State state;
    public ushort opcode;
    public ushort ea_cycles;
    public UInt16 ea;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void write(nint context, ushort address, byte value);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate byte read(nint context, ushort address);

public static class Cpu
{
    [DllImport("6502")]
    public static extern void m6502_power(ref M6502 cpuobject, bool state);
    [DllImport("6502")]
    public static extern void m6502_reset(ref M6502 cpuobject);
    [DllImport("6502")]
    public static extern void m6502_run(ref M6502 cpuobject, nuint cycles);
    [DllImport("6502")]
    public static extern void m6502_nmi(ref M6502 cpuobject);
    [DllImport("6502")]
    public static extern void m6502_irq(ref M6502 cpuobject, bool state);
}*/