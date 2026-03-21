using GammarBE.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Threading.Tasks;

namespace GammarBE.Services
{
    public interface IWalletService
    {
        Task<dynamic> GetBalanceAsync(Guid userId);
        Task<dynamic> Create(long Amount);
    }

    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;

        public WalletService(AppDbContext context  , IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<dynamic> GetBalanceAsync(Guid userId)
        {
            try
            {
                var wallet = await _context.Wallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Không tìm thấy ví của người dùng",
                        data = 0m
                    };
                }

                return new
                {
                    code = 200,
                    message = "Lấy số dư thành công",
                    data = wallet.Balance ?? 0m
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi lấy số dư: " + ex.Message,
                    data = 0m
                };
            }
        }


        public async Task<dynamic> Create(long Amount)
        {
            var clientId = _configuration["PayOs:ClientId"];
            var apiKey = _configuration["PayOs:ApiKey"];
            var checksumKey = _configuration["PayOs:ChecksumKey"];

            var payOS = new PayOSClient(clientId, apiKey, checksumKey);

            var domain = "https://audrina-subultimate-ghostily.ngrok-free.dev/api/Wallet/payos-return";


            var paymentLinkRequest = new CreatePaymentLinkRequest
            {
                OrderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")),
                Amount = Amount,
                Description = "Thanh toan don hang",
                ReturnUrl = domain,
                CancelUrl = domain
            };
            var response = await payOS.PaymentRequests.CreateAsync(paymentLinkRequest);


            return new { response };
        }
    }
}

