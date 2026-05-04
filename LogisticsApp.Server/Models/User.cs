namespace LogisticsApp.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; //Пока что текст
        public string Role { get; set; } = string.Empty; //"Admin","SuperUser","User"
    }
}