using GammarBE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace GammarBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {     
        [HttpGet("GetKey")]
        public Task<IActionResult> GetKey()
        {
            const int keySize = 2048;
            string privateKey;
            string publicKey;
            
            using (var rsa = RSA.Create(keySize))
            {
                // Xuất Private Key (Bao gồm cả thông số công khai)
                privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                // Xuất Public Key
                publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            }
            
            return Task.FromResult<IActionResult>(Ok(new { publicKey, privateKey }));
        }
    }
}
