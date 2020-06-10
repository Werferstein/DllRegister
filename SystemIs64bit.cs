//https://stackoverflow.com/questions/336633/how-to-detect-windows-64-bit-platform-with-net
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DllRegister
{
    public class SystemUtil
    {
        static bool is64BitProcess = (IntPtr.Size == 8);
        static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        [DllImport("fusion.dll")]
        private static extern IntPtr CreateAssemblyCache(out IAssemblyCache ppAsmCache,int reserved);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            int Dummy1();

            [PreserveSig()]
            IntPtr QueryAssemblyInfo(
                int flags,
                [MarshalAs(UnmanagedType.LPWStr)] string assemblyName,
                ref AssemblyInfo assemblyInfo);

            int Dummy2();
            int Dummy3();
            int Dummy4();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AssemblyInfo
        {
            public int cbAssemblyInfo;
            public int assemblyFlags;
            public long assemblySizeInKB;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string currentAssemblyPath;

            public int cchBuf;
        }

        public static bool IsAssemblyInGAC(string assemblyName)
        {
            var assembyInfo = new AssemblyInfo { cchBuf = 512 };
            assembyInfo.currentAssemblyPath = new string('\0', assembyInfo.cchBuf);

            IAssemblyCache assemblyCache;

            var hr = CreateAssemblyCache(out assemblyCache, 0);

            if (hr == IntPtr.Zero)
            {
                hr = assemblyCache.QueryAssemblyInfo(
                    1,
                    assemblyName,
                    ref assembyInfo);

                if (hr != IntPtr.Zero)
                {
                    return false;
                }

                return true;
            }

            Marshal.ThrowExceptionForHR(hr.ToInt32());
            return false;
        }
    }

}
