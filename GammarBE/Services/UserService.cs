using GammarBE.Models.DTO;
using GammarBE.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GammarBE.Services
{
    public interface IUserService
    {
        Task<dynamic> GetProfileAsync(Guid userId);
        Task<dynamic> UpdateProfileAsync(Guid userId, UpdateProfileReqDTO req);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetProfileAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Select(u => new
                    {
                        u.Id,
                        u.Fullname,
                        u.Email,
                        u.CreatedAt
                    })
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Không tìm thấy thông tin người dùng",
                        data = (object)null!
                    };
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

                var usage = await _context.UserUsages
                    .AsNoTracking()
                    .Where(u => u.UserId == userId && u.Date == today)
                    .Select(u => new
                    {
                        TotalGen = u.TotalGen ?? 0,
                        TotalCost = u.TotalCost ?? 0m
                    })
                    .FirstOrDefaultAsync();

                return new
                {
                    code = 200,
                    message = "Lấy thông tin thành công",
                    data = new
                    {
                        fullname = user.Fullname,
                        email = user.Email,
                        createdAt = user.CreatedAt,
                        totalGen = usage?.TotalGen ?? 0,
                        totalCost = usage?.TotalCost ?? 0m
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi lấy thông tin người dùng: " + ex.Message,
                    data = (object)null!
                };
            }
        }

        public async Task<dynamic> UpdateProfileAsync(Guid userId, UpdateProfileReqDTO req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new
                    {
                        code = 404,
                        message = "Người dùng không tồn tại",
                        data = (object)null!
                    };
                }

                // Kiểm tra email trùng với người dùng khác
                if (user.Email != req.Email)
                {
                    var isEmailExist = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Email == req.Email && u.Id != userId);

                    if (isEmailExist)
                    {
                        return new
                        {
                            code = 400,
                            message = "Email này đã được sử dụng bởi tài khoản khác",
                            data = (object)null!
                        };
                    }
                }

                user.Fullname = req.Fullname;
                user.Email = req.Email;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    code = 200,
                    message = "Cập nhật thông tin thành công",
                    data = new
                    {
                        fullname = user.Fullname,
                        email = user.Email
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    code = 500,
                    message = "Đã có lỗi xảy ra khi cập nhật thông tin: " + ex.Message,
                    data = (object)null!
                };
            }
        }
    }
}
