using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications
{
    public class PrivacyUtil : IPrivacyUtil
    {
        public string ObfuscateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return string.Empty;
            }

            const string pattern = @"(?<=[\w]{1})[\w\-._\+%]*(?=[\w]{1}@)";
            string result = Regex.Replace(email, pattern, m => new string('*', m.Length));
            return result;
        }

        public string ObfuscatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            if (phoneNumber.Length <= 4)
            {
                return phoneNumber;
            }

            int lengthToObfuscate = phoneNumber.Length - 4;
            string obfuscatedPart = new('*', lengthToObfuscate);
            var visiblePart = phoneNumber.Substring(lengthToObfuscate);

            return obfuscatedPart + visiblePart;
        }
    }
}
