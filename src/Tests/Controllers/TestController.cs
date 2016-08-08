namespace Tests.Controllers
{
    using global::Tests.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class TestController : ApiController
    {
        private IAuthenticationManager Authentication => this.Request.GetOwinContext().Authentication;

        public async Task<HttpResponseMessage> Get()
        {
            return this.Request.CreateResponse(new TestModel("Get()"));
        }

        [Route("api/test/customheader")]
        public async Task<HttpResponseMessage> GetCustomHeader()
        {
            var response = this.Request.CreateResponse(new TestModel("Get()"));
            response.Headers.Add("DataServiceVersion", "3.0");

            return response;
        }

        [Route("api/test/customcontentencoding")]
        public async Task<HttpResponseMessage> GetCustomContentEncoding()
        {
            var response = this.Request.CreateResponse(new TestModel("Get()"));
            response.Content.Headers.ContentEncoding.Add("lol");

            return response;
        }

        [Route("api/test/redirect")]
        public async Task<HttpResponseMessage> GetRedirect()
        {
            var response = this.Request.CreateResponse(HttpStatusCode.Redirect);
            response.Content = new ByteArrayContent(new byte[1024]);
            response.Content.Headers.ContentLength = 1024;
            response.Headers.Location = new Uri($"{this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Authority}/api/test");

            return response;
        }

        public async Task<HttpResponseMessage> Get(string id)
        {
            return this.Request.CreateResponse(new TestModel("Get(" + id + ")"));
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post(TestModel m)
        {
            return this.Request.CreateResponse(m);
        }

        [HttpPost]
        [Route("api/test/login")]
        public async Task<HttpResponseMessage> Login(LoginModel m)
        {
            if (m.Username == m.Password)
            {
                this.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                this.Authentication.SignIn(
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1),
                        RedirectUri = m.ReturnUrl
                    },
                    new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, m.Username) }, DefaultAuthenticationTypes.ApplicationCookie));

                var response = this.Request.CreateResponse(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(m.ReturnUrl);

                return response;
            }

            return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Put(string id, TestModel m)
        {
            return this.Request.CreateResponse(m);
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(string id)
        {
            return this.Request.CreateResponse();
        }
    }
}
