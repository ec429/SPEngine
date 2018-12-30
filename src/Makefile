LIBDIR := $(HOME)/ksp/KSP_Data/Managed
BINDIR := bin

SOURCES := Core.cs Util.cs DesignLibrary.cs UI/MasterWindow.cs UI/AbstractWindow.cs Properties/AssemblyInfo.cs

$(BINDIR)/SPEngine.dll: $(SOURCES)
	mcs -t:library -nostdlib -r:$(LIBDIR)/Assembly-CSharp.dll,$(LIBDIR)/UnityEngine.dll,$(LIBDIR)/System.dll -r:$(LIBDIR)/System.Core.dll -r:$(LIBDIR)/mscorlib.dll -out:$@ $(SOURCES)
