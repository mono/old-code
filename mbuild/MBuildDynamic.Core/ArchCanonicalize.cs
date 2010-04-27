using System;
using System.Collections;
using System.Text.RegularExpressions;

using Mono.Build; //StrUtils

namespace MBuildDynamic.Core.Native {

    internal class ArchCanonicalize {

	ArchCanonicalize () {}

	// Oh god.

	static Regex r_kernelos;
	static Regex r_firstosnoop;
	static Regex r_notosmach;
	static Regex r_sco86;
	static Regex r_sco32v;
	static Regex r_sco32dot;
	static Regex r_mint;
	static Regex r_cpuset1;
	static Regex r_pc_cpu;
	static Regex r_baremach_noop;
	static Regex r_baremach_unk;
	static Regex r_mfgmach_ok;
	static Regex r_osnoop;
	static Regex r_osnoopfull;

	static Hashtable h_machreplace;
	static Hashtable h_cpualiases;
	static Hashtable h_machosreplace;
	static Hashtable h_osreplace;
	static Hashtable h_mfgosguesses;
	static Hashtable h_cpuosguesses;

	static string[] a_startreplace;
	static string[] a_machosstartreplace;
	static string[] a_mfgalias;
	static string[] a_osstartreplace;
	static string[] a_osstartsubs;
	static string[] a_osmfgstarts;

	const string s_machreplaces = 
"386bsd i386-unknown " +
"3b1,7300,7300-att,att-7300,pc7300,safari,unixpc m68000-att " +
"a29khif a29k-amd abacus abacus-unknown adobe68k m68010-adobe " +
"alliant,fx80 fx80-alliant " + 
"altos,altos3068 m68k-altos " +
"am29k a29k-none amd64 x86_64-pc amdahl 580-amdahl " +
"amiga,amigaos,amigados,amigaunix,amix m68k-unknown " +
"apollo68,apollo68bsd m68k-apollo " +
"aux m68k-apple balance ns32k-sequent c90 c90-cray " +
"convex-c1 c1-convex convex-c2 c2-convex convex-c32 c32-convex convex-c34 c34-convex " +
"conex-c38 c38-convex " + 
"cray,j90 j90-cray " + 
"craynv craynv-cray cr16c cr16c-unknown " +
"crds,unos m68k-crds " + 
"crisv32 crisv32-axis cris cris-axis crx crx-unknown da30 m68k-da30 " +
"decstation,decstation-3100,pmax,pmin,dec3100,decstatn mips-dec " +
"delta,3300,motorola-3300,motorola-delta,3300-motorola,delta-motorola,delta88 m68k-motorola " +
"djgpp i586-pc dpx20 rs6000-bull ebmon29k a29k-amd elxsi elxsi-elxsi " +
"encore,umax,mmax ns32k-encore " +
"es1800,OSE68k,ose68k,ose,OSE m68k-ericsson " +
"fx2800 i860-alliant genix ns32k-ns gmicro tron-gmicro go32 i386-pc " +
"h8300hms,h8300xray h8300-hitachi " +
"h8500hms h8500-hitachi harris m68k-harris " + 
"hp300bsd,hp300hpux m68k-hp " +
"hppaosf,hppro hppa1.1-hp " +
"i386mach i386-mach " + 
"i386-vsta,vsta i386-unknown " +
"iris,iris4d mips-sgi " +
"isi68,isi m68k-isi " +
"magnum,m3230 mips-mips " +
"merlin ns32k-utek mingw32 i386-pc miniframe m68000-convergent monitor m68k-rom68k " +
"morphos powerpc-unknown msdos i386-pc mvs i370-ibm ncr3000 i486-ncr netbsd386 i386-unknown " +
"netwinder armv4l-rebel " +
"news,news700,news800,news900 m68k-sony " +
"news1000 m68030-sony " +
"news-3600,risc-news mips-sony " +
"necv70 v70-nec next m68k-next nh3000 m68k-harris " +
"nindy960,mon960 i960-intel " +
"nonstopux mips-compaq np1 np1-gould nsr-tandem nsr-tandem openrisc or32-unknown " +
"os400 powerpc-ibm " + 
"OSE68000,ose68000 m68000-ericsson " +
"os68k m68k-none pa-hitachi hppa1.1-hitachi paragon i860-intel pbd sparc-tti pbb m68k-tti " +
"pc532 ns32k-pc532 " +
"pentium,p5,k5,k6,nexgen,viac32 i586-pc " +
"pentiumpro,p6,6x86,athlon,pentiumii,pentium2,pentiumiii,pentium3 i686-pc " +
"pentium4 i786-pc pn pn-gould power power-ibm ppc powerpc-unknown " +
"ppcle,powerpclittle,ppc-le,powerpc-little powerpcle-unknown " +
"ppc64 powerpc64-unknown " +
"ppc64le,powerpc64little,ppc64-le,powerpc64-little powerpc64le-unknown " +
"ps2 i386-ibm pw32 i586-unknown rom68k m68k-rom68k rtpc romp-ibm s390 s390-ibm " +
"s390x s390x-ibm sa29200 a29k-amd sb1 mipsisa64sb1-unknown sb1el mipsisa64sb1el-unknown " +
"sei mips-sei sequent i386-sequent sh sh-hitachi sh64 sh64-unknown " +
"sparclite-wrs,simso-wrs sparclite-wrs " +
"sps7 m68k-bull spur spur-unknown st2000 m68k-tandem stratus i860-stratus " +
"sun2,sun2os3,sun2os4 m68000-sun " +
"sun3os3,sun3os4,sun3 m68k-sun " +
"sun4os3,sun4os4,sun4sol2,sun4 sparc-sun " +
"sun386,sun386i,roadrunner i386-sun " +
"sv1 sv1-cray symmetry i386-sequent t3e alphaev5-cray t90 t90-cray tic54x tic54x-unknown " +
"tic55x tic55x-unknown tic6x tic6x-unknown tx39 mipstx39-unknown tx39el mipstx39el-unknown " +
"toad1 pdp10-xkl " +
"tower,tower-32 m68k-ncr " +
"tpf s390x-ibm udi29k a29k-amd ultra3 a29k-nyu " +
"v810,necv810 v810-nec " +
"vaxv,vms vax-dec " +
"vx f301-fujitsu vxworks960 i960-wrs vxworks68 m68k-wrs vxworks29k a29k-wrs " +
"xbos i686-pc " +
"xps,xps100 xps100-honeywell " +
"ymp ymp-cray none none-none w89k hppa1.1-winbond op50n hppa1.1-oki op60c hppa1.1-oki " +
"romp romp-ibm mmix mmix-knuth rs6000 rs6000-ibm vax vax-dev pdp10 pdp10-unknown " +
"pdp11 pdp11-dec we32k we32k-att " +
"sparc,sparcv8,sparcv9,sparcv9b sparc-sun " +
"cydra cydra-cydrome orion orion-highlevel orion105 clipper-highlevel " +
"mac,mpw,mac-mpw m68k-apple " +
"pmac,pmac-mpw powerpc-apple";

