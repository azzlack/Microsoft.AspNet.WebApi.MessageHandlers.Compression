namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes
{
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public class CompressionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes.CompressionAttribute class.
        /// </summary>
        public CompressionAttribute()
        {
            this.Enabled = true;
        }

        /// <summary>Gets or sets a value indicating whether to enable compression for this request.</summary>
        /// <value><c>true</c> if enabled, <c>false</c> if not. The default value is <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Request.Properties.Add("compression:Enable", this.Enabled);

            base.OnActionExecuting(actionContext);
        }
    }
}