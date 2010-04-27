/* File generated automatically; do not edit! */
/* This file can be copied, modified and distributed without restriction. */

extern char __wine_spec_pe_header[];
#ifndef __GNUC__
static void __asm__dummy_header(void) {
#endif
asm(".text\n\t"
    ".align 4096\n"
    "__wine_spec_pe_header:\t.skip 65536\n\t"
    ".data\n\t"
    ".align 4\n"
    "__wine_spec_data_start:\t.long 1");
#ifndef __GNUC__
}
#endif
extern char _end[];
extern int __wine_spec_data_start[], __wine_spec_exports[];

#define __stdcall __attribute__((__stdcall__))


static struct {
  struct {
    void        *OriginalFirstThunk;
    unsigned int TimeDateStamp;
    unsigned int ForwarderChain;
    const char  *Name;
    void        *FirstThunk;
  } imp[2];
  const char *data[8];
} imports = {
  {
    { 0, 0, 0, "kernel32.dll", &imports.data[0] },
    { 0, 0, 0, 0, 0 },
  },
  {
    /* kernel32.dll */
    "\052\001ExitProcess",
    "\224\001GetCommandLineA",
    "\003\002GetModuleHandleA",
    "\035\002GetProcAddress",
    "\066\002GetStartupInfoA",
    "\310\002LoadLibraryA",
    "\021\004VirtualFree",
    0,
  }
};

#ifndef __GNUC__
static void __asm__dummy_import(void) {
#endif

asm(".data\n\t.align 8\n"
    "\t.type ExitProcess,@function\n"
    "\t.globl ExitProcess\n"
    "ExitProcess:\n\tjmp *(imports+40)\n\tmovl %esi,%esi\n"
    "\t.type GetCommandLineA,@function\n"
    "\t.globl GetCommandLineA\n"
    "GetCommandLineA:\n\tjmp *(imports+44)\n\tmovl %esi,%esi\n"
    "\t.type GetModuleHandleA,@function\n"
    "\t.globl GetModuleHandleA\n"
    "GetModuleHandleA:\n\tjmp *(imports+48)\n\tmovl %esi,%esi\n"
    "\t.type GetProcAddress,@function\n"
    "\t.globl GetProcAddress\n"
    "GetProcAddress:\n\tjmp *(imports+52)\n\tmovl %esi,%esi\n"
    "\t.type GetStartupInfoA,@function\n"
    "\t.globl GetStartupInfoA\n"
    "GetStartupInfoA:\n\tjmp *(imports+56)\n\tmovl %esi,%esi\n"
    "\t.type LoadLibraryA,@function\n"
    "\t.globl LoadLibraryA\n"
    "LoadLibraryA:\n\tjmp *(imports+60)\n\tmovl %esi,%esi\n"
    "\t.type VirtualFree,@function\n"
    "\t.globl VirtualFree\n"
    "VirtualFree:\n\tjmp *(imports+64)\n\tmovl %esi,%esi\n"
".text");
#ifndef __GNUC__
}
#endif

static int __wine_spec_init_state;
extern int __wine_main_argc;
extern char **__wine_main_argv;
extern char **__wine_main_environ;
extern unsigned short **__wine_main_wargv;
extern void _init(int, char**, char**);
extern void _fini();

