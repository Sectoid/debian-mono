#include <stdio.h>
extern char **environ;

int main(int argc, char **argv){
   char *tmp;
   char *args[argc+2];
   memcpy(args+sizeof(char), argv, sizeof(char *) * argc);
   args[argc+1]=NULL;
   args[0]=argv[0];
   tmp = strrchr(argv[0], '/');
   if(!tmp) tmp=argv[0]; else tmp++;
   args[1] = (char *) calloc( 15+strlen(tmp), sizeof(char)); 
   sprintf(args[1], "/usr/bin/%s.exe", tmp);
   return execve("/usr/bin/cli",args,environ);
} 
