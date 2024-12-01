namespace AdventOfCode;

using System;
using System.Linq;
using System.Text;

internal static class OcrExtension
{
    public static OcrString Ocr(this string st)
    {
        return new OcrString(st);
    }
}

internal record OcrString(string st)
{
    public override string ToString()
    {
        string[] lines = this.st.Split("\n").
            SkipWhile(x => string.IsNullOrWhiteSpace(x)).
            TakeWhile(x => !string.IsNullOrWhiteSpace(x)).
            ToArray();

        while (lines.All(line => line.StartsWith(' ')))
        {
            lines = GetRect(lines, 1, 0, lines[0].Length - 1, lines.Length).Split("\n");
        }

        while (lines.All(line => line.EndsWith(' ')))
        {
            lines = GetRect(lines, 0, 0, lines[0].Length - 1, lines.Length).Split("\n");
        }

        int width = lines[0].Length;

        string[] smallAlphabet = """
                                 A    B    C    E    F    G    H    I    J    K    L    O    P    R    S    U    Y    Z    
                                  ##  ###   ##  #### ####  ##  #  #  ###   ## #  # #     ##  ###  ###   ### #  # #   ##### 
                                 #  # #  # #  # #    #    #  # #  #   #     # # #  #    #  # #  # #  # #    #  # #   #   # 
                                 #  # ###  #    ###  ###  #    ####   #     # ##   #    #  # #  # #  # #    #  #  # #   #  
                                 #### #  # #    #    #    # ## #  #   #     # # #  #    #  # ###  ###   ##  #  #   #   #   
                                 #  # #  # #  # #    #    #  # #  #   #  #  # # #  #    #  # #    # #     # #  #   #  #    
                                 #  # ###   ##  #### #     ### #  #  ###  ##  #  # ####  ##  #    #  # ###   ##    #  #### 
                                 """.Split('\n');

        string[] largeAlphabet = """
                                 A       B       C       E       F       G       H       J       K       L       N       P       R       X       Z
                                   ##    #####    ####   ######  ######   ####   #    #     ###  #    #  #       #    #  #####   #####   #    #  ######  
                                  #  #   #    #  #    #  #       #       #    #  #    #      #   #   #   #       ##   #  #    #  #    #  #    #       #  
                                 #    #  #    #  #       #       #       #       #    #      #   #  #    #       ##   #  #    #  #    #   #  #        #  
                                 #    #  #    #  #       #       #       #       #    #      #   # #     #       # #  #  #    #  #    #   #  #       #   
                                 #    #  #####   #       #####   #####   #       ######      #   ##      #       # #  #  #####   #####     ##       #    
                                 ######  #    #  #       #       #       #  ###  #    #      #   ##      #       #  # #  #       #  #      ##      #     
                                 #    #  #    #  #       #       #       #    #  #    #      #   # #     #       #  # #  #       #   #    #  #    #      
                                 #    #  #    #  #       #       #       #    #  #    #  #   #   #  #    #       #   ##  #       #   #    #  #   #       
                                 #    #  #    #  #    #  #       #       #   ##  #    #  #   #   #   #   #       #   ##  #       #    #  #    #  #       
                                 #    #  #####    ####   ######  #        ### #  #    #   ###    #    #  ######  #    #  #       #    #  #    #  ######  
                                 """.Split('\n');

        string[] charMap;
        int charWidth = 0;
        int charHeight = 0;
        if (lines.Length == smallAlphabet.Length - 1)
        {
            charMap = smallAlphabet;
            charWidth = 5;
            charHeight = 6;
        }
        else if (lines.Length == largeAlphabet.Length - 1)
        {
            charMap = largeAlphabet;
            charWidth = 8;
            charHeight = 10;
        }
        else
        {
            return "<UNKNOWN ALPHABET>";
        }

        StringBuilder builder = new();
        for (int i = 0; i < width; i += charWidth)
        {
            builder.Append(Detect(lines, i, charWidth, charHeight, charMap));
        }

        return builder.ToString();
    }

    public static string Detect(string[] text, int icolLetter, int charWidth, int charHeight, string[] charMap)
    {
        string textRect = GetRect(text, icolLetter, 0, charWidth, charHeight);

        for (int icol = 0; icol < charMap[0].Length; icol += charWidth)
        {
            string ch = charMap[0][icol].ToString();
            string charPattern = GetRect(charMap, icol, 1, charWidth, charHeight);
            bool found = Enumerable.Range(0, charPattern.Length).
                All(
                    i =>
                    {
                        bool textWhiteSpace = " .".Contains(textRect[i]);
                        bool charWhiteSpace = " .".Contains(charPattern[i]);
                        return textWhiteSpace == charWhiteSpace;
                    }
                );

            if (found)
            {
                return ch;
            }
        }

        throw new AggregateException($"Unrecognized letter: \n{textRect}\n");
    }

    private static string GetRect(string[] st, int icol0, int irow0, int ccol, int crow)
    {
        StringBuilder builder = new();
        for (int irow = irow0; irow < irow0 + crow; irow++)
        {
            for (int icol = icol0; icol < icol0 + ccol; icol++)
            {
                char ch = irow < st.Length && icol < st[irow].Length ? st[irow][icol] : ' ';
                builder.Append(ch);
            }

            if (irow + 1 != irow0 + crow)
            {
                builder.Append('\n');
            }
        }

        return builder.ToString();
    }
}
