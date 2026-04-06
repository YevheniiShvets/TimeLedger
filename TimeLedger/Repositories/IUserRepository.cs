using TimeLedger.Models;

namespace TimeLedger.Repositories;

public interface IUserRepository
{
    User? GetById(int id);
    User? GetByEmail(string email);
    User Add(User user);
    User Update(User user);
    void Delete(User user);
    bool Exists(string email);
}