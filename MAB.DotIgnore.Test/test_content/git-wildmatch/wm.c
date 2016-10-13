#include <stddef.h>
#include "wildmatch.h"

main(int argc, char **argv)
{
    if(argc < 4) 
    {
        printf("%s\n", "Usage: wm <pattern> <text> <casefold>");
        return 1;
    }

    int flags = WM_PATHNAME;

    if(strcmp(argv[3], "1") == 0)
        flags = WM_PATHNAME | WM_CASEFOLD;

    int match = wildmatch(argv[1], argv[2], flags, NULL);
    printf("%i\n", match);

    return match;
}