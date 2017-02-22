using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDisasm;
using SharpDisasm.Udis86;

namespace Tera.Analytics
{
    /// <summary>
    ///     When injected in TERA's process, scans for hardcoded data such as the Data Center's AES key/IV.
    /// </summary>
    internal class MemoryScanner
    {
        private IntPtr moduleEntryPoint;
        private uint moduleSize;

        /// <summary>
        ///     Scans the memory of the current process.
        /// </summary>
        /// <returns>The results of the analysis.</returns>
        public AnalysisResult Analyze()
        {
            (IntPtr, uint) GetMainModuleInfo()
            {
                var process = Process.GetCurrentProcess();
                var moduleHandle = GetModuleHandle($"{process.ProcessName}.exe");
                GetModuleInformation(
                    process.Handle,
                    moduleHandle,
                    out var moduleInfo,
                    (uint) Marshal.SizeOf<MODULEINFO>());
                return (moduleInfo.lpBaseOfDll, moduleInfo.SizeOfImage);
            }

            (moduleEntryPoint, moduleSize) = GetMainModuleInfo();
            var keys = GetDataCenterKeys();
            var systemMessageIds = GetSystemMessageReadableIds();
            var packetNamesByOpcode = GetPacketNames();
            return new AnalysisResult
            {
                DataCenterIv = keys.iv,
                DataCenterKey = keys.key,
                PacketNamesByOpcode = packetNamesByOpcode,
                SystemMessageReadableIds = systemMessageIds
            };
        }

        private (byte[] key, byte[] iv) GetDataCenterKeys()
        {
            var pattern = Convert.FromBase64String("jUX0ZKMAAAAAi3MIi84=");
            var keysAddress = FindAddress(pattern);
            var disassembler = new Disassembler(
                keysAddress,
                (int) moduleSize,
                ArchitectureMode.x86_32,
                (ulong) moduleEntryPoint);

            byte[] Read128BitKey()
            {
                const int keyLengthInBytes = 16;
                var keyStream = new MemoryStream(keyLengthInBytes);
                var writer = new BinaryWriter(keyStream);
                while (keyStream.Position < keyLengthInBytes)
                {
                    var instruction = disassembler.NextInstruction();
                    //If it's MOV [EBP-x], y then it's part of the key. There are usually other unrelated instructions in between.
                    if (instruction.Mnemonic == ud_mnemonic_code.UD_Imov &&
                        instruction.Operands[0].Base == ud_type.UD_R_EBP)
                        writer.Write(instruction.Operands[1].LvalUDWord);
                }
                return keyStream.ToArray();
            }

            using (disassembler)
            {
                return (Read128BitKey(), Read128BitKey());
            }
        }

        private List<string> GetSystemMessageReadableIds()
        {
            var pattern = Convert.FromBase64String("VYvsi0UIhcB4ED0=");
            var routineAddress = FindAddress(pattern);
            var routine = Marshal.GetDelegateForFunctionPointer<SystemMessageTypesResolverDelegate>(routineAddress);
            var systemMessageTypesCount = Marshal.ReadInt32(routineAddress + pattern.Length);
            var systemMessageReadableIds = Enumerable.Range(0, systemMessageTypesCount)
                .Select(id => routine(id))
                .ToList();
            return systemMessageReadableIds;
        }

        private Dictionary<ushort, string> GetPacketNames()
        {
            var pattern = Convert.FromBase64String("VYvsi0UID7fAPYgTAAB/bXRk");
            var routineAddress = FindAddress(pattern);
            var routine = Marshal.GetDelegateForFunctionPointer<PacketNamesResolverDelegate>(routineAddress);
            var packetNamesByOpcode = Enumerable.Range(0, ushort.MaxValue)
                .Select(opcode => new {Opcode = (ushort) opcode, Name = routine(opcode)})
                .Where(packet => !string.IsNullOrWhiteSpace(packet.Name))
                .ToDictionary(packet => packet.Opcode, packet => packet.Name);
            return packetNamesByOpcode;
        }

        private IntPtr FindAddress(byte[] needle)
        {
            var needleLength = needle.Length;
            for (var i = (int) moduleEntryPoint; i < moduleSize - needleLength; i++)
            {
                var address = (IntPtr) i;
                var k = 0;
                for (; k < needleLength; k++)
                    if (needle[k] != Marshal.ReadByte(address + k)) break;
                if (k == needleLength) return address;
            }
            throw new Exception($"Pattern not found: {BitConverter.ToString(needle)}");
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool GetModuleInformation(
            IntPtr hProcess,
            IntPtr hModule,
            out MODULEINFO lpmodinfo,
            uint cb);

        //The system message type names are stored in memory as null-terminated Unicode strings.
        [return: MarshalAs(UnmanagedType.LPWStr)]
        private delegate string SystemMessageTypesResolverDelegate(int id);

        private delegate string PacketNamesResolverDelegate(int opcode);

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private struct MODULEINFO
        {
            public readonly IntPtr lpBaseOfDll;
            public readonly uint SizeOfImage;
            private readonly IntPtr EntryPoint;
        }
    }
}