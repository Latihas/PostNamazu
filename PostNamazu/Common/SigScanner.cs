using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using PostNamazu.Common.Localization;

namespace PostNamazu.Common;

public class SigScanner {
	private readonly MemHelper _memhelper;
	public readonly uint SizeOfCode;
	public readonly uint CodeBase;
	public readonly uint _dataLength;
	public readonly byte[] _data;
	public readonly IntPtr _baseAddress;
	public IntPtr TextSectionBase => new(_baseAddress.ToInt64() + TextSectionOffset);
	public long TextSectionOffset { get; private set; }
	public int TextSectionSize { get; private set; }
	public IntPtr DataSectionBase => new(_baseAddress.ToInt64() + DataSectionOffset);
	public long DataSectionOffset { get; set; }
	public int DataSectionSize { get; set; }

	private void SetupSearchSpace(ProcessModule module) {
		var baseAddress = module.BaseAddress;

		// We don't want to read all of IMAGE_DOS_HEADER or IMAGE_NT_HEADER stuff so we cheat here.
		var ntNewOffset = ReadInt32(baseAddress, 0x3C);
		var ntHeader = baseAddress + ntNewOffset;

		// IMAGE_NT_HEADER
		var fileHeader = ntHeader + 4;
		var numSections = ReadInt16(ntHeader, 6);

		// IMAGE_OPTIONAL_HEADER
		var optionalHeader = fileHeader + 20;

		var sectionHeader = optionalHeader + 240;

		// IMAGE_SECTION_HEADER
		var sectionCursor = sectionHeader;
		for (var i = 0; i < numSections; i++) {
			var sectionName = ReadInt64(sectionCursor);

			// .text
			switch (sectionName) {
				case 0x747865742E: // .text
					ReadInt32(sectionCursor, 12);
					ReadInt32(sectionCursor, 8);
					break;
				case 0x617461642E: // .data
					DataSectionOffset = ReadInt32(sectionCursor, 12);
					DataSectionSize = ReadInt32(sectionCursor, 8);
					break;
			}

			sectionCursor += 40;
		}
	}

	#region structs

	[StructLayout(LayoutKind.Sequential)] private struct IMAGE_DOS_HEADER {
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public char[] e_magic; // Magic number
		public UInt16 e_cblp; // Bytes on last page of file
		public UInt16 e_cp; // Pages in file
		public UInt16 e_crlc; // Relocations
		public UInt16 e_cparhdr; // Size of header in paragraphs
		public UInt16 e_minalloc; // Minimum extra paragraphs needed
		public UInt16 e_maxalloc; // Maximum extra paragraphs needed
		public UInt16 e_ss; // Initial (relative) SS value
		public UInt16 e_sp; // Initial SP value
		public UInt16 e_csum; // Checksum
		public UInt16 e_ip; // Initial IP value
		public UInt16 e_cs; // Initial (relative) CS value
		public UInt16 e_lfarlc; // File address of relocation table
		public UInt16 e_ovno; // Overlay number
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public UInt16[] e_res1; // Reserved words
		public UInt16 e_oemid; // OEM identifier (for e_oeminfo)
		public UInt16 e_oeminfo; // OEM information; e_oemid specific
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		public UInt16[] e_res2; // Reserved words
		public Int32 e_lfanew; // File address of new exe header

		private string EMagic => new(e_magic);

		public bool IsValid => EMagic == "MZ";
	}


	[StructLayout(LayoutKind.Sequential)] private struct IMAGE_FILE_HEADER {
		public UInt16 Machine;
		public UInt16 NumberOfSections;
		public UInt32 TimeDateStamp;
		public UInt32 PointerToSymbolTable;
		public UInt32 NumberOfSymbols;
		public UInt16 SizeOfOptionalHeader;
		public UInt16 Characteristics;
	}


