using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ark.TracePyramid
{
    public static class TraceInfoExtensions
    {

        public static void Mark(this TraceInfo traceInfo, Action<ILogger> callback)
        {
            using (LogContext.PushProperty(SerilogBookmarkBase.LableName, traceInfo.Bookmark))
            {
                callback(traceInfo.Logger);
                //traceInfo.Logger.Information("line {@item}", item);
            }
        }

        public static TraceInfo MarkOn<TBookmark>(this TraceInfo traceInfo, Expression<Func<TBookmark, string>> bookMarkName)
            where TBookmark : SerilogBookmarkBase
        {
            var memerExpre = bookMarkName.Body as MemberExpression;

            if (memerExpre == null)
            {
                throw new Exception("asdsa");
            }

            var propertyName = memerExpre.Member.Name;

            var instance = Activator.CreateInstance(typeof(TBookmark));
            var property = instance.GetType().GetProperty(propertyName);
            var value = property.GetValue(instance).ToString();


            return MarkOn(traceInfo, value); ;
        }

        public static TraceInfo MarkOn(this TraceInfo traceInfo, string bookmarkName)
        {
            traceInfo.Bookmarks.Add(bookmarkName);

            return traceInfo;
        }
    }
}
