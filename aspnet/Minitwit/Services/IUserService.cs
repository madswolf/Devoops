using Minitwit.Models.DTO;

namespace Minitwit.Services
{
    public interface IUserService
    {
        public Task<Result> CreateUser(UserCreationDTO userCreationDTO);
    }
}
