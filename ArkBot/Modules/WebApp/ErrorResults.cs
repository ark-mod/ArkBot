using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace ArkBot.Modules.WebApp
{
    [DefaultStatusCode((int)HttpStatusCode.InternalServerError)]
    public class InternalServerErrorResult : StatusCodeResult
    {
        public InternalServerErrorResult() : base(0) { }
    }

    [DefaultStatusCode((int)HttpStatusCode.InternalServerError)]
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult([ActionResultObjectValue] ModelStateDictionary modelState) : base(modelState) { }
        public InternalServerErrorObjectResult([ActionResultObjectValue] object error) : base(error) { }
    }
}
