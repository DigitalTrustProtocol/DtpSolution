using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DtpCore.Model
{
    public class SchemaValidationResult
    {
        public const string MssingErrorTemplate = "{0}{1} is missing.";
        public const string MaxRangeErrorTemplate = "{0}{1} may not be longer than {2} - is {3} bytes.";
        public const string NotSupportedErrorTemplate = "{0}{1} is not supported.";
        public const string DefinedErrorTemplate = "{0}{1} has to be empty.";

        public const int DEFAULT_MAX_LENGTH = 127;


        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public int ErrorsFound
        {
            get
            {
                return Errors.Count;
            }
        }

        public SchemaValidationResult()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool MissingCheck(string name, Identity value, string location)
        {
            if (value == null)
            {
                this.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                return true;
            }

            return false;
        }


        public bool MissingCheck(string name, string value, string location)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                this.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                return true;
            }

            return false;
        }

        public bool MissingCheck(string name, byte[] value, string location)
        {
            if (value == null || value.Length == 0)
            {
                this.Errors.Add(string.Format(MssingErrorTemplate, location, name));
                return true;
            }

            return false;
        }

        public bool NotEmptyCheck(string name, string value, string location)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            this.Errors.Add(string.Format(DefinedErrorTemplate, location, name));
            return true;
        }

        public bool NotEmptyCheck(string name, byte[] value, string location)
        {
            if (value == null || value.Length == 0)
            {
                return true;
            }

            this.Errors.Add(string.Format(DefinedErrorTemplate, location, name));
            return false;
        }

        public bool MaxRangeCheck(string name, string value, string location, int maxLength)
        {
            if (value == null)
                return true;

            if (value.Length > maxLength)
            {
                this.Errors.Add(string.Format(MaxRangeErrorTemplate, location, name, maxLength, value.Length));
                return false;
            }
            return true;
        }

        public bool MaxRangeCheck(string name, byte[] value, string location, int maxLength)
        {
            if (value == null)
                return true;

            if (value.Length > maxLength)
            {
                this.Errors.Add(string.Format(MaxRangeErrorTemplate, location, name, maxLength, value.Length));
                return false;
            }
            return true;
        }

    }
}
