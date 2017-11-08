using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.TracePyramid
{
    public class TraceInfo
    {
        public TraceInfo()
        {
            Bookmarks = new List<string> ();

        }
        public ILogger Logger { get; set; }
        public string Bookmark { get { return string.Join("|", Bookmarks); } }

              
        public List<string> Bookmarks { get;  }
    }
}
