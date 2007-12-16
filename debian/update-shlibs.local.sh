#!/bin/sh -e

VERSION=$(dpkg-parsechangelog | grep ^Vers | cut -d\  -f2)
UPVERSION=$(echo $VERSION | sed 's,-.*,,')

echo -n "Updating shlibs.local for Mono $UPVERSION..."

cp debian/shlibs.local debian/shlibs.local.backup
rm -f debian/shlibs.local.new

# libs that don't ship shlibs
echo "libMonoPosixHelper 0 libmono0 (>= $UPVERSION)"	>> debian/shlibs.local.new
echo "libgdiplus 0 libgdiplus (>= $UPVERSION)"		>> debian/shlibs.local.new
echo "libgluezilla 0 libgluezilla (>= $UPVERSION)"	>> debian/shlibs.local.new

grep --no-filename "libcairo 2"		/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new
grep --no-filename "libfbclient 2"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new
grep --no-filename "libsqlite 0"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new
grep --no-filename "libsqlite3 0"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new
grep --no-filename "libasound 2"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new
grep --no-filename "libgda-2 3"		/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new

cp debian/shlibs.local.new debian/shlibs.local

echo "done."
