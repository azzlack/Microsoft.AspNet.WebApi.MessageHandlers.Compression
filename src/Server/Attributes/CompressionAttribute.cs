namespace Microsoft.AspNet.WebApi.Extensions.Compression.Server.Attributes
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public class CompressionAttribute : ActionFilterAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="CompressionAttribute" /> class.</summary>
        public CompressionAttribute()
        {
            this.Enabled = true;
        }

        /// <summary>Gets or sets a value indicating whether to enable compression for this request.</summary>
        /// <value><c>true</c> if enabled, <c>false</c> if not. The default value is <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>Executes the action executing asynchronous action.</summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            actionContext.Request.Properties.Add("compression:Enable", this.Enabled);

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}