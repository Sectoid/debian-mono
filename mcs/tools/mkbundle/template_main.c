int mono_main (int argc, char* argv[]);

#include <stdlib.h>
#include <string.h>
#ifdef _WIN32
#include <windows.h>
#endif

static char **mono_options = NULL;

static int count_mono_options_args (void)
{
	const char *e = getenv ("MONO_BUNDLED_OPTIONS");
	const char *p, *q;
	int i, n;

	if (e == NULL)
		return 0;

	/* Don't bother with any quoting here. It is unlikely one would
	 * want to pass options containing spaces anyway.
	 */

	p = e;
	n = 1;
	while ((q = strchr (p, ' ')) != NULL) {
		n++;
		p = q + 1;
	}

	mono_options = malloc (sizeof (char *) * (n + 1));

	p = e;
	i = 0;
	while ((q = strchr (p, ' ')) != NULL) {
		mono_options[i] = malloc ((q - p) + 1);
		memcpy (mono_options[i], p, q - p);
		mono_options[i][q - p] = '\0';
		i++;
		p = q + 1;
	}
	mono_options[i++] = strdup (p);
	mono_options[i] = NULL;

	return n;
}


int main (int argc, char* argv[])
{
	char **newargs;
	int i, k = 0;

#ifdef _WIN32
	/* CommandLineToArgvW() might return a different argc than the
	 * one passed to main(), so let it overwrite that, as we won't
	 * use argv[] on Windows anyway.
	 */
	wchar_t **wargv = CommandLineToArgvW (GetCommandLineW (), &argc);
#endif

	newargs = (char **) malloc (sizeof (char *) * (argc + 2) + count_mono_options_args ());

#ifdef _WIN32
	newargs [k++] = g_utf16_to_utf8 (wargv [0], -1, NULL, NULL, NULL);
#else
	newargs [k++] = argv [0];
#endif

	if (mono_options != NULL) {
		i = 0;
		while (mono_options[i] != NULL)
			newargs[k++] = mono_options[i++];
	}

	newargs [k++] = image_name;

	for (i = 1; i < argc; i++) {
#ifdef _WIN32
		newargs [k++] = g_utf16_to_utf8 (wargv [i], -1, NULL, NULL, NULL);
#else
		newargs [k++] = argv [i];
#endif
	}
#ifdef _WIN32
	LocalFree (wargv);
#endif
	newargs [k] = NULL;
	
	if (config_dir != NULL && getenv ("MONO_CFG_DIR") == NULL)
		mono_set_dirs (getenv ("MONO_PATH"), config_dir);
	
	mono_mkbundle_init();

	return mono_main (k, newargs);
}
