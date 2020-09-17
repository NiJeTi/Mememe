namespace Mememe.Service.Configurations
{
    public class MongoConfiguration
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 27017;
        
        public string Database { get; set; } = string.Empty;
        
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AuthMechanism { get; set; } = "SCRAM-SHA-1";
    }
}