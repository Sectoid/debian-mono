#ifndef __EGLIB_CONFIG_H
#define __EGLIB_CONFIG_H

/*
 * System-dependent settings
 */
#define G_GNUC_PRETTY_FUNCTION   
#define G_GNUC_UNUSED            __attribute__((__unused__))
#define G_BYTE_ORDER             G_LITTLE_ENDIAN
#define G_GNUC_NORETURN          __attribute__((__noreturn__))
#define G_SEARCHPATH_SEPARATOR_S ":"
#define G_SEARCHPATH_SEPARATOR   ':'
#define G_DIR_SEPARATOR          '/'
#define G_DIR_SEPARATOR_S        "/"
#define G_BREAKPOINT()           G_STMT_START { __asm__ ("int $03"); } G_STMT_END
#define G_OS_UNIX
#define GPOINTER_TO_INT(ptr)   ((gint)(long) (ptr))
#define GPOINTER_TO_UINT(ptr)  ((guint)(long) (ptr))
#define GINT_TO_POINTER(v)     ((gpointer)(glong) (v))
#define GUINT_TO_POINTER(v)    ((gpointer)(gulong) (v))

#if 1 == 1
#define G_HAVE_ALLOCA_H
#endif

typedef unsigned long gsize;
typedef signed   long gssize;

#define G_GSIZE_FORMAT   "lu"
#define G_GUINT64_FORMAT "lu"
#define G_GINT64_FORMAT "ld"

#if 1 == 1
#define G_HAVE_ISO_VARARGS
#endif

#if defined (__native_client__)
#define sem_trywait(x) sem_wait(x)
#define sem_timedwait(x,y) sem_wait(x)
#define getdtablesize() (32768)
#undef G_BREAKPOINT
#define G_BREAKPOINT()
#endif

#endif
