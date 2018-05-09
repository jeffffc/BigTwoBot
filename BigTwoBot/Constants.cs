using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTwoBot
{
    public static class Constants
    {
        // Token from registry
        private static RegistryKey _key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\BigTwo");
        public static string GetBotToken(string key)
        {
            return _key.GetValue(key, "").ToString();
        }
        private static string _logPath = @"C:\Logs\BigTwo.log";
        public static string GetLogPath()
        {
            return Path.GetFullPath(_logPath);
        }
        private static string _languageDirectory = @"C:\BigTwoLanguages";
        private static string _languageTempDirectory = Path.Combine(_languageDirectory, @"temp\");
        public static string GetLangDirectory(bool temp = false)
        {
            return (!temp) ? Path.GetFullPath(_languageDirectory) : Path.GetFullPath(_languageTempDirectory);
        }
        public static long LogGroupId = -1001347944461;
        public static int[] Dev = new int[] { 106665913 };

        #region GameConstants
        public static int JoinTime = 120;
        public static int JoinTimeMax = 300;
#if DEBUG
        public static int ChooseCardTime = 30;
#else
        public static int ChooseCardTime = 45;
#endif
        public static int ExtendTime = 30;
        public static int WitnessTime = 15;

        public static int ChipsPerCard = 10;
        public static int PlayerDefaultChips = 10000;

        #endregion

        public static string DonationLiveToken = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\BigTwo").GetValue("DonationLiveToken").ToString();
        public static string DonationPayload = "CRIMINALDANCEBOTPAYLOAD:";
    }
}
