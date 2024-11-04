using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MemScan
{
    public static class MemoryScanner
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static List<(string Value, IntPtr Address)> ScanProcessForStrings(Process process)
        {
            var results = new List<(string Value, IntPtr Address)>();

            foreach (ProcessModule module in process.Modules)
            {
                IntPtr address = module.BaseAddress;
                long moduleSize = module.ModuleMemorySize;

                for (long i = 0; i < moduleSize; i += 4096)
                {
                    var buffer = new byte[4096];
                    if (ReadProcessMemory(process.Handle, address + (int)i, buffer, buffer.Length, out int bytesRead))
                    {
                        string text = Encoding.UTF8.GetString(buffer);
                        foreach (var line in text.Split('\0'))
                        {
                            if (!string.IsNullOrWhiteSpace(line) && line.Length >= 4)
                            {
                                results.Add((line, address + (int)i));
                            }
                        }
                    }
                }
            }

            return results;
        }
    }
}