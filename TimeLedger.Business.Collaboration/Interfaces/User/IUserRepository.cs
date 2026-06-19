namespace BusinessCollaboration.Interfaces.User;

public interface IUserRepository
{
    Models.User.User? GetById(int id);
    Models.User.User? GetByEmail(string email);
    Models.User.User Add(Models.User.User user);
    Models.User.User Update(Models.User.User user);
    void Delete(Models.User.User user);
    bool Exists(string email);
}