	const string s_startreplaces = 
"3b we32k-att amiga- m68k-unknown crisv32- crisv32-axis etraxfs crisv32-axis " +
"cris- cris-axis etrax cris-axis da30- m68k-da30 pmax- mips-dec " +
"decsystem10 pdp10-dec dec10 pdp10-dec decsystem20 pdp10-dec dec20 pdp10-dec " +
"dpx20- rs6000-bull dpx2 m68k-bull h3050r hppa1.1-hitachi hiux hppa1.1-hitachi " +
"hp300- m68k-hp i370-ibm i370-ibm ibm i370-ibm m88k-omron m88k-omron " +
"op50n- hppa1.1-oki op60c- hppa1.1-oki openrisc- or32-unknown pc532- ns32k-pc532 " +
"athlon_ i686-pc rtpc- romp-ibm s390- s390-ibm s390x- s390x-ibm sun3- m68k-sun " +
"c54x tic54x-unknown c55x tic55x-unknown c6x tic6x-unknown vpp f301-fujitsu " +
"vx- f301-fujitsu w65 w65-wdc w89k- hppa1.1-winbond";

	const string s_cpualiases =
"amd64 x86_x64 " +
"mips3 mips64 " + // plus mips3*
"pentium,p5,k5,k6,nexgen,viac3 i586 " +
"pentiumpro,p6,6x86,athlon,pentiumii,pentium2,pentiumiii,pentium3 i686 " +
"pentium4 i786 " +
"ppc powerpc " +
"ppcle,powerpclittle,ppc-le,powerpc-little powerpcle " +
"ppc64 powerpc64 " +
"ppc64le,powerpc64little,ppc64-le,powerpc64-little powerpc64le";

