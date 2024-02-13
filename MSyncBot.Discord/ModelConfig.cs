namespace MSyncBot.Discord.Config;

public class ModelConfig
{
    public string? BotToken { get; set; }
    
    public string? ServerIp { get; set; }
    public int ServerPort { get; set; }
    
    public string? DatabaseIp { get; set; }
    public string? DatabaseName { get; set; }
    public string? DatabaseUser { get; set; }
    public string? DatabaseUserPassword { get; set; }
}