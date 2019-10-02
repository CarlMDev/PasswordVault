namespace PasswordVault
{
    public class Company
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; } 

        public Company(int id, string name, string login, string password)
        {
            Id = id;
            Name = name;
            Login = login;
            Password = password;
        }

        public Company(string name, string login, string password)
        {
            Name = name;
            Login = login;
            Password = password;
        }
    }
}