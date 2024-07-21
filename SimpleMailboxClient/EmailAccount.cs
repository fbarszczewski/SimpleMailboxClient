namespace ConsoleMailbox.Entities;

public class EmailAccount
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public ImapConfig? ImapConfig { get; set; }
    public SmtpConfig? SmtpConfig { get; set; }
}

public class ImapConfig
{
    public required string Server { get; set; }
    public required int Port { get; set; }
    public EncryptionType SecurityProtocol { get; set; } = EncryptionType.SSL;
    public int IdleTimeout { get; set; } = 60000;
}

public class SmtpConfig
{
    public required string Server { get; set; }
    public required int Port { get; set; }
    public EncryptionType SecurityProtocol { get; set; } = EncryptionType.SSL;
}

public enum EncryptionType
{
    None,
    SSL,
    TLS,
    StartTLS
}