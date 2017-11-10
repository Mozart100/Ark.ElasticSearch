using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.TracePyramid
{
    public class ElasticConfig
    {
        public ElasticConfig(string uri, string index)
        {
            Uri = uri;
            Index = index;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------

        public string Uri { get; }
        public string Index { get; }
    }

}
