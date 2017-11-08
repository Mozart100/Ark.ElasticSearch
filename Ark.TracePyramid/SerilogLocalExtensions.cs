using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ark.TracePyramid
{
    public static class SerilogLocalExtensions
    {
        public static TraceInfo ToTraceInfo<TBookmark>(this ILogger logger , 
            Expression<Func<TBookmark,string>> bookMarkName)
            where TBookmark : SerilogBookmarkBase
        {
            var memerExpre = bookMarkName.Body as MemberExpression;
            
            if ( memerExpre == null)
            {
                throw new Exception("asdsa");
            }

            var propertyName = memerExpre.Member.Name;

            var instance = Activator.CreateInstance(typeof(TBookmark));
            var property = instance.GetType().GetProperty(propertyName);
            var value = property.GetValue(instance);

            var tracInfo  =  new TraceInfo { Logger = logger };
            tracInfo.Bookmarks.Add(value.ToString());

            return tracInfo;
        }



    }

    
}
