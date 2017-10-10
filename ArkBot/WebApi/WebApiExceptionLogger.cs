using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace ArkBot.WebApi
{
    public class WebApiExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            //context.ExceptionContext.ActionContext.ActionDescriptor.ActionName
            //context.ExceptionContext.ControllerContext.ControllerDescriptor.ControllerName
            Logging.LogException(context.Exception.Message, context.Exception, GetType(), LogLevel.WARN, ExceptionLevel.Unhandled);
        }
    }
}
