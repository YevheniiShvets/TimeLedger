using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Users;

namespace TimeLedger.Core.Interfaces.Users;

public interface IUserService
{
    AccountInfoDto Register(RegisterDto dto);
    AccountInfoDto Login(LoginDto dto);
    AccountInfoDto GetById(int id);
    AccountInfoDto Update(int id, UpdateAccountDto dto);
    void Delete(int id);
}

