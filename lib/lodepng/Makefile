PROJ=liblodepng
IOS_SDK_VER=7.1


SRC=$(wildcard *.c)
OBJS=$(patsubst %.c,%.o,$(SRC))
UNAME=$(shell uname)

LIB_EXT=so
LD_SHARED=-shared
ifeq ($(UNAME),Darwin)
	LD_SHARED=-dynamiclib
	LIB_EXT=dylib
endif


all: $(OBJS)
	$(LD) $(LD_SHARED) $(LDFLAGS) -o $(PROJ).$(LIB_EXT) $(OBJS)
	ar rcs $(PROJ).a $(OBJS)

static: $(OBJS)
	ar rcs $(PROJ).a $(OBJS)

android: CC=~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/toolchains/arm-linux-androideabi-4.7/prebuilt/darwin-x86/bin/arm-linux-androideabi-gcc
android: LD=~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/toolchains/arm-linux-androideabi-4.7/prebuilt/darwin-x86/bin/arm-linux-androideabi-gcc
android: CFLAGS=-I ~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/platforms/android-14/arch-arm/usr/include -mhard-float -mfloat-abi=softfp -mfpu=vfp
android: LDFLAGS=-L ~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/platforms/android-14/arch-arm/usr/lib \
	-L ~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/toolchains/arm-linux-androideabi-4.7/prebuilt/darwin-x86/lib/gcc/arm-linux-androideabi/4.7/armv7-a/ \
	-lc -lgcc \
	--sysroot ~/Library/Developer/Xamarin/android-ndk/android-ndk-r8d/platforms/android-14/arch-arm
android: $(OBJS)
	$(LD) $(LD_SHARED) $(LDFLAGS) -o $(PROJ).so $(OBJS)

$(ARCH)-$(PROJ): $(OBJS)

clean:
	rm -f *.dylib *.so *.a obj/ $(OBJS)

.PHONY: desktop ios clean
