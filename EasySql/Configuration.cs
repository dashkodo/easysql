namespace EasySql;

public class Configuration
{
    public List<Connection> Databases { get; set; } = new();
}
public class Connection
{
    public string Name { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;
}