using SimpleMailboxClient.Utilities;

namespace SimpleMailboxClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing..");

            var emailConfig = SettingsLoader.GetEmailConfig();

            Console.WriteLine($"Loaded {emailConfig.Username} mail configuration.");




            //using (var client = new ImapMonitor(new ImapClientProvider(fufuAccount)))
            //{
            //    Console.WriteLine("Hit any key to end the demo.");

            //    var idleTask = client.MonitorAsync();

            //    Task.Run(() =>
            //    {
            //        Console.ReadKey(true);
            //    }).Wait();

            //    client.Exit();


            //    idleTask.GetAwaiter().GetResult();
            //}

        }
    }
}
