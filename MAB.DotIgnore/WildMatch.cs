using System;

namespace MAB.DotIgnore
{
    internal static class WildMatch
    {
        internal const int ABORT_MALFORMED = 2;
        internal const int NOMATCH = 1;
        internal const int MATCH = 0;
        internal const int ABORT_ALL = -1;
        internal const int ABORT_TO_STARSTAR = -2;

        static char NEGATE_CLASS = '!';
        static char NEGATE_CLASS2 = '^';

        internal static int IsMatch(string pattern, string text, MatchFlags flags)
        {
            return Match(pattern.ToCharArray(), text.ToCharArray(), 0, 0, flags);
        }

        private static int Match(char[] pattern, char[] text, int p, int t, MatchFlags flags)
        {
            int p_len = pattern.Length;
            int p_EOP = p_len - 1;

            int t_len = text.Length;
            int t_EOP = t_len - 1;

            char p_ch;

            for (; p < p_len && (p_ch = pattern[p]) != -1; p++, t++)
            {
                int match, negated;
                bool match_slash;
        
                char t_ch, prev_ch;
        
                if (t == t_len)
                    return p_ch != '*' ? ABORT_ALL : MATCH;
            
                t_ch = text[t];

                if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsUpper(t_ch))
                    t_ch = Char.ToLower(t_ch);
                if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsUpper(p_ch))
                    p_ch = Char.ToLower(p_ch);

                switch (p_ch)
                {
                    // Escape character: require literal match of next char
                    case '\\': 
                        if(p == p_EOP)
                            return NOMATCH;
                        p_ch = pattern[++p];
                        goto default;
                    // Normal character: literal match
                    default: 
                        if (t_ch != p_ch)
                            return NOMATCH;
                        continue;
                    // Match any character except slash
                    case '?': 
                        if (t_ch == '/')
                            return NOMATCH;
                        continue;
                    // Match any character unless PATHNAME flag is set, then match any char except slash
                    case '*': 
                        // If the *next* character is a star as well...
                        if ((++p) < p_len && pattern[p] == '*')
                        {
                            // Figure out what the character *before* the first star is
                            // (using null char to represent the beginning of the pattern)
                            char pre_star_ch = (p - 2) >= 0 ? pattern[p - 2] : '\0';
                            // Advance through the pattern until we get to something which *isn't* a star
                            while ((++p) < p_len && pattern[p] == '*') { }
                            // If PATHNAME isn't set, a single star also matches slashes
                            if (!flags.HasFlag(MatchFlags.PATHNAME))
                                match_slash = true;
                            // If the character before the first star is either the beginning of the pattern or a slash,
                            // and the character after the last star is either the end of the pattern or a slash
                            if ((pre_star_ch == '\0' || pre_star_ch == '/') && (p == p_len || pattern[p] == '/'))
                            {
                                if ((p == p_len || pattern[p] == '/') && Match(pattern, text, p + 1, t, flags) == MATCH)
                                    return MATCH;

                                match_slash = true;
                            }
                            else
                            {
                                // The pattern is invalid (double-star wildcards are only valid 
                                // if bounded by slashes or beginning/end of line)
                                return ABORT_MALFORMED;
                            }
                        }
                        else
                        {
                            // It's only a single star, so use PATHNAME to determine whether to match slashes
                            match_slash = !flags.HasFlag(MatchFlags.PATHNAME);
                        }
                
                        // If we're at the end of the pattern
                        if (p == p_len)
                        {
                            // If there was only one star and PATHNAME was set, match_slash will be false
                            // Trailing "*" matches only if there are no more slash characters
                            if (!match_slash && Array.IndexOf(text, '/', t) != -1)
                                return NOMATCH;
                    
                            // Trailing "**" matches everything
                            return MATCH;
                        }
                        else if  (!match_slash && pattern[p] == '/') 
                        {
                            // We're at a slash, so consume the text until the next slash
                            int nextSlashIndex = Array.IndexOf(text, '/', t);
                            // If there aren't any more slashes, this can't be a match
                            if (nextSlashIndex == -1)
                                return NOMATCH;
                        
                            t = nextSlashIndex;
                            break;
                        }
                
                        // Try to match the remaining text
                        // Each time the match fails, remove the first character from the text and retry
                        while (true)
                        {
                            // Try to advance faster when an asterisk is followed by a literal. 
                            // We know in this case that the string before the literal must belong to "*".
                            // If match_slash is false, do not look past the first slash as it cannot belong to '*'.
                            if (!IsGlobSpecial(pattern[p]))
                            {
                                p_ch = pattern[p];
                        
                                if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsUpper(p_ch))
                                    p_ch = Char.ToLower(p_ch);
                            
                                while (t < t_len && ((t_ch = text[t]) != '/' || match_slash))
                                {
                                    if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsUpper(t_ch))
                                        t_ch = Char.ToLower(t_ch);
                                
                                    if (t_ch == p_ch)
                                        break;
                                
                                    t++;
                                }

                                if (t_ch != p_ch)
                                    return NOMATCH;
                            }

                            if ((match = Match(pattern, text, p, t, flags)) != NOMATCH)
                                return match;

                            if(t == t_EOP)
                                break;

                            t_ch = text[++t];
                        }
                
                        return ABORT_ALL;
                    // Match character ranges
                    case '[':
                        if (++p == p_len)
                            return ABORT_ALL;

                        p_ch = pattern[p];
                        
                        negated = (p_ch == NEGATE_CLASS || p_ch == NEGATE_CLASS2) ? 1 : 0;

                        if (negated == 1)
                        {
                            if (++p == p_len)
                                return ABORT_ALL;

                            p_ch = pattern[p];
                        }
                
                        prev_ch = '\0';
                        match = 0;
                
                        do {
                            if (p_ch == '\\') 
                            {
                                if (++p == p_len)
                                    return ABORT_ALL;
                        
                                p_ch = pattern[p];
                        
                                if (t_ch == p_ch)
                                    match = 1;
                            } 
                            else if (p_ch == '-' && prev_ch != '\0' && p < p_EOP && pattern[p + 1] != ']') 
                            {
                                p_ch = pattern[++p];
                        
                                if (p_ch == '\\') 
                                {
                                    if (++p == p_len)
                                        return ABORT_ALL;
                                
                                    p_ch = pattern[p];
                                }

                                if (t_ch <= p_ch && t_ch >= prev_ch)
                                {
                                    match = 1;
                                }
                                else if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsLower(t_ch)) 
                                {
                                    char t_ch_upper = Char.ToUpper(t_ch);
                                    if (t_ch_upper <= p_ch && t_ch_upper >= prev_ch)
                                        match = 1;
                                }

                                p_ch = '\0'; // This makes "prev_ch" get set to 0
                            } 
                            else if (p_ch == '[' && p < p_EOP && pattern[p + 1] == ':') 
                            {
                                int s;
                                int i;
                        
                                // SHARED ITERATOR
                                for (s = p + 2; p < p_len && (p_ch = pattern[p]) != ']'; p++) {} 
                        
                                if (p == p_EOP)
                                    return ABORT_ALL;
                            
                                i = p - s - 1;

                                if (i < 0 || pattern[p - 1] != ':') 
                                {
                                    // Didn't find ":]", so treat like a normal set
                                    p = s - 2;
                                    p_ch = '[';
                                    if (t_ch == p_ch)
                                        match = 1;
                                    continue;
                                }

                                if (CC_EQ(pattern, s, i, "alnum")) 
                                {
                                    if (Char.IsLetterOrDigit(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "alpha")) 
                                {
                                    if (Char.IsLetter(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "blank")) 
                                {
                                    if (Char.IsWhiteSpace(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "cntrl")) 
                                {
                                    if (Char.IsControl(t_ch))
                                        match = 1;
                                }
                                else if (CC_EQ(pattern, s, i, "digit")) 
                                {
                                    if (Char.IsDigit(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "graph")) 
                                {
                                    if (ISGRAPH(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "lower")) 
                                {
                                    if (Char.IsLower(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "print")) 
                                {
                                    if (ISPRINT(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "punct")) 
                                {
                                    if (Char.IsPunctuation(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "space")) 
                                {
                                    if (Char.IsWhiteSpace(t_ch))
                                        match = 1;
                                } 
                                else if (CC_EQ(pattern, s, i, "upper")) 
                                {
                                    if (Char.IsUpper(t_ch))
                                        match = 1;
                                    else if (flags.HasFlag(MatchFlags.CASEFOLD) && Char.IsLower(t_ch))
                                        match = 1;
                                }
                                else if (CC_EQ(pattern, s, i, "xdigit")) 
                                {
                                    if (Char.IsDigit(t_ch) || (Char.ToLower(t_ch) >= 'a' && Char.ToLower(t_ch) <= 'f'))
                                        match = 1;
                                } 
                                else // Malformed [:class:] string
                                {
                                    return ABORT_ALL;
                                }

                                p_ch = '\0'; // This makes "prev_ch" get set to 0
                            } 
                            else if (t_ch == p_ch)
                            {
                                match = 1;
                            }

                            prev_ch = p_ch;
                        } while (p < p_EOP && (p_ch = pattern[++p]) != ']');
                
                        if (match == negated || (flags.HasFlag(MatchFlags.PATHNAME) && t_ch == '/'))
                            return NOMATCH;
                
                        continue;
                }
            }
    
            return t == text.Length ? MATCH : NOMATCH;
        }

        static bool IsGlobSpecial(char c)
        {
            return c == '*' || c == '?' || c == '[' || c == '\\';
        }

        static bool CC_EQ(char[] pattern, int s, int len, string @class)
        {
            return string.Compare(new String(pattern, s, len), @class, StringComparison.Ordinal) == 0;
        }

        static bool ISPRINT(char ch) => ch >= 0x20 && ch <= 0x7e;
        static bool ISGRAPH(char ch) => ISPRINT(ch) && ch != 0x20;
    }
}
