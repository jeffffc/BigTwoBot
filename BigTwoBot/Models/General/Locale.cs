using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BigTwoBot.Models
{
    public class Locale
    {
        public string Language { get; set; }
        public XDocument XMLFile { get; set; }
        public string LanguageName { get; set; }
    }
}