namespace SimpleMailboxClient.Entities
{
    public class EmailAttachment
    {

        public string MessageId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long Size { get; set; }
    }
}
