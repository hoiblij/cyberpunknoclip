using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace Cyberpunk_trainer
{
    public static class MemoryFunctions
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        public static IntPtr GetDMAAddy(IntPtr hProc, IntPtr baseAddr, IntPtr[] offsets)
        {
            unsafe
            {
                byte[] buffer = new byte[sizeof(IntPtr)];
                IntPtr bytesRead;

                IntPtr addr = baseAddr;

                foreach (IntPtr offset in offsets)
                {
                    ReadProcessMemory(hProc, addr, buffer, buffer.Length, out bytesRead);
                    long pAddr = BitConverter.ToInt64(buffer, 0);
                    addr = new IntPtr((long)offset + pAddr);
                }

                return addr;
            }
        }

        public static void WriteMem(IntPtr hProc, IntPtr address, float v)
        {
            byte[] val = BitConverter.GetBytes(v);
            int e = 0;
            WriteProcessMemory(hProc, address, val, (uint)val.Length, out e);
        }

        public static void WriteMem(IntPtr hProc, IntPtr address, double v)
        {
            byte[] val = BitConverter.GetBytes(v);
            int e = 0;
            WriteProcessMemory(hProc, address, val, (uint)val.Length, out e);
        }

        public static float ReadMem(IntPtr hProc, IntPtr address)
        {
            byte[] buffer = new byte[sizeof(float)];
            IntPtr bytesRead;
            ReadProcessMemory(hProc, address, buffer, buffer.Length, out bytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static double ReadMemDouble(IntPtr hProc, IntPtr address)
        {
            byte[] buffer = new byte[sizeof(double)];
            IntPtr bytesRead;
            ReadProcessMemory(hProc, address, buffer, buffer.Length, out bytesRead);
            return BitConverter.ToDouble(buffer, 0);
        }
    }
}
