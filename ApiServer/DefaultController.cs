using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiServer
{
    public class DefaultController : ApiController
    {
        [Route("test")]
        [HttpGet]
        public string Test()
        {
            return Request.Content.ReadAsStringAsync().Result;
        }
    }
}