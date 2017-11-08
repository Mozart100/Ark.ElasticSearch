using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.TracePyramid
{
    public abstract class SerilogBookmarkBase
    {
        public const string LableName = "Bookmark";
        public abstract string Topic { get;  }
    }
}
