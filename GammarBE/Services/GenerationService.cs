using GammarBE.Models.Entities;

namespace GammarBE.Services
{
    public class GenerationService
    {
        private readonly AppDbContext _context;
        public GenerationService(AppDbContext context)
        {
            _context = context;
        }
        public Task<dynamic> GenerateImg()
        {

        }
    }
}
