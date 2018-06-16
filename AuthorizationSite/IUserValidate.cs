public interface IUserValidate
{
    UserModel GetUserByContext(string userName,string password);    
}