	const string s_mfgaliases = "digital dec commodore cbm";

	const string s_machosreplaces = 
"m6811,m68hc11,m6812,m68hc12,v810,necv810,none none " +
"386bsd,am29k,apollo68bsd,convex-c1,convex-c2,convex-c32,convex-c34,convex-c38,elxsi,hp300bsd bsd " +
"a29khif,sa29200,udi29k udi " +
"adobe68k scout " +
"amdahl,apollo68,gmicro,isi68,isi,magnum,m3230,merlin,necv70,vaxv sysv " +
"amigaos,amigados amigaos " +
"amigaunix,amix,ncr3000,stratus sysv4 " +
"aux aux " +
"balance dynix " +
"c90,cray,j90,sv1,t3e,t90,ymp unicos " +
"craynv unicosmp " +
"cr16c,crx elf " +
"delta88,harris sysv3 " +
"djgpp msdosdjgpp " +
"dpx20 bosx " +
"ebmon29k ebmon " +
"es1800,OSE68k,ose68k,ose,OSE,OSE68000,ose68000 ose " +
"go32 go32 " +
"h8300hms,h8500hms,sh hms " +
"h8300xray xray " +
"hp300hpux hpux " +
"hppa-next nextstep3 " +
"hppaosf,paragon osf " +
"hppro proelf " +
"i386mach mach " +
"i386-vsta,vsta vsta " +
"mingw32,xbox mingw32 " +
"monitor,rom68k,tic54x,tic55x,tic6x coff " +
"morphos morphos " +
"msdos msdos " +
"mvs mvs " +
"netbsd386 netbsd " +
"netwinder linux " +
"news,news700,news800,news900,news1000,news-3600,risc-news newsos " +
"nh3000,nh4000,nh5000 cxux " +
"nindy960 nindy " +
"mon960 mon960 " +
"nonstopux nonstopux " +
"os400 os400 " +
"os68k os68k " +
"pa-hitachi hiuxwe2 " +
"pw32 pw32 " +
"sei seiux " +
"sparclite-wrs,simso-wrs,vxworks960,vxworks68 vxworks " +
"sps7 sysv2 " +
"sun2os3,sun3os3,sun4os3 sunos3 " +
"sun2os4,sun3os4,sun4os4 sunos4 " +
"sun4sol2 solaris2 " +
"symmetry dynix " +
"toad1 tops2 " +
"tpf tpf " +
"ultra3 sym1 " +
"vms vms";

	const string s_machosstartreplaces = 
"decsystem10 tops10 dec10 tops10 dpx20 bosx dpx2 sysv3 " +
"h3050r hiuxwe2 hiux hiuxwe2 " +
"op50n- proelf op60c proelf " +
"c54x coff c55x coff c6x coff " +
"w65 none w89k proelf";

	const string s_osreplaces = 
"solaris solaris2 386bsd bsd ns2 nextstep2 svr4 sysv4 svr3 sysv3 " +
"sysvr4 sysv4 zvmoe zvmoe";

	const string s_osstartreplaces = 
"svr4 sysv4 unixware sysv4.2uw opened openedition os400 os400 " + 
"wince wince osfrose osfrose osf osf utek bsd dynix bsd acis aos " +
"atheos atheos syllable syllable ctix sysv uts sysv nova rtmk-nova nsk nsk " +
"sinix sysv4 tpf tpf triton sysv3 oss sysv3 ose ose es1800 ose " +
"aros aros kaos kaos solaris1 sunos4";

	const string s_osstartsubs = 
"solaris1. sunos4. gnu/linux linux-gnu mac macos linux linux-gnu " +
"sunos5 solaris2 sunos6 solaris3 sinix5. sysv5. nto nto-qnx";

	const string s_mfgosguesses = 
"acorn riscix1.2 dec ultrix4.2 sun sunos4.1.1 be beos haiku haiku " +
"ibm aix knuth mmixware wec,winbond,oki proelf hp hpux hitachi hiux " +
"att,ncr,altos,motorola,convergent,gould sysv cbm amigaos dg dgux " +
"dolphin sysv3 next nextstep3 sequent ptx crds unos ns genix " +
"highlevel,encore bsd sgi irix siemens sysv4 masscomp rtu " +
"rom68k coff apple macos";

	const string s_cpuosguesses = 
"c4x,tic4x,or32 coff sparc sunos4.1.1 i860 sysv i370 mvs vax ultrix4.2";

	const string s_osmfgstarts = 
"riscix acorn sunos sun aix ibm beos be hpux hp mpeix hp hiux hitachi " +
"unos crds dgux dg luna omron genix ns mvs ibm opened ibm os400 ibm " +
"ptx sequent tpf ibm vxsim wrs vxworks wrs windiss wrs aux apple " +
"hms hitachi mpw apple macos apple vos stratus";

	static Hashtable InitMultimap (string s)
	{
	    Hashtable h = new Hashtable ();
	    string[] bits = s.Split (' ');
	    
	    if ((bits.Length & 0x1) != 0)
		throw new Exception (String.Format ("Error in name mapping table: {0}, {1}, {2}", 
						    bits.Length, bits[0], bits[bits.Length - 1]));
	    
	    for (int i = 0; i < bits.Length; i += 2) {
		string[] synonyms = bits[i].Split (',');
		
		for (int j = 0; j < synonyms.Length; j++)
		    h[synonyms[j]] = bits[i + 1];
	    }
	    
	    return h;
	}

	static string[] InitStartList (string s)
	{
	    string[] a = s.Split (' ');
	    
	    if ((a.Length & 0x1) != 0)
		throw new Exception ("Error in starts-with table");

	    return a;
	}

