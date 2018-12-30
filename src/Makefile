LIBDIR := $(HOME)/ksp/KSP_Data/Managed
BINDIR := bin
OUTDIR := ../GameData/SPEngine/Plugins

SOURCES := Core.cs Util.cs Design.cs DesignLibrary.cs Family.cs ModuleSPEngine.cs UI/MasterWindow.cs UI/AbstractWindow.cs Properties/AssemblyInfo.cs

all: $(OUTDIR)/SPEngine.dll

$(BINDIR)/SPEngine.dll: $(SOURCES)
	mcs -t:library -nostdlib -r:$(LIBDIR)/Assembly-CSharp.dll,$(LIBDIR)/UnityEngine.dll,$(LIBDIR)/UnityEngine.UI.dll,$(LIBDIR)/System.dll,$(LIBDIR)/System.Core.dll,$(LIBDIR)/mscorlib.dll -out:$@ $(SOURCES)

$(OUTDIR)/SPEngine.dll: $(BINDIR)/SPEngine.dll
	cp $< $@
