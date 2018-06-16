public class UserValidate : IUserValidate
{
    public UserModel GetUserByContext(string userName, string password)
    {
        UserModel rct = null;
        if (userName == "moto" && password == "P@sw0rd123")
        {
            rct = new UserModel { UserName = userName, UniqueId = "1234567890" };
        }

        return rct;
    }
}