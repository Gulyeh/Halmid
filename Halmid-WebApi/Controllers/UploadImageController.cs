using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;

namespace Halmid_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadImageController : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public IActionResult Post(JObject data)
        {
            if(data["Post_Data"].ToString() == "Message")
            {
                if (data["channelID"] != null && data["ImageID"] != null && data["Base64"] != null)
                {
                    if (System.IO.Directory.Exists("C:/xampp/htdocs/Channels/" + data["channelID"].ToString()))
                    {
                        System.IO.File.WriteAllBytes("C:/xampp/htdocs/Channels/" + data["channelID"].ToString() + "/" + data["ImageID"].ToString() + ".png", Convert.FromBase64String(data["Base64"].ToString()));
                    }
                }
            }
            else if (data["Post_Data"].ToString() == "User_Avatar")
            {
                if (data["userID"] != null && data["ImageID"] != null && data["Base64"] != null)
                {
                    if (System.IO.Directory.Exists("C:/xampp/htdocs/Users/" + data["userID"].ToString()))
                    {
                        System.IO.File.WriteAllBytes("C:/xampp/htdocs/Users/"+ data["userID"].ToString() + "/" + data["ImageID"].ToString() + ".png", Convert.FromBase64String(data["Base64"].ToString()));
                    }
                }
            }
            else if (data["Post_Data"].ToString() == "Channel_Avatar")
            {
                if (data["ImageID"] != null && data["Base64"] != null)
                {
                    if (System.IO.Directory.Exists("C:/xampp/htdocs/Channels"))
                    {
                        System.IO.File.WriteAllBytes("C:/xampp/htdocs/Channels/" + data["ImageID"].ToString() + ".png", Convert.FromBase64String(data["Base64"].ToString()));
                    }
                }
            }
            return Ok("Done");
        }
    }
}
