namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes
{
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public class DisableCompressionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Request.Properties.Add("DisableCompression", true);

            base.OnActionExecuting(actionContext);
        }
    }
}