using WpfAppLayer.Common.Encrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppLayer.Entities
{
    public class Account
    {
        private string _encryptedPassword;
        public string Username { get; set; }
        //private string _password;
        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_encryptedPassword))
                    return string.Empty;
                return EncryptionHelper.Decrypt(_encryptedPassword);
            }
            set
            {
                _encryptedPassword = string.IsNullOrEmpty(value)
                    ? string.Empty
                    : EncryptionHelper.Encrypt(value);
            }
        }
        public string AppName { get; set; }
        public string DisplayName => $"{AppName} - {Username}";
    }
}
