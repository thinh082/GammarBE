using GammarBE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CommonServices _commonServices;
        public CommonController(IConfiguration configuration,CommonServices commonServices)
        {
            _configuration = configuration;
            _commonServices = commonServices;   
        }
        //public Task<IActionResult> HashAccesskey() 
        //{

        //}
    }
}
