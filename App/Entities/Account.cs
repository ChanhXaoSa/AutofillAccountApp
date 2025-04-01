using App.Common.Encrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entities
{
    public class Account
    {
        public string Username { get; set; }
        private string _password;
        public string Password
        {
            get => EncryptionHelper.Decrypt(_password);
            set => _password = EncryptionHelper.Encrypt(value);
        }
        public string AppName { get; set; }
        public string DisplayName => $"{AppName} - {Username}";
    }
}
