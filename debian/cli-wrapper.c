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
#define runit if(NAME_ISREG(cmd)) return execve("/usr/bin/cli",args,environ);
#define trypath(dir) sprintf(cmd, "%s/%s.exe", dir, exName); runit; sprintf(cmd, "%s/%s/%s.exe", dir, exName, exName); runit;
#define runthem(paths)  { char **tmp=paths; while(*tmp) { trypath(*tmp); tmp++; } }
      
int main(int argc, char **argv){
   char *exName;
   char *args[argc+2];
   struct stat si;
   char *paths[] = {
      "/usr/bin",
      "/usr/share/dotnet/exe",
      "/usr/share/dotnet/bin",
      "/usr/share/dotnet",
      "/usr/lib/dotnet/exe",
      "/usr/lib/dotnet/bin",
      "/usr/lib/dotnet",
      NULL
   };
   const char *path;
   
   memcpy(args+sizeof(char), argv, sizeof(char *) * argc);
   args[argc+1]=NULL;
   args[0]=argv[0];
   exName = strrchr(argv[0], '/');
   if(!exName) exName=argv[0]; else exName++;
   cmd = (char *) calloc( 215+strlen(exName), sizeof(char));
   /* Done. exName has the binary name, look for it and write result to cmd */

   path = getenv ("MONO_PATH");
   if (path)
      runthem(g_strsplit (path, G_SEARCHPATH_SEPARATOR_S, 1000));

   runthem(paths);

   /* should never be reached */
   errno = ENOENT;
   return(-1);
} 
