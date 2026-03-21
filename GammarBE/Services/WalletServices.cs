using GammarBE.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GammarBE.Services
{
    public interface IWalletService
    {
        Task<dynamic> GetBalanceAsync(Guid userId);
    }

    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
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
    }
}

