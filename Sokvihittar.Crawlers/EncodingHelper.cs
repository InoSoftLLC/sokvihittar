using System.Collections.Generic;

namespace Sokvihittar.Crawlers
{
    public static class EncodingHelper
    {   
        /// <summary>
        /// Dictionary contains code pages of encodings. Key is encoding name. Value is code page.
        /// </summary>
        public static Dictionary<string,int> CodePages = new Dictionary<string, int>
        {
            {"iso-8859-1", 28591}
        };
 
    }
}