	static ArchCanonicalize ()
	{
	    r_kernelos = new Regex (@"^(nto-qnx.*|linux-(gnu.*|dietlibc|uclibc.*)|" +
				    @"uclinux-(uclibc.*|gnu.*)|kfreebsd.*-gnu.*|" +
				    @"knetbsd.*-gnu.*|netbsd.*-gnu.*|storm-chaos.*|" +
				    @"os2-emx.*|rtmk-nova.*)$");
	    
	    r_firstosnoop = new Regex (@"(sun.*os.*)|(scout)");
	    
	    r_notosmach = new Regex (@"^(dec.*|mips.*|sequent.*|encore.*|pc532.*|" +
				     @"sgi.*|sony.*|att.*|7300.*|3300.*|delta.*|" +
				     @"motorola.*|sun[234].*|unicom.*|ibm.*|next|" +
				     @"hp|isi.*|apollo|altos.*|convergent.*|" +
				     @"ncr.*|news|32.*|3600.*|3100.*|hitachi.*|" +
				     @"c[123].*|convex.*|sun|crds|omron.*|dg|" +
				     @"ultra|tti.*|harris|dolphin|highlevel|" +
				     @"gould|cbm|ns|masscomp|apple|axis|knuth" +
				     @"cray|sim|cisco|oki|wec|winbond)$");
	    
	    r_sco86 = new Regex (@"86-.*");

	    r_sco32v = new Regex (@"^sco3.2[v\.][4-9].*");
	    
	    r_sco32dot = new Regex (@"^sco3.2\.");
	    
	    r_mint = new Regex (@"(mint|mint[0-9].*|MiNT|MiNT[0-9]*)$");
	    
	    r_cpuset1 = new Regex (@"^(580|a29k|alpha|(alphaev([4-8]|56|6[78]))|" +
				   @"alphapca5[67]|alpha64|alpha64ev([4-8]|56|6[78])|" +
				   @"alpha64pca5[67]|am33_2.0|arc|arm|arm[bl]e|arme[lb]|" +
				   @"armv[2345]|armv[345][lb]|avr|bfin|c4x|clipper|d[13]0v|" +
				   @"dlx|fr30|frv|h8[35]00|hppa|hppa1\.[01]|hppa2\.0[nw]?|" +
				   @"hppa64|i370|i860|i960|ia64|ip2k|iq2000|m32r(le?)|m68000|" +
		                   @"m68k|m88k|maxq|mcore|mips([bl]e)?|mipse[bl]|mips16|" +
		                   @"mips64(vr)?(el)?|mips64orion(el)?|" +
		                   @"mips64vr(41|43|50|59)00(el)?|" +
		                   @"mipsisa(32|32r2|64|64r2|64sb1|64sr71k)(el)?|" +
		                   @"mipstx39(el)?|ms1|msp430|ns(16|32)k|or32" +
		                   @"pdp1[01]|pjl?|powerpc(64)?(le)?|ppcbe|pyramid|" +
		                   @"sh[1234]?|sh[24]a|sh[23]e|sh[34eb]|sh[bl]e|sh[1234]le|" +
		                   @"sh3ele|sh64(le)?|sparc(64)?|sparc64b|sparc86x|sparclet|" +
		                   @"sparclite|sparcv[89]|sparcv9b|strongarm|tahoe|thumb|" +
		                   @"tic(4x|80)|tron|v850e?|we32k|x86|xscale|xscalee[bl]|" +
		                   @"xstormy16|xtensa|z8k)$");

	    r_pc_cpu = new Regex (@"i.*86|x86_64");
	    
	    r_baremach_noop = new Regex (@"^(m88110|m680[12346]0|m683.2|m68360|m5200|v70|w65)$");
	    
	    r_baremach_unk = new Regex (@"^(1750a|am33_2.0|dsp16xx|m32c|mn10[23]00|" +
					@"m68(hc)?1[12])$");
	    
	    r_mfgmach_ok = new Regex (@"^(bs2000|c[123]|c30|[cjt]90|c5[45]x|c6x|craynv|cydra|" +
				      @"elsxi|f30[01]|f700|fx80|i.*86|m680[12346]0|m68360|m683.2|" + 
				      @"m88110|mmix|none|np1|orion|pn|power|romp|rs6000|" + 
				      @"sv1|sx.|tic30|tic5[45]x|tic6x|vax|x86_64|xps100|ymp)");
	    
	    r_osnoop = new Regex (@"^(gnu|bsd|mach|minix|genix|ultrix|irix|.*vms|sco|esix|isc|" +
				  @"aix|sunos[34]?|hpux|unos|osf|luna|dgux|solaris|sym|" +
				  @"amigaos|amigados|msdos|newsos|unicos|aof|aos|nindy|vxsim|" +
				  @"vxworks|ebmon|hms|mvs|clix|riscos|uniplus|iris|rtu|xenix|" +
				  @"hiux|386bsd|knetbsd|mirbsd|netbsd|openbsd|ekkobsd|kfreebsd|" +
				  @"freebsd|riscix|lynxos|bosx|nextstep|cxux|aout|elf|oabi|" +
				  @"ptx|coff|ecoff|winnt|domain|vsta|udi|eabi|lites|ieee|go32|" +
				  @"aux|chorusos|chorusrdb|cygwin|pe|psos|moss|proelf|rtems|" +
				  @"mingw32|linux-gnu|linux-uclibc|uxpv|beos|mpeix|udk|" +
				  @"interix|uwin|mks|rhapsody|darwin|opened|openstep|oskit|" +
				  @"conix|pw32|nonstopux|storm-chaos|tops10|tenex|tops20|its|" +
				  @"os2|vos|palmos|uclinux|nucleus|morphos|superux|rtmk|" +
				  @"rtmk-nova|windiss|powermax|dnix|nx6|nx7|sei|dragonfly|" +
				  @"skyos|haiku|" +
				  @"nto-qnx|" +
				  @"es1800|hms|os86k|none|v88r|windows|netware|os9|beos|" +
				  @"macos|mpw|magic|mmixware|mon960|lnews" +
				  @")");

	    r_osnoopfull = new Regex ("^(sim|xray|osx|abug)$");

	    // Tables 
		
	    h_machreplace = InitMultimap (s_machreplaces);
	    a_startreplace = InitStartList (s_startreplaces);
	    h_cpualiases = InitMultimap (s_cpualiases);
	    h_machosreplace = InitMultimap (s_machosreplaces);
	    a_machosstartreplace = InitStartList (s_machosstartreplaces);
	    a_mfgalias = s_mfgaliases.Split (' ');
	    h_osreplace = InitMultimap (s_osreplaces);
	    a_osstartreplace = InitStartList (s_osstartreplaces);
	    a_osstartsubs = InitStartList (s_osstartsubs);
	    h_mfgosguesses = InitMultimap (s_mfgosguesses);
	    h_cpuosguesses = InitMultimap (s_cpuosguesses);
	    a_osmfgstarts = InitStartList (s_osmfgstarts);
	}

	static string FindStartsReplace (string[] table, string text)
	{
	    for (int i = 0; i < table.Length; i += 2) {
		if (StrUtils.StartsWith (text, table[i]))
		    return table[i+1];
	    }
	    
	    return null;
	}