	[StructLayout(LayoutKind.Explicit)] private struct IMAGE_NT_HEADERS64 {
		[FieldOffset(0)] public uint Signature;

		[FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;

		[FieldOffset(24)] public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
	}

	[StructLayout(LayoutKind.Explicit)] private struct IMAGE_OPTIONAL_HEADER64 {
		[FieldOffset(0)] public MagicType Magic;

		[FieldOffset(2)] public byte MajorLinkerVersion;

		[FieldOffset(3)] public byte MinorLinkerVersion;

		[FieldOffset(4)] public uint SizeOfCode;

		[FieldOffset(8)] public uint SizeOfInitializedData;

		[FieldOffset(12)] public uint SizeOfUninitializedData;

		[FieldOffset(16)] public uint AddressOfEntryPoint;

		[FieldOffset(20)] public uint BaseOfCode;

		[FieldOffset(24)] public ulong ImageBase;

		[FieldOffset(32)] public uint SectionAlignment;

		[FieldOffset(36)] public uint FileAlignment;

		[FieldOffset(40)] public ushort MajorOperatingSystemVersion;

		[FieldOffset(42)] public ushort MinorOperatingSystemVersion;

		[FieldOffset(44)] public ushort MajorImageVersion;

		[FieldOffset(46)] public ushort MinorImageVersion;

		[FieldOffset(48)] public ushort MajorSubsystemVersion;

		[FieldOffset(50)] public ushort MinorSubsystemVersion;

		[FieldOffset(52)] public uint Win32VersionValue;

		[FieldOffset(56)] public uint SizeOfImage;

		[FieldOffset(60)] public uint SizeOfHeaders;

		[FieldOffset(64)] public uint CheckSum;

		[FieldOffset(68)] public SubSystemType Subsystem;

		[FieldOffset(70)] public DllCharacteristicsType DllCharacteristics;

		[FieldOffset(72)] public ulong SizeOfStackReserve;

		[FieldOffset(80)] public ulong SizeOfStackCommit;

		[FieldOffset(88)] public ulong SizeOfHeapReserve;

		[FieldOffset(96)] public ulong SizeOfHeapCommit;

		[FieldOffset(104)] public uint LoaderFlags;

		[FieldOffset(108)] public uint NumberOfRvaAndSizes;

		[FieldOffset(112)] public IMAGE_DATA_DIRECTORY ExportTable;

		[FieldOffset(120)] public IMAGE_DATA_DIRECTORY ImportTable;

		[FieldOffset(128)] public IMAGE_DATA_DIRECTORY ResourceTable;

		[FieldOffset(136)] public IMAGE_DATA_DIRECTORY ExceptionTable;

		[FieldOffset(144)] public IMAGE_DATA_DIRECTORY CertificateTable;

		[FieldOffset(152)] public IMAGE_DATA_DIRECTORY BaseRelocationTable;

		[FieldOffset(160)] public IMAGE_DATA_DIRECTORY Debug;

		[FieldOffset(168)] public IMAGE_DATA_DIRECTORY Architecture;

		[FieldOffset(176)] public IMAGE_DATA_DIRECTORY GlobalPtr;

		[FieldOffset(184)] public IMAGE_DATA_DIRECTORY TLSTable;

		[FieldOffset(192)] public IMAGE_DATA_DIRECTORY LoadConfigTable;

		[FieldOffset(200)] public IMAGE_DATA_DIRECTORY BoundImport;

		[FieldOffset(208)] public IMAGE_DATA_DIRECTORY IAT;

		[FieldOffset(216)] public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

		[FieldOffset(224)] public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

		[FieldOffset(232)] public IMAGE_DATA_DIRECTORY Reserved;
	}

	[StructLayout(LayoutKind.Sequential)] private struct IMAGE_DATA_DIRECTORY {
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}

	private enum MagicType : ushort;

	private enum SubSystemType : ushort;

	private enum DllCharacteristicsType : ushort;

	#endregion

	public T Read<T>(IntPtr address) where T : struct => _memhelper.Read<T>(address);

	public T[] Read<T>(IntPtr address, int count) where T : struct => _memhelper.Read<T>(address, count);

	public byte ReadByte(IntPtr address, int offset = 0) => _memhelper.Read<byte>(IntPtr.Add(address, offset));
	public short ReadInt16(IntPtr address, int offset = 0) => _memhelper.Read<short>(IntPtr.Add(address, offset));
	public int ReadInt32(IntPtr address, int offset = 0) => _memhelper.Read<int>(IntPtr.Add(address, offset));
	public long ReadInt64(IntPtr address, int offset = 0) => _memhelper.Read<long>(IntPtr.Add(address, offset));
	public IntPtr ReadIntPtr(IntPtr address, int offset = 0) => _memhelper.Read<IntPtr>(IntPtr.Add(address, offset));

	public SigScanner() {
		_memhelper = new MemHelper();
		SetupSearchSpace(_memhelper.target.MainModule!);
		_baseAddress = _memhelper.BaseAddress;

		var dosHeaders = _memhelper.Read<IMAGE_DOS_HEADER>(_baseAddress);
		if (!dosHeaders.IsValid) return;
		var ntHeaders = _memhelper.Read<IMAGE_NT_HEADERS64>(_baseAddress + dosHeaders.e_lfanew);
		SizeOfCode = ntHeaders.OptionalHeader.SizeOfCode;
		CodeBase = ntHeaders.OptionalHeader.BaseOfCode;
		_dataLength = CodeBase + SizeOfCode;
		_data = _memhelper.ReadBytes(_baseAddress, (int)_dataLength);
	}

	public T ScanText<T>(string pattern, Func<IntPtr, T> visitor, string name = null) => visitor(ScanText(pattern, name));

	/// <summary>
	///     使用签名字符串扫描内存，并返回唯一匹配的位置指针，格式详见 <see cref="SigPatternInfo(string)" /> 。<br /><br />
	///     如果需要手动指定相对偏移或指令长度，请使用重载版本 <see cref="ScanText(SigPatternInfo, string)" />。<br /><br />
	/// </summary>
	/// <param name="pattern">
	///     签名字符串，格式详见 <see cref="SigPatternInfo" />。
	/// </param>
	/// <param name="name">
	///     （可选）该签名的名称，用于调试或报错信息中显示。
	/// </param>
	/// <returns>匹配到的内存地址指针（如启用相对寻址，则为计算后的地址）。</returns>
	public IntPtr ScanText(string pattern, string? name = null)
		=> ScanText(new SigPatternInfo(pattern), name);


	public IntPtr ScanText(SigPatternInfo sig, string? name = null) {
		var results = FindPattern(sig.Bytes);
		switch (results.Count) {
			case > 1:
				throw new ArgumentException(L.Get("PostNamazu/resultMultiple",
					name == null ? "" : $" {name} ",
					results.Count
				));
			case 0:
				throw new ArgumentException(L.Get("PostNamazu/resultNone",
					name == null ? "" : $" {name} "
				));
		}
		var patternPtr = results[0];
		if (sig.IsRelAddressing) // 指定相对寻址
		{
			var disp32Ptr = patternPtr + sig.Disp32Offset; // 第一个 * 的地址
			patternPtr = patternPtr + sig.NextCmdOffset + ReadInt32(disp32Ptr);
		}
#if DEBUG
		PostNamazu.Plugin.PluginUi.Log($"[Scanner] {name ?? ""} @ {patternPtr} ({sig})");
#endif
		return patternPtr;
	}

	public readonly struct SigPatternInfo {
		private readonly string Pattern;
		/// <summary> 内存签名的字节序列对应的整数值。通配符 ? 和 * 分别以 -1 和 -2 表示。 </summary>
		public readonly List<int> Bytes;
		/// <summary> 是否为相对寻址。 </summary>
		public readonly bool IsRelAddressing;
		/// <summary> 相对寻址的 disp32（即 * 的起始处）相对于内存签名起始处的偏移。</summary>
		public readonly int Disp32Offset;
		/// <summary> 相对寻址的下一条指令（即 * 后的指令）相对于内存签名起始处的偏移。</summary>
		public readonly int NextCmdOffset;


		public SigPatternInfo(string hexPattern) {
			Pattern = hexPattern;
			Bytes = hexPattern.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries).Select(s => {
				return s switch {
					"*" or "**" => -2,
					"?" or "??" => -1,
					_ => byte.Parse(s, NumberStyles.AllowHexSpecifier)
				};
			}).ToList();

			// relative addressing
			var firstStar = Bytes.IndexOf(-2);
			if (firstStar >= 0) {
				var lastStar = Bytes.LastIndexOf(-2);
				var starLength = lastStar - firstStar + 1;
				if (starLength < 4 || Bytes.GetRange(firstStar, starLength).Any(b => b != -2)) {
					throw new FormatException(L.Get("PostNamazu/relAddressingFormatError", hexPattern));
				}
				IsRelAddressing = true;
				Disp32Offset = firstStar;
				NextCmdOffset = lastStar + 1;
			} else {
				IsRelAddressing = false;
				Disp32Offset = 0;
				NextCmdOffset = 0;
			}
		}


		public override string ToString() => IsRelAddressing
			? $"{Pattern}, Disp32Offset={Disp32Offset}, NextCmdOffset={NextCmdOffset}"
			: Pattern;
	}

	public List<IntPtr> FindPattern(List<int> pattern) {
		var results = Find(pattern);
		for (var i = 0; i < results.Count; i++) {
			results[i] = _baseAddress + (int)results[i];
		}
		return results;
	}

	private List<IntPtr> Find(List<int> pattern) {
		var ret = new List<IntPtr>();
		var plen = (uint)pattern.Count;
		var dataLength = _dataLength - plen;
		for (var i = CodeBase; i < dataLength; i++) {
			if (ByteMatch(_data, (int)i, pattern))
				ret.Add((IntPtr)i);
		}
		return ret;
	}

	private static bool ByteMatch(byte[] bytes, int start, List<int> pattern) {
		for (int i = start, j = 0; j < pattern.Count; i++, j++) {
			if (pattern[j] < 0)
				continue;

			if (bytes[i] != pattern[j])
				return false;
		}
		return true;
	}

	/// <summary>
	///     Scan for a .data address using a .text function.
	///     This is intended to be used with IDA sigs.
	///     Place your cursor on the line calling a static address, and create and IDA sig.
	/// </summary>
	/// <param name="signature">The signature of the function using the data.</param>
	/// <param name="offset">The offset from function start of the instruction using the data.</param>
	/// <param name="name"></param>
	/// <returns>An IntPtr to the static memory location.</returns>
	[Obsolete("Use relative addressing sigcodes.")]
	public IntPtr GetStaticAddressFromSig(string signature, int offset = 0, string? name = null) {
		var instrAddr = ScanText(signature, name);
		instrAddr = IntPtr.Add(instrAddr, offset);
		var bAddr = (long)_baseAddress;
		long num;
		do {
			instrAddr = IntPtr.Add(instrAddr, 1);
			num = ReadInt32(instrAddr) + (long)instrAddr + 4 - bAddr;
		} while (!(num >= DataSectionOffset && num <= DataSectionOffset + DataSectionSize));
		return IntPtr.Add(instrAddr, ReadInt32(instrAddr) + 4);
	}
}