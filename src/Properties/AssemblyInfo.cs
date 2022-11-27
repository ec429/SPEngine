using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle ("SPEngine")]
[assembly: AssemblyDescription ("Simple Procedural Engine")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("KSPSPE")]
[assembly: AssemblyCopyright ("Copyright © 2018-20")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: Guid("349cd4dd-fbd9-495e-aea9-73b036736bd8")]

[assembly: AssemblyVersion("0.3")]
[assembly: AssemblyFileVersion("0.3.2")]

// Use KSPAssembly to allow other DLLs to make this DLL a dependency in a
// non-hacky way in KSP.  Format is (AssemblyProduct, major, minor), and it
// does not appear to have a hard requirement to match the assembly version.
[assembly: KSPAssembly("SPEngine", 0, 3)]
[assembly: KSPAssemblyDependency("RealFuels", 15, 2)]
