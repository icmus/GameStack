CONFIGURATION=Release
XBUILD=xbuild
XBUILDOPTS=/t:Build /p:WarningLevel=0
IMPORT=mono bin/import.exe
PREFIX=/usr/local
MONO_VER=3.4.0
MDK_PATH=/opt/mono-$(MONO_VER)
LIB_EXT=so
MDTOOLOPTS=build -c:$(CONFIGURATION)
MD_PATH=/usr/local
MD_VER=5.0
MD_ADDINS_PATH=$(HOME)/.local/share/XamarinStudio-$(MD_VER)/LocalInstall/Addins
ifeq ($(shell uname),Darwin)
	LIB_EXT=dylib
	MDK_PATH=/Library/Frameworks/Mono.framework/Versions/$(MONO_VER)
	MDTOOL="/Applications/Xamarin Studio.app/Contents/MacOS/mdtool"
	MD_PATH="/Applications/Xamarin Studio.app/Contents/MacOS"
	MD_ADDINS_PATH="$(HOME)/Library/Application Support/XamarinStudio-$(MD_VER)/LocalInstall/Addins"
endif

xbuild = \
	$(XBUILD) $(XBUILDOPTS) /p:Configuration=$(CONFIGURATION) $(1)

mdbuild = \
	$(MDTOOL) $(MDTOOLOPTS) -t:$(1) -p:$(2) src/GameStack.sln

import = \
	$(IMPORT) $(1) $(2)

gac = \
	gacutil -i $(1)

ungac = \
	gacutil -u $(1)

all: pipeline import desktop

libs:
	make -C lib

pipeline:
	$(call xbuild,src/pipeline/GameStack.Pipeline.csproj)

import:
	$(call xbuild,src/pipeline/import/Import.csproj)

desktop:
	$(call xbuild,src/Desktop/bindings/GameStack.Desktop.Bindings.csproj)
	$(call xbuild,src/Desktop/GameStack.Desktop.csproj)
	$(call xbuild,src/MonoDevelop/MonoDevelop.GameStack.csproj)

ios:
	$(call mdbuild,Clean,GameStack.iOS.Bindings)
	$(call mdbuild,Clean,GameStack.iOS)
	$(call mdbuild,Build,GameStack.iOS.Bindings)
	$(call mdbuild,Build,GameStack.iOS)

android:
	$(call mdbuild,Clean,GameStack.Android.Bindings)
	$(call mdbuild,Clean,GameStack.Android)
	$(call mdbuild,Build,GameStack.Android.Bindings)
	$(call mdbuild,Build,GameStack.Android)

install: install-libs install-addin install-gac

uninstall: uninstall-gac uninstall-addin uninstall-libs

install-libs:
	if [ -d bin ]; then \
		mkdir -p $(PREFIX)/lib/GameStack; \
		install bin/*.exe bin/*.dll bin/*.mdb bin/*.config bin/*.$(LIB_EXT) dist/*.sh $(PREFIX)/lib/GameStack; \
		cp -n bin/*.$(LIB_EXT) $(PREFIX)/lib/; \
		if [ -d bin/desktop ]; then \
			mkdir -p $(PREFIX)/lib/GameStack/desktop; \
			install bin/desktop/*.dll bin/desktop/*.mdb bin/desktop/*.config bin/desktop/*.$(LIB_EXT) $(PREFIX)/lib/GameStack/desktop; \
			for i in liblodepng libogg libopus libopusfile libpixman-1 libcairo; do \
				ln -s $(PREFIX)/lib/GameStack/desktop/$$i.$(LIB_EXT) $(PREFIX)/lib/$$i.$(LIB_EXT); \
			done; \
		fi; \
		[ -d bin/ios ] && mkdir -p $(PREFIX)/lib/GameStack/ios && install bin/ios/*.dll bin/ios/*.mdb bin/ios/*.a $(PREFIX)/lib/GameStack/ios; \
		[ -d bin/android ] && mkdir -p $(PREFIX)/lib/GameStack/android && install bin/android/*.dll bin/android/*.so $(PREFIX)/lib/GameStack/android; \
		for i in gamestack-desktop gamestack-ios gamestack-android sdl2-cs; do \
			[ -d $(PREFIX)/lib/pkgconfig ] && sed 's,_LIB_,$(PREFIX)/lib,g' dist/$$i.pc > $(PREFIX)/lib/pkgconfig/$$i.pc; \
		done; \
	fi;

uninstall-libs:
	rm -rf $(PREFIX)/lib/GameStack
	rm -f $(PREFIX)/lib/pkgconfig/gamestack-*.pc
	for i in liblodepng libogg libopus libopusfile libpixman-1 libcairo; do \
		[ ! -e $(PREFIX)/lib/$$i ] && rm -f $(PREFIX)/lib/$$i.$(LIB_EXT); \
	done

install-addin:
	if [ -d $(MD_PATH) ]; then \
		install dist/gamestack-*.pc $(MD_PATH)/lib/pkgconfig; \
		mkdir -p $(MD_ADDINS_PATH)/GameStack; \
		install bin/MonoDevelop.GameStack.dll $(MD_ADDINS_PATH)/GameStack; \
		for i in gamestack-desktop gamestack-ios gamestack-android sdl2-cs; do \
			sed 's,_LIB_,$(PREFIX)/lib,g' dist/$$i.pc > $(MD_PATH)/lib/pkgconfig/$$i.pc; \
		done; \
		for i in liblodepng libogg libopus libopusfile libpixman-1 libcairo; do \
			ln -sf $(PREFIX)/lib/GameStack/$$i.$(LIB_EXT) $(MD_PATH)/lib/$$i.$(LIB_EXT); \
		done; \
	fi

uninstall-addin:
	rm -rf $(MD_PATH)/lib/pkgconfig/gamestack-*.pc $(MD_ADDINS_PATH)/GameStack
	for i in liblodepng libogg libopus libopusfile libpixman-1 libcairo; do \
		rm -f $(MD_PATH)/lib/$$i.$(LIB_EXT); \
	done

install-gac:
	$(call gac,bin/desktop/GameStack.Desktop.dll)
	$(call gac,bin/desktop/GameStack.Desktop.Bindings.dll)
	$(call gac,bin/desktop/SDL2-CS.dll)

uninstall-gac:
	$(call ungac,GameStack.Desktop)
	$(call ungac,GameStack.Desktop.Bindings)
	$(call ungac,SDL2-CS)

clean:
	find . -type d \( -name bin -o -name obj -o -name assets \) |xargs rm -rf ; rm -rf bin/ 

.PHONY: all libs pipeline import ios android desktop clean install install-libs uninstall-libs \
	uninstall install-addin uninstall-addin install-gac uninstall-gac
