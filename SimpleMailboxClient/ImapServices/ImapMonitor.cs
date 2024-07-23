using MailKit;
using MailKit.Net.Imap;
using SimpleMailboxClient.Entities;

namespace SimpleMailboxClient.ImapServices;

public class ImapMonitor : IDisposable
{
    private readonly ImapClientProvider _clientProvider;
    private readonly int _idleTimeout;
    private ImapClient _client;
    private CancellationTokenSource _cancelMonitor;
    private CancellationTokenSource _doneIdle;

    public ImapMonitor(EmailConfig emailConfig)
    {
        _idleTimeout = emailConfig.ImapConfig!.IdleTimeout;
        _clientProvider = new ImapClientProvider(emailConfig);
    }
    public event EventHandler<EventArgs> NewMail;
    public event EventHandler<EventArgs> RemovedMail;
    public event EventHandler<EventArgs> ChangedMailFlag;

    public async Task MonitorAsync()
    {
        try
        {
            await ConfigureClientConnection();
        }
        catch (OperationCanceledException)
        {
            await _client.DisconnectAsync(true);
        }

        var inbox = _client.Inbox;

        inbox.CountChanged += OnCountChanged;
        inbox.MessageExpunged += OnMessageExpunged;
        inbox.MessageFlagsChanged += OnMessageFlagsChanged;
        Console.WriteLine("## Starting monitoring mailbox..");
        await EnterIdle();

        inbox.MessageFlagsChanged -= OnMessageFlagsChanged;
        inbox.MessageExpunged -= OnMessageExpunged;
        inbox.CountChanged -= OnCountChanged;

        await _client.DisconnectAsync(true);
    }

    private async Task ConfigureClientConnection()
    {
        _cancelMonitor = new CancellationTokenSource();
        _client = await _clientProvider.GetClientAsync(_cancelMonitor);
        await _client.Inbox.OpenAsync(FolderAccess.ReadOnly, _cancelMonitor.Token);
    }

    private async Task EnterIdle()
    {
        do
        {
            try
            {
                await WaitForNewMessagesAsync();

            }
            catch (OperationCanceledException)
            {
                break;
            }
        } while (!_cancelMonitor.IsCancellationRequested);
    }

    async Task WaitForNewMessagesAsync()
    {
        do
        {
            try
            {
                if (_client.Capabilities.HasFlag(ImapCapabilities.Idle))
                {
                    // Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
                    // we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
                    // about 10 minutes, so we'll only idle for 9 minutes.
                    _doneIdle = new CancellationTokenSource(new TimeSpan(0, _idleTimeout, 0));
                    try
                    {
                        await _client.IdleAsync(_doneIdle.Token, _cancelMonitor.Token);
                    }
                    finally
                    {
                        _doneIdle.Dispose();
                        _doneIdle = null;
                    }
                }
                else
                {
                    // Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
                    // between each NOOP command.
                    await Task.Delay(new TimeSpan(0, 1, 0), _cancelMonitor.Token);
                    await _client.NoOpAsync(_cancelMonitor.Token);
                }
                break;
            }
            catch (ImapProtocolException)
            {
                // protocol exceptions often result in the client getting disconnected
                await ConfigureClientConnection();
            }
            catch (IOException)
            {
                // I/O exceptions always result in the client getting disconnected
                await ConfigureClientConnection();
            }
        } while (true);
    }


    void OnCountChanged(object sender, EventArgs e)
    {
        Console.WriteLine("## New message has arrived.");
        NewMail?.Invoke(this, e);
    }

    void OnMessageExpunged(object sender, MessageEventArgs e)
    {
        Console.WriteLine("## Message removed");
        RemovedMail?.Invoke(this, e);
    }

    void OnMessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
    {
        var folder = (ImapFolder)sender;

        Console.WriteLine("## {0}: flags have changed for message #{1} ({2}).", folder, e.Index, e.Flags);
        ChangedMailFlag?.Invoke(this, e);
    }

    public void Exit()
    {
        _cancelMonitor.Cancel();
    }

    public void Dispose()
    {
        _client.Dispose();
        _cancelMonitor.Dispose();
    }

}