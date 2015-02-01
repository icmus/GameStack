SHELL=/bin/bash
JSON_NET_VER=6.0.1
ASSIMP_NET_VER=tags/3.3.1
SDL2_CS_VER=babac2a7f0bab92d8a63d195effbf3804743f60a
ASSIMP_VER=tags/v3.1.1
LIBOGG_VER=1.3.2
LIBOPUS_VER=1.1
LIBOPUSFILE_VER=0.6
CAIRO_VER=1.12.16
PIXMAN_VER=0.32.4
FREETYPE_VER=2.5.3
IOS_SDK_VER=8.1
ANDROID_VER=14
ANDROID_NDK_VER=8d
NDK_TOOLCHAIN_VER=4.7
OPENALSOFT_VER=1.15.1

UNAME=$(shell uname)
LIB_EXT=so
NDK_ROOT=/opt/android-ndk-r$(ANDROID_NDK_VER)
CMAKE_FLAGS=
ifeq ($(UNAME),Darwin)
	LIB_EXT=dylib
	CFLAGS=-m32 -arch i386
	DEVROOT=/Applications/Xcode.app/Contents/Developer
	IOS_ROOT=$(DEVROOT)/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS$(IOS_SDK_VER).sdk
	NDK_ROOT=$(HOME)/Library/Developer/Xamarin/android-ndk/android-ndk-r$(ANDROID_NDK_VER)
	CMAKE_FLAGS=-DCMAKE_OSX_ARCHITECTURES=i386
endif
NDK_TOOLS_ROOT=$(NDK_ROOT)/toolchains/arm-linux-androideabi-$(NDK_TOOLCHAIN_VER)/prebuilt/darwin-x86
NDK_SYS_ROOT=$(NDK_ROOT)/platforms/android-$(ANDROID_VER)/arch-arm
NDK_TOOL=$(NDK_TOOLS_ROOT)/bin/arm-linux-androideabi
NDK_CC=$(NDK_TOOL)-gcc
NDK_LD=$(NDK_TOOL)-ld
NDK_CFLAGS=--sysroot $(NDK_SYS_ROOT) -I $(NDK_SYS_ROOT)/usr/include -I $(NDK_ROOT)/sources/android/cpufeatures -mhard-float -mfloat-abi=softfp -mfpu=vfp -D__ANDROID__
NDK_LDFLAGS=--sysroot $(NDK_SYS_ROOT) -L$(NDK_SYS_ROOT)/usr/lib -L$(NDK_TOOLS_ROOT)/lib/arm-linux-androideabi/4.7/armv7-a

