#!/bin/sh -e

VERSION=$(dpkg-parsechangelog | grep ^Vers | cut -d\  -f2)
UPVERSION=$(echo $VERSION | sed 's,-.*,,' | sed 's,+dfsg,,')

echo -n "Updating shlibs.local for Mono $UPVERSION..."

# libgamin-dev, libcupsys2-dev, librsvg2-dev, libgtk2.0-dev, libgnomeui-dev

cp debian/shlibs.local debian/shlibs.local.backup
rm -f debian/shlibs.local.new

# libs that don't ship shlibs
echo "libMonoPosixHelper 0 libmono0 (>= $UPVERSION)"	>> debian/shlibs.local.new
echo "libgdiplus 0 libgdiplus (>= $UPVERSION)"		>> debian/shlibs.local.new
echo "libgluezilla 0 libgluezilla (>= $UPVERSION)"	>> debian/shlibs.local.new

grep --no-filename "^libcairo 2"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libcairo"
grep --no-filename "^libfbclient 2"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libfbclient"
grep --no-filename "^libsqlite 0"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libsqlite"
grep --no-filename "^libsqlite3 0"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libsqlite3"
grep --no-filename "^libasound 2"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libasound"
grep --no-filename "^libgda-2 3"	/var/lib/dpkg/info/*.shlibs >> debian/shlibs.local.new || echo "ERROR: could not resolve libgda-2"

cp debian/shlibs.local.new debian/shlibs.local

echo "done."
