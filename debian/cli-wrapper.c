#include <stdio.h>
#include <glib.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>

extern int errno;
extern char **environ;

#define cmd args[1]
#define NAME_ISREG(name)		\
	((! stat(name,&si)) &&		\
	 S_ISREG((&si)->st_mode))

/* extend monopath to the default lib path */
#define usdl "/usr/share/dotnet/lib"
#define mypaths "/usr/bin:/usr/share/dotnet/bin:/usr/share/dotnet:/usr/lib/dotnet/bin:/usr/lib/dotnet"

int main(int argc, char **argv){
   char *exName;
   char *args[argc+2];
   struct stat si;
   char **runpaths=NULL;
   char *monopath=NULL;
   char *foo;
   
   memcpy(args+sizeof(char), argv, sizeof(char *) * argc);
   args[argc+1]=NULL;
   args[0]=argv[0];
   exName = strrchr(argv[0], '/');
   if(!exName) exName = argv[0]; else exName++;
   cmd = (char *) calloc( 512, sizeof(char));
   /* Done. exName has the binary name, look for it and write result to cmd */

   monopath = getenv ("MONO_PATH");
   /* only split to runpaths when found */
   foo = monopath ? 
      g_strjoin(G_SEARCHPATH_SEPARATOR_S, monopath, mypaths, NULL) :
      mypaths;

   //printf("bla, %s\n", foo);

   runpaths = g_strsplit (foo, G_SEARCHPATH_SEPARATOR_S, 1000);

   while(*runpaths) {
      snprintf(cmd, 511, "%s/%s.exe", *runpaths, exName);
      if(NAME_ISREG(cmd)) {
         if(monopath) setenv("MONO_PATH", g_strjoin(G_SEARCHPATH_SEPARATOR_S, monopath, *runpaths, usdl, NULL), 1);
         else setenv("MONO_PATH", g_strjoin(G_SEARCHPATH_SEPARATOR_S, *runpaths, usdl, NULL), 1);
         return execve("/usr/bin/cli",args,environ);
      }
      snprintf(cmd, 511, "%s/%s/%s.exe", *runpaths, exName, exName);
      if(NAME_ISREG(cmd)) {
         if(monopath) setenv("MONO_PATH", g_strjoin(G_SEARCHPATH_SEPARATOR_S, monopath, g_strjoin("/", *runpaths, exName, NULL), usdl, NULL), 1);
         else setenv("MONO_PATH", g_strjoin(G_SEARCHPATH_SEPARATOR_S, g_strjoin("/", *runpaths, exName, NULL), usdl, NULL), 1);
         return execve("/usr/bin/cli",args,environ);
      }
      runpaths++;
   }
   
   /* should never be reached */
   errno = ENOENT;
   return(-1);
}
