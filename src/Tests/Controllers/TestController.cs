namespace Tests.Controllers
{
    using global::Tests.Models;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class TestController : ApiController
    {
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

        [Route("api/test/redirect")]
        public async Task<HttpResponseMessage> GetRedirect()
        {
            var response = this.Request.CreateResponse(HttpStatusCode.Redirect);
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
