namespace SimpleMailboxClient.Entities;

public class Email
{
    public required string MessageId { get; set; }
    public string Subject { get; set; }
    public string Author { get; set; }
    public string From { get; set; }
    public string[] To { get; set; }
    public string[] Cc { get; set; }
    public string TextBody { get; set; }
    public DateTime Date { get; set; }
    //Folder on mailbox where  message is stored
    public string Folder { get; set; }
}