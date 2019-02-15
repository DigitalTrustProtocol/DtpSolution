using System.IO;

namespace DtpCore.Model.Configuration
{
    public class ServerSection
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }

        //private const string KEYWORDPATH = @"C:\tmp\ServerKeyword.txt";


        public string GetSecureKeyword()
        {
            if (Keyword != null)
                return Keyword;

            //if(File.Exists(KEYWORDPATH))
            //{
            //    // Returns Unicode (UTF16)
            //    return File.ReadAllText(KEYWORDPATH);
            //}
            return string.Empty;
        }
    }
}
