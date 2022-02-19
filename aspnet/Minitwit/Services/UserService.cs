using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;

namespace Minitwit.Services
{
    public class UserService : IUserService
    {
        private readonly MinitwitContext _context;

        public UserService(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<Result> CreateUser(UserRegistrationDTO userRegistrationDto)
        {

            //Todo real user creation this is just so i can test the simulator api
            _context.Users.Add(new User()
            {
                UserName = userRegistrationDto.username,
                Email = userRegistrationDto.email,
                PasswordHash = userRegistrationDto.pwd,
            });
            await _context.SaveChangesAsync();
            return Result.Created;
        }
    }
}