	static string FindStartsSub (string[] table, string text)
	{
	    for (int i = 0; i < table.Length; i += 2) {
		if (StrUtils.StartsWith (text, table[i]))
		    return text.Replace (table[i], table[i+1]);
	    }
	    
	    return null;
	}
	
	public static void SetFromString (string desc, Architecture arch)
	{
	    // Derived from automake 1.9.6 config.sub, timestamp 2005-07-08
	    
	    if (desc == null)
		throw new ArgumentNullException ();
	    if (desc.Length == 0)
		throw new ArgumentException ();
	    
	    // valid inputs, as far as I can tell:
	    //
	    // cpu -> cpu-[guess manu]-"none"
	    // magicalias -> [magic cpu]-[magic manu]-[magic os] (eg, mvs or sun)
	    // cpu-manu -> cpu-manu-[guess os]
	    // cpu-os -> cpu-[guess manu]-os
	    // cpu-kernel -> cpu-[guess manu]-kernel-[guess os]
	    // manu-kernel -> [guess cpu]-manu-kernel-[guess os]
	    // manu-os -> [guess cpu]-manu-os
	    // cpu-manu-os -> cpu-manu-os
	    // cpu-manu-kernel -> cpu-manu-kernel-[guess os]
	    // cpu-manu-kernel-os -> cpu-manu-kernel-os
	    //
	    // Due to the insanity of this code, I opt to just follow
	    // config.sub as closely as possible.

	    string[] s = desc.Split ('-');
	    int len = s.Length;
	    string os, mach, tmp;

	    // config.sub, lines 120 - 133
	    
	    if (len > 1) {
		string maybeos = s[len - 2] + "-" + s[len - 1];
		
		if (r_kernelos.IsMatch (maybeos)) {
		    os = maybeos;
		    mach = string.Join ("-", s, 0, len - 2);
		} else {
		    os = s[len - 1];
		    mach = string.Join ("-", s, 0, len - 1);
		}
	    } else {
		mach = s[0];
		os = "";
	    }

	    // config.sub, lines 139 - 223

	    if (r_firstosnoop.IsMatch (os)) {
	    } else if (r_notosmach.IsMatch (os)) {
		os = "";
		mach = desc;
	    } else if (os == "wrs") {
		os = "vxworks";
		mach = desc;
	    } else if (StrUtils.StartsWith (os, "chorusos")) {
		os = "chorusos";
		mach = desc;
	    } else if (os == "chorusrdb") {
		os = "chorusrdb";
		mach = desc;
	    } else if (StrUtils.StartsWith (os, "hiux")) {
		os = "hiuxwe2";
	    } else if (os == "sco5") {
		os = "sco3.2v5";
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (os == "sco4") {
		os = "sco3.2v4";
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (r_sco32v.IsMatch (os)) {
		os = r_sco32dot.Replace (os, "sco3.2v");
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (StrUtils.StartsWith (os, "sco")) {
		os = "sco3.2v2";
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (StrUtils.StartsWith (os, "udk")) {
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (os == "isc") {
		os = "isc2.2";
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (StrUtils.StartsWith (os, "clix")) {
		mach = "clipper-intergraph";
	    } else if (StrUtils.StartsWith (os, "isc")) {
		mach = r_sco86.Replace (desc, "86-pc");
	    } else if (StrUtils.StartsWith (os, "lynx")) {
		os = "lynxos";
	    } else if (StrUtils.StartsWith (os, "ptx")) {
		mach = r_sco86.Replace (desc, "86-sequent");
	    } else if (StrUtils.StartsWith (os, "windowsnt")) {
		os = os.Replace ("windowsnt", "winnt");
	    } else if (StrUtils.StartsWith (os, "psos")) {
		os = "psos";
	    } else if (r_mint.IsMatch (os)) {
		os = "mint";
		mach = "m68k-atari";
	    }

	    // config.sub, lines 226 - 1129. 

	    s = mach.Split ('-');
	    len = s.Length;

	    // Do CPU aliases now to make life easy.

	    if (h_cpualiases.Contains (s[0])) {
		s[0] = (string) h_cpualiases[s[0]];
		mach = String.Join ("-", s);
	    }

	    // Now, record any OS hints
	    
	    if (h_machosreplace.Contains (mach)) {
		os = (string) h_machosreplace[mach];
	    } else if ((tmp = FindStartsReplace (a_machosstartreplace, mach)) != null) {
		os = tmp;
	    } else if (mach == "iris" || mach == "iris4d") {
		if (!StrUtils.StartsWith (os, "irix"))
		    os = "irix4";
	    } else if (mach == "next") {
		if (StrUtils.StartsWith (os, "nextstep")) {
		} else if (StrUtils.StartsWith (os, "ns2"))
		    os = "nextstep2";
		else
		    os = "nextstep3";
	    }
	    
	    // Now clean up machine names.

	    if (len == 1 && r_cpuset1.IsMatch (s[0])) {
		mach = s[0] + "-unknown";
	    } else if (len == 1 && r_baremach_unk.IsMatch (s[0])) {
		mach = s[0] + "-unknown";
	    } else if (len == 1 && r_baremach_noop.IsMatch (s[0])) {
	    } else if (len == 1 && r_pc_cpu.IsMatch (s[0])) {
		mach = s[0] + "-pc";
	    } else if (len > 2) {
		throw new ArgumentException ("More than two pieces to machine name chunk?");
	    } else if (len == 2 && r_cpuset1.IsMatch (s[0])) {
	    } else if (len == 2 && r_mfgmach_ok.IsMatch (s[0])) {
	    } else if (h_machreplace.Contains (mach)) {
		mach = (string) h_machreplace[mach];
	    } else if ((tmp = FindStartsReplace (a_startreplace, mach)) != null) {
		mach = tmp;
	    } else if (Regex.IsMatch ("dpx2.*-bull", mach)) {
		mach = "rs6000-bull";
		os = "bosx";
	    } else if (Regex.IsMatch ("hp(3k)?9[0-9]", mach)) {
		mach = "hppa1.0-hp";
	    } else if (Regex.IsMatch ("hp9k(2[0-9][0-9])|(31[0-9])", mach)) {
		mach = "m68000-hp";
	    } else if (Regex.IsMatch ("hp9k3[2-9][0-9]", mach)) {
		mach = "m68k-hp";
	    } else if (Regex.IsMatch ("hp(9k)?6[0-9][0-9]", mach)) {
		mach = "hppa1.0-hp";
	    } else if (Regex.IsMatch ("hp(9k)?7[0-79][0-9]", mach)) {
		mach = "hppa1.0-hp";
	    } else if (Regex.IsMatch ("hp(9k)?78[0-9]", mach)) {
		mach = "hppa1.1-hp"; // config.sub: "FIXME really hppa2.0-hp"
	    } else if (Regex.IsMatch ("hp(9k)?8([67]1|0[24]|[78]9|893)", mach)) {
		mach = "hppa1.1-hp"; // config.sub: "FIXME really hppa2.0-hp"
	    } else if (Regex.IsMatch ("hp(9k)?8[0-9][13679]", mach)) {
		mach = "hppa1.1-hp";
	    } else if (Regex.IsMatch ("hp(9k)?8[0-9][0-9]", mach)) {
		mach = "hppa1.0-hp";
	    } else if (Regex.IsMatch ("i.*86v32", mach)) {
		mach = r_sco86.Replace (mach, "86-pc");
		os = "sysv32";
	    } else if (Regex.IsMatch ("i.*86v4.*", mach)) {
		mach = r_sco86.Replace (mach, "86-pc");
		os = "sysv4";
	    } else if (Regex.IsMatch ("i.*86v", mach)) {
		mach = r_sco86.Replace (mach, "86-pc");
		os = "sysv";
	    } else if (Regex.IsMatch ("i.*86sol2", mach)) {
		mach = r_sco86.Replace (mach, "86-pc");
		os = "solaris2";
	    } else if (r_mint.IsMatch (mach)) {
		mach = "m68k-atari";
		os = "mint";
	    } else if (Regex.IsMatch ("mips3.*-.*", mach)) {
		mach = mach.Replace ("mips3", "mips64");
	    } else if (StrUtils.StartsWith (mach, "mips3.*")) {
		mach = mach.Replace ("mips3", "mips64") + "-unknown";
	    } else if (Regex.IsMatch ("m.*-next", mach)) {
		mach = "m68k-next";
		
		if (StrUtils.StartsWith (os, "nextstep")) {
		} else if (StrUtils.StartsWith (os, "ns2")) {
		    os = "nextstep2";
		} else {
		    os = "nextstep3";
		}
	    } else if (Regex.IsMatch ("z8k-.*-coff", mach)) {
		// can this happen? there's a z8k match earlier
		mach = "z8k-unknown";
		os = "sim";
	    } else if (Regex.IsMatch ("(sh[1234](le)?)|(sh[24]a)|(sh[34]eb)|sh[23]ele", mach)) {
		mach = "sh-unknown";
	    } else if (len == 2 && s[1] == "unknown") {
	    } else {
		throw new ArgumentException ("Can't figure out machine \"" + mach + "\"");
	    }

	    // config.sub lines 1132 - 1141

	    s = mach.Split ('-');
	    len = s.Length;
	    
	    if (len != 2)
		throw new ArgumentException ("Machine has more than two parts! \"" + mach + "\"");
	    
	    for (int i = 0; i < a_mfgalias.Length; i += 2 ) {
		string n = a_mfgalias[i];
		
		if (!StrUtils.StartsWith (s[1], n))
		    continue;
		
		s[1] = a_mfgalias[i+1] + s[1].Substring (n.Length);
		break;
	    }
	    
	    mach = String.Join ("-", s);

	    // config.sub lines 1145 - 1495
	    
	    if (os != null && os.Length > 0) {
		if (StrUtils.StartsWith (os, "qnx")) {
		    if (!r_pc_cpu.IsMatch (mach))
			os = "nto-" + os;
		} else if (h_osreplace.Contains (os)) {
		    os = (string) h_osreplace[os];
		} else if ((tmp = FindStartsReplace (a_osstartreplace, os)) != null) {
		    os = tmp;
		} else if (r_osnoop.IsMatch (os)) {
		} else if (r_osnoopfull.IsMatch (os)) {
		} else if ((tmp = FindStartsSub (a_osstartsubs, os)) != null) {
		    os = tmp;
		} else if (r_mint.IsMatch (os)) {
		    os = "mint";
		} else {
		    throw new ArgumentException ("Dont know OS \"" + os + "\"");
		}
	    } else {
		if (s[0] == "pdp10") {
		    os = "tops20";
		} else if (s[0] == "pdp11") {
		    os = "none";
		} else if (s[1] == "tti") {
		    os = "sysv3";
		} else if (h_mfgosguesses.Contains (s[1])) {
		    os = (string) h_mfgosguesses[s[1]];
		} else if (h_cpuosguesses.Contains (s[0])) {
		    os = (string) h_mfgosguesses[s[0]];
		} else if (Regex.IsMatch (@"arm.*-rebel", mach)) {
		    os = "linux";
		} else if (Regex.IsMatch (@"arm.*-semi", mach)) {
		    os = "aout";
		} else if (Regex.IsMatch (@"m68.*-apollo", mach)) {
		    os = "domain";
		} else if (mach == "i386-sun") {
		    os = "sunos4.0.2";
		} else if (mach == "m68000-sun") {
		    os = "sunos3";
		} else if (Regex.IsMatch (@"m68.*-cisco", mach)) {
		    os = "aout";
		} else if (Regex.IsMatch (@"m68.*-", mach)) {
		    os = "elf";
		} else if (mach == "m68k-ccur") {
		    os = "rtu";
		} else if (Regex.IsMatch (@"m88k-omron.*", mach)) {
		    os = "luna";
		} else if (Regex.IsMatch (@"f(300|301|700)-fujitsu", mach)) {
		    os = "uxpv";
		} else if (Regex.IsMatch (@".*-.*bug$", mach)) {
		    os = "coff";
		} else if (Regex.IsMatch (@".*-atari.*", mach)) {
		    os = "mint";
		} else {
		    os = "none";
		}
	    }
	    
	    // config.sub lines 1497 - 1569
	    
	    if (s[1] == "unknown") {
		if ((tmp = FindStartsReplace (a_osmfgstarts, os)) != null) {
		    s[1] = tmp;
		} else if (r_mint.IsMatch (os)) {
		    s[1] = "atari";
		}
		
		// not strictly neccessary since we're done
		mach = s[0] + "-" + s[1];
	    }
	    
	    arch.CPU = s[0];
	    arch.Manufacturer = s[1];
	    
	    s = os.Split ('-');
	    if (s.Length == 1) {
		arch.Kernel = null;
		arch.OS = os;
	    } else if (s.Length == 2) {
		arch.Kernel = s[0];
		arch.OS = s[1];
	    } else
		throw new ArgumentException (String.Format ("Got weird OS: {0} -> {1}", desc, os));
	}
    }
}
