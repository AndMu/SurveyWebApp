using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using NLog;
using ServiceStack.Redis;
using Wikiled.Survey.Data;
using Wikiled.Survey.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wikiled.Survey.Controllers
{
    [Route("api/[controller]")]
    public class SurveyController : Controller
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IRedisClientsManager cache;

        public SurveyController(IRedisClientsManager cache)
        {
            this.cache = cache;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] SurveyData value)
        {
            var ip = new IpResolve(HttpContext).GetRequestIP();
            logger.Debug("Saving data: {0}...", ip);
            Save(ip, value);
        }

        private void Save(string ip, SurveyData value)
        {
            using (var client = cache.GetClient())
            {
                foreach (var localAuthority in value.Data.Where(item => item.Active).OrderBy(item => item.Code))
                {
                    var line = $"{DateTime.Now},{ip},{value.PostCode},{value.Authority},{localAuthority.Code},{localAuthority.RatingOne},{localAuthority.RatingTwo}";
                    client.AddItemToList("Survey:Results", line);
                }
            }
        }
    }
}