typedef struct {
    unsigned int cb;
    char *lpReserved, *lpDesktop, *lpTitle;
    unsigned int dwX, dwY, dwXSize, dwYSize;
    unsigned int dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
    unsigned short wShowWindow, cbReserved2;
    char *lpReserved2;
    void *hStdInput, *hStdOutput, *hStdError;
} STARTUPINFOA;
extern int __stdcall WinMain(void *,void *,char *,int);
extern char * __stdcall GetCommandLineA(void);
extern void * __stdcall GetModuleHandleA(char *);
extern void __stdcall GetStartupInfoA(STARTUPINFOA *);
extern void __stdcall ExitProcess(unsigned int);
static void __wine_exe_main(void)
{
    STARTUPINFOA info;
    char *cmdline = GetCommandLineA();
    int ret, bcount=0, in_quotes=0;
    while (*cmdline) {
        if ((*cmdline=='\t' || *cmdline==' ') && !in_quotes) break;
        else if (*cmdline=='\\') bcount++;
        else if (*cmdline=='\"') {
            if ((bcount & 1)==0) in_quotes=!in_quotes;
            bcount=0;
        }
        else bcount=0;
        cmdline++;
    }
    while (*cmdline=='\t' || *cmdline==' ') cmdline++;
    GetStartupInfoA( &info );
    if (!(info.dwFlags & 1)) info.wShowWindow = 1;
    if (__wine_spec_init_state == 1)
        _init( __wine_main_argc, __wine_main_argv, __wine_main_environ );
    ret = WinMain( GetModuleHandleA(0), 0, cmdline, info.wShowWindow );
    if (__wine_spec_init_state == 1) _fini();
    ExitProcess( ret );
}

static const struct image_nt_headers
{
  int Signature;
  struct file_header {
    short Machine;
    short NumberOfSections;
    int   TimeDateStamp;
    void *PointerToSymbolTable;
    int   NumberOfSymbols;
    short SizeOfOptionalHeader;
    short Characteristics;
  } FileHeader;
  struct opt_header {
    short Magic;
    char  MajorLinkerVersion, MinorLinkerVersion;
    int   SizeOfCode;
    int   SizeOfInitializedData;
    int   SizeOfUninitializedData;
    void *AddressOfEntryPoint;
    void *BaseOfCode;
    void *BaseOfData;
    void *ImageBase;
    int   SectionAlignment;
    int   FileAlignment;
    short MajorOperatingSystemVersion;
    short MinorOperatingSystemVersion;
    short MajorImageVersion;
    short MinorImageVersion;
    short MajorSubsystemVersion;
    short MinorSubsystemVersion;
    int   Win32VersionValue;
    void *SizeOfImage;
    int   SizeOfHeaders;
    int   CheckSum;
    short Subsystem;
    short DllCharacteristics;
    int   SizeOfStackReserve;
    int   SizeOfStackCommit;
    int   SizeOfHeapReserve;
    int   SizeOfHeapCommit;
    int   LoaderFlags;
    int   NumberOfRvaAndSizes;
    struct { const void *VirtualAddress; int Size; } DataDirectory[16];
  } OptionalHeader;
} nt_header = {
  0x4550,
  { 0x014c,
    0, 0, 0, 0,
    sizeof(nt_header.OptionalHeader),
    0x0000 },
  { 0x010b,
    0, 0,
    0, 0, 0,
    __wine_exe_main,
    0, __wine_spec_data_start,
    __wine_spec_pe_header,
    4096,
    4096,
    1, 0,
    0, 0,
    4, 0,
    0,
    _end,
    4096,
    0,
    0x0002,
    0,
    1048576, 4096,
    1048576, 4096,
    0,
    16,
    {
      { 0, 0 },
      { &imports, sizeof(imports) },
      { 0, 0 },
    }
  }
};

void __wine_spec_init(void)
{
    extern void __wine_dll_register( const struct image_nt_headers *, const char * );
    __wine_spec_init_state = 1;
    __wine_dll_register( &nt_header, "winelib.exe" );
}

#ifndef __GNUC__
static void __asm__dummy_dll_init(void) {
#endif
asm("\t.section\t\".init\" ,\"ax\"\n"
    "\tcall __wine_spec_init_ctor\n"
    "\t.section\t\".text\"\n");
#ifndef __GNUC__
}
#endif
void __wine_spec_init_ctor(void)
{
    if (__wine_spec_init_state) return;
    __wine_spec_init();
    __wine_spec_init_state = 2;
}