nuget = \
	mkdir -p .tmp; \
	pushd .tmp; \
	wget -O $(1).zip http://packages.nuget.org/api/v1/package/$(1)/$(2); \
	unzip $(1).zip 'lib/*'; \
	if [ -d lib/net45/ ]; then \
		cp -f lib/net45/* ../../bin/; \
	elif [ -d lib/net40/ ]; then \
		cp -f lib/net40/* ../../bin/; \
	elif [ -d lib/net35/ ]; then \
		cp -f lib/net35/* ../../bin/; \
	fi; \
	popd; \
	rm -rf .tmp; \
	rm -f ../bin/policy.*; \
	rm -f ../bin/*.pdb; 

wget = \
	if [ ! -d $(3) ]; then \
		mkdir -p $(3) && pushd $(3); \
		wget $(1); \
		tar xvf $(2) --strip-components=1; \
		rm $(2); \
		popd; \
		rm -rf .tmp; \
	fi

autoconf = \
	cd $(1); \
	if [ "$(2)" != "" ]; then \
		if [ "$(14)" != "1" ] || [ "$(UNAME)" = "Darwin" ]; then \
			./configure --enable-shared --enable-static $(8) $(9) && make clean; \
			[ "$(12)" != "" ] && /bin/bash -c "$(12)"; \
			make; \
			cp -f $(2) ../$(3); \
		fi; \
	fi; \
	if [ "$(4)" != "" ] && [ $(UNAME) == 'Darwin' ]; then \
		./configure --host=arm-apple-darwin7 --disable-shared --enable-static $(8) $(10) CC="$(DEVROOT)/usr/bin/gcc" CFLAGS="-marm -arch armv7 -isysroot $(IOS_ROOT)" && make clean; \
		[ "$(12)" != "" ] && /bin/bash -c "$(12)"; \
		make; \
		cp -f $(4) ../$(5) 2>/dev/null || true; \
	fi; \
	if [ "$(6)" != "" ] && [ -d $(NDK_ROOT) ]; then \
		./configure --host=arm-linux-androideabi CC=$(NDK_CC) LD=$(NDK_LD) \
			RANLIB=$(NDK_TOOL)-ranlib OBJDUMP=$(NDK_TOOL)-objdump \
			DLLTOOL=$(NDK_TOOL)-dlltool AR=$(NDK_TOOL)-ar NM=$(NDK_TOOL)-nm CPP=$(NDK_TOOL)-cpp \
			CPPFLAGS="$(NDK_CFLAGS)" --with-sysroot=$(NDK_SYS_ROOT) $(8) $(11) && make clean; \
		[ "$(12)" != "" ] && /bin/bash -c "$(12)"; \
		[ "$(13)" != "" ] && /bin/bash -c "$(13)"; \
		make; \
		cp -f $(6) ../$(7); \
	fi;

lipo = \
	if [ $(UNAME) = 'Darwin' ]; then \
		lipo -arch i386 $(1) -arch arm $(2) -create -output $(3); \
	fi

dyld_ref = \
	if [ $(UNAME) = 'Darwin' ]; then \
		install_name_tool -change $(2) $(3) $(1); \
	fi

dyld_id = \
	if [ $(UNAME) = 'Darwin' ]; then \
		install_name_tool -id $(2) $(1); \
	fi

comma := ,

all: json-net assimp-net sdl2-cs pnglite assimp libogg libopus libopusfile openalsoft pixman freetype cairo

json-net: folders
	$(call nuget,Newtonsoft.Json,$(JSON_NET_VER))

assimp-net: folders
	if [ ! -d assimp-net ]; then \
		svn checkout http://assimp-net.googlecode.com/svn/$(ASSIMP_NET_VER) assimp-net; \
		patch -d assimp-net -p0 < patches/assimp-nix.patch; \
	fi
	xbuild /p:Configuration=Net45-Release assimp-net/AssimpNet.sln
	cp -f assimp-net/AssimpNet/bin/Net45-Release/AssimpNet.dll ../bin/

sdl2-cs: folders
	if [ ! -d sdl2-cs ]; then \
		git clone https://github.com/flibitijibibo/SDL2-CS sdl2-cs; \
		cd sdl2-cs && git checkout $(SDL2_CS_VER) && patch -p1 < ../patches/sdl2-cs.patch; \
	fi
	xbuild /p:Configuration=Debug sdl2-cs/SDL2-CS.csproj 
	xbuild /p:Configuration=Release sdl2-cs/SDL2-CS.csproj
	cp -f sdl2-cs/bin/Release/SDL2-CS.* ../bin/desktop/
	cp -f sdl2-cs/bin/Debug/SDL2-CS.dll ../bin/desktop/SDL2-CS.dbg.dll
	cp -f sdl2-cs/bin/Debug/SDL2-CS.dll.mdb ../bin/desktop/SDL2-CS.dbg.dll.mdb

pnglite: folders
	if [ $(UNAME) = 'Darwin' ]; then \
		cd pnglite && gcc -c -m32 -arch i386 -o pnglite.o pnglite.c && gcc -m32 -arch i386 -dynamiclib -lz pnglite.o -o libpnglite.$(LIB_EXT); \
		ar rcs libpnglite.a pnglite.o; \
		cp -f libpnglite.$(LIB_EXT) libpnglite.a ../../bin/desktop; \
        if [ -d $(IOS_ROOT) ]; then \
            gcc -isysroot $(IOS_ROOT) -marm -arch armv7 -c -o pnglite.o pnglite.c && gcc -isysroot $(IOS_ROOT) -marm -arch armv7 -dynamiclib -lz pnglite.o -o libpnglite.$(LIB_EXT); \
            ar rcs libpnglite.a pnglite.o; \
            cp -f libpnglite.a ../../bin/ios; \
            lipo -arch i386 ../../bin/desktop/libpnglite.a -arch arm ../../bin/ios/libpnglite.a -create -output ../../bin/ios/libpnglite.a; \
        fi; \
	else \
		cd pnglite && gcc -fPIC -c -o pnglite.o pnglite.c && gcc -lz -shared pnglite.o -o libpnglite.$(LIB_EXT); \
		cp -f libpnglite.$(LIB_EXT) ../../bin/desktop; \
	fi; \
	if [ -d $(NDK_ROOT) ]; then \
			$(NDK_CC) $(NDK_CFLAGS) -c -o pnglite.o pnglite.c; \
			$(NDK_CC) $(NDK_LDFLAGS) -lz -shared -o libpnglite.so pnglite.o; \
			cp -f libpnglite.so ../../bin/android; \
	fi

assimp: folders
	if [ ! -d assimp ]; then \
		git clone https://github.com/assimp/assimp assimp; \
		cd assimp && git fetch --tags && git checkout $(ASSIMP_VER); \
	fi
	cd assimp && cmake -G 'Unix Makefiles' $(CMAKE_FLAGS) && make
	cp -f assimp/lib/libassimp.$(LIB_EXT) ../bin/

libogg: folders
	$(call wget,http://downloads.xiph.org/releases/ogg/libogg-$(LIBOGG_VER).tar.xz,libogg-$(LIBOGG_VER).tar.xz,libogg)
	$(call autoconf,libogg,src/.libs/libogg.a,../bin/desktop,src/.libs/libogg.a,../bin/ios,src/.libs/libogg.a,../bin/android,,CFLAGS="$(CFLAGS) -fPIC",,CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)")
	$(call lipo,../bin/desktop/libogg.a,../bin/ios/libogg.a,../bin/ios/libogg.a)

libopus: folders
	$(call wget,http://downloads.xiph.org/releases/opus/opus-$(LIBOPUS_VER).tar.gz,opus-$(LIBOPUS_VER).tar.gz,libopus)
	$(call autoconf,libopus,.libs/libopus.a,../bin/desktop,.libs/libopus.a,../bin/ios,.libs/libopus.a,../bin/android,,CFLAGS="$(CFLAGS) -fPIC",,CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)")
	$(call lipo,../bin/desktop/libopus.a,../bin/ios/libopus.a,../bin/ios/libopus.a)

libopusfile: folders
	$(call wget,http://downloads.xiph.org/releases/opus/opusfile-$(LIBOPUSFILE_VER).tar.gz,opusfile-$(LIBOPUSFILE_VER).tar.gz,libopusfile)
	$(call autoconf,libopusfile,.libs/libopusfile.{$(LIB_EXT)$(comma)a},../bin/desktop,.libs/libopusfile.a,../bin/ios,.libs/libopusfile.so,../bin/android,--disable-http --disable-doc CFLAGS="$(CFLAGS)" DEPS_CFLAGS="-I`pwd`/../libogg/include -I`pwd`/../libopus/include",DEPS_LIBS="-logg -lopus -L`pwd`/../../bin/desktop",DEPS_LIBS="-logg -lopus -L`pwd`/../../bin/ios",DEPS_LIBS="-logg -lopus -L`pwd`/../../bin/android" CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)")
	$(call lipo,../bin/desktop/libopusfile.a,../bin/ios/libopusfile.a,../bin/ios/libopusfile.a)
	$(call dyld_id,../bin/desktop/libopusfile.dylib,libopusfile.dylib)
	$(call dyld_ref,../bin/desktop/libopusfile.dylib,libogg.dylib,"@loader_path/libogg.dylib")
	$(call dyld_ref,../bin/desktop/libopusfile.dylib,libopus.dylib,"@loader_path/libopus.dylib")

pixman: folders
	$(call wget,http://cairographics.org/releases/pixman-$(PIXMAN_VER).tar.gz,pixman-$(PIXMAN_VER).tar.gz,pixman)
	$(call autoconf,pixman,pixman/.libs/libpixman-1.a,../bin/desktop,pixman/.libs/libpixman-1.a,../bin/ios,pixman/.libs/libpixman-1.a,../bin/android,--disable-gtk --disable-libpng CFLAGS="$(CFLAGS) -DPIXMAN_NO_TLS",,,CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)",echo SUBDIRS=pixman >> Makefile,,1)
	$(call lipo,../bin/desktop/libpixman-1.a,../bin/ios/libpixman-1.a,../bin/ios/libpixman-1.a)

freetype: folders
	$(call wget,http://download.savannah.gnu.org/releases/freetype/freetype-$(FREETYPE_VER).tar.bz2,freetype-$(FREETYPE_VER).tar.bz2,freetype)
	$(call autoconf,freetype,objs/.libs/libfreetype.{$(LIB_EXT)$(comma)a},../bin/desktop,objs/.libs/libfreetype.a,../bin/ios,objs/.libs/libfreetype.a,../bin/android,--with-png=no --with-harfbuzz=no --with-bzip2=no --with-zlib=no --with-old-mac-fonts=no CFLAGS="$(CFLAGS)",,,CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)",,,1)
	$(call lipo,../bin/desktop/libfreetype.a,../bin/ios/libfreetype.a,../bin/ios/libfreetype.a)

cairo: folders
	$(call wget,http://cairographics.org/releases/cairo-$(CAIRO_VER).tar.xz,cairo-$(CAIRO_VER).tar.xz,cairo)
	patch -N -d cairo -p1 < patches/cairo-android.patch || true
	$(call autoconf,cairo,src/.libs/libcairo.{$(LIB_EXT)$(comma)a},../bin/desktop,src/.libs/libcairo.a,../bin/ios,src/.libs/libcairo.so,../bin/android,pixman_CFLAGS="-I`pwd`/../pixman/pixman" FREETYPE_CFLAGS="-I`pwd`/../freetype/include" --disable-fc --enable-ft --disable-ps --disable-pdf --disable-svg --disable-script --disable-xlib --disable-qt --disable-quartz --disable-png --disable-egl --disable-glx --disable-wgl --disable-gobject,FREETYPE_LIBS="-lfreetype" CFLAGS="$(CFLAGS)" pixman_LIBS="-L`pwd`/../../bin/desktop -lpixman-1",pixman_LIBS="-L`pwd`/../../bin/ios -lpixman-1",pixman_LIBS="-L`pwd`/../../bin/android -lpixman-1 `pwd`/src/cpu-features.lo" CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS)",,cp -f $(NDK_ROOT)/sources/android/cpufeatures/cpu-features.* src/ && ./libtool --mode=compile $(NDK_CC) $(NDK_CFLAGS) -g -O -c src/cpu-features.c -o src/cpu-features.o,1)
	$(call lipo,../bin/desktop/libcairo.a,../bin/ios/libcairo.a,../bin/ios/libcairo.a)

openalsoft: folders
	$(call wget,http://kcat.strangesoft.net/openal-releases/openal-soft-$(OPENALSOFT_VER).tar.bz2,openal-soft-$(OPENALSOFT_VER).tar.bz2,openalsoft)
	if [ -d $(NDK_ROOT) ]; then \
		cd openalsoft/ && patch -N -p1 < ../patches/openalsoft-android.patch; \
		cd build/; \
		CC="$(NDK_CC)" LD=$(NDK_LD) \
			RANLIB=$(NDK_TOOL)-ranlib OBJDUMP=$(NDK_TOOL)-objdump \
			DLLTOOL=$(NDK_TOOL)-dlltool AR=$(NDK_TOOL)-ar NM=$(NDK_TOOL)-nm CPP=$(NDK_TOOL)-cpp \
			CFLAGS="$(NDK_CFLAGS)" LDFLAGS="$(NDK_LDFLAGS) -lm" cmake -DEXAMPLES=OFF -DCMAKE_SYSTEM_NAME="Linux" ..; \
		make; \
		cp -f libopenal.so ../../../bin/android/libopenal32.so; \
	fi;

folders:
	mkdir -p ../bin ../bin/desktop ../bin/ios ../bin/android

clean:
	rm -rf json-net assimp-net sdl2-cs assimp libogg libopus libopusfile pixman cairo openalsoft

.PHONY: all json-net assimp-net sdl2-cs pnglite assimp libogg libopus libopusfile pixman cairo openalsoft pixman freetype cairo
