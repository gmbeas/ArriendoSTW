using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionNotificacionArriendo
{
    internal class IniFile
    {
        private string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        private string Path;

        public IniFile(string IniPath = null)
        {
            this.Path = new FileInfo(IniPath ?? this.EXE + ".ini").FullName.ToString();
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public string Read(string Key, string Section = null)
        {
            StringBuilder RetVal = new StringBuilder((int)byte.MaxValue);
            IniFile.GetPrivateProfileString(Section ?? this.EXE, Key, "", RetVal, (int)byte.MaxValue, this.Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            IniFile.WritePrivateProfileString(Section ?? this.EXE, Key, Value, this.Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            this.Write(Key, (string)null, Section ?? this.EXE);
        }

        public void DeleteSection(string Section = null)
        {
            this.Write((string)null, (string)null, Section ?? this.EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return this.Read(Key, Section).Length > 0;
        }
    }
}
