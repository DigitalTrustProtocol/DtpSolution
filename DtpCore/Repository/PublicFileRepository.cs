using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DtpCore.Repository
{
    public class PublicFileRepository
    {
        public const string PUBLIC = "public";
        public const string REQUESTPATH = @"\public";

        public static string FilePath
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), PUBLIC);
            }
        }

        public bool Exist(string name)
        {
            var path = Path.Combine(FilePath, name);
            return File.Exists(path);
        }

        public void WriteFile(string name, string contents)
        {
            File.WriteAllText(name, contents);
        }
    }
}



