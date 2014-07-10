namespace Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class TestController : ApiController
    {
        public async Task<HttpResponseMessage> Get()
        {
            return this.Request.CreateResponse(new TestModel("Get()"));
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
