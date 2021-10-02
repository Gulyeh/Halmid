using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Halmid_WebApi.Controllers
{
    [Route("api/Security/[controller]")]
    [ApiController]
    public class GetToken : ControllerBase
    {
        private class Data
        {
            public string Token { get; set; }
        }

        [HttpPost]
        public IActionResult Generate(JObject data)
        {
            try
            {
                if (data["Login"].ToString() == "balistic" && data["Pass"].ToString() == "airplane")
                {
                    string Token = GenerateToken.JwtGenerate();
                    var _Token = new Data()
                    {
                        Token = Token
                    };
                    var JsonData = JsonConvert.SerializeObject(_Token);
                    return Ok(JsonData);
                }
                else
                {
                    return Ok("Error");
                }
            }
            catch (Exception) { return Ok("Error"); }
        }
    }
}
