using System.Net.Sockets;
using MailKit.Net.Imap;
using MailKit.Security;
using SimpleMailboxClient.Entities;

namespace SimpleMailboxClient.ImapServices;

public class ImapClientProvider
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly EncryptionType _encryptionType;
    private readonly ImapClient _client;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private SecureSocketOptions SecurityProtocol
    {
        get
        {
            switch (_encryptionType)
            {
                case EncryptionType.None:
                    return SecureSocketOptions.None;
                case EncryptionType.SSL:
                    return SecureSocketOptions.SslOnConnect;
                case EncryptionType.TLS:
                    return SecureSocketOptions.StartTls;
                default:
                    return SecureSocketOptions.Auto;
            }
        }
    }

    public ImapClientProvider(EmailAccount emailAccount)
    {
        if (emailAccount.ImapConfig == null)
            throw new ArgumentNullException(nameof(emailAccount.ImapConfig), "ImapConfig cannot be null.");

        if (string.IsNullOrWhiteSpace(emailAccount.Username) || string.IsNullOrWhiteSpace(emailAccount.Password))
            throw new ArgumentNullException(nameof(emailAccount), "Username & Password cannot be null.");

        _host = emailAccount.ImapConfig.Server;
        _port = emailAccount.ImapConfig.Port;
        _username = emailAccount.Username;
        _password = emailAccount.Password;
        _encryptionType = emailAccount.ImapConfig.SecurityProtocol;
        _client = new ImapClient();
    }

    public async Task<ImapClient> GetClientAsync(CancellationTokenSource cancellationToken)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!_client.IsConnected)
                await ReConnectAsync(cancellationToken);

            if (!_client.IsAuthenticated)
                await ReAuthenticateAsync(cancellationToken);

            return _client;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DisposeAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(true);
            }
        }
        finally
        {
            _client.Dispose();
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }

    private async Task ReConnectAsync(CancellationTokenSource cancellationToken)
    {
        if (!_client.IsConnected)
        {
            try
            {
                await _client.ConnectAsync(_host, _port, SecurityProtocol, cancellationToken.Token);
                Console.WriteLine("Imap Connected.");
            }
            catch (SocketException ex)
            {
                throw new Exception(
                    $"Failed to connect to IMAP server at {_host}:{_port}. Please check your internet connection and server details.",
                    ex);
            }
            catch (ImapProtocolException ex)
            {
                throw new Exception(
                    $"IMAP protocol error occurred while connecting to {_host}:{_port}. The server may not support IMAP or SSL/TLS.",
                    ex);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An unexpected error occurred while connecting to IMAP server at {_host}:{_port}.", ex);
            }
        }
    }

    private async Task ReAuthenticateAsync(CancellationTokenSource cancellationToken)
    {
        if (!_client.IsAuthenticated)
        {
            try
            {
                await _client.AuthenticateAsync(_username, _password, cancellationToken.Token);
                Console.WriteLine("Authenticated.");
            }
            catch (AuthenticationException ex)
            {
                throw new Exception(
                    $"Authentication failed for user '{_username}'. Please check your username and password.",
                    ex);
            }
            catch (ImapCommandException ex)
            {
                throw new Exception(
                    "IMAP command error occurred during authentication. The server may have rejected the login attempt.",
                    ex);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An unexpected error occurred during authentication for user '{_username}'.", ex);
            }
        }
    }

    private async Task ConnectAndAuthenticateAsync(CancellationTokenSource cancellationToken)
    {

        try
        {
            await _client.ConnectAsync(_host, _port, SecurityProtocol, cancellationToken.Token);
            Console.WriteLine("Imap Connected.");
        }
        catch (SocketException ex)
        {
            throw new Exception(
                $"Failed to connect to IMAP server at {_host}:{_port}. Please check your internet connection and server details.",
                ex);
        }
        catch (ImapProtocolException ex)
        {
            throw new Exception(
                $"IMAP protocol error occurred while connecting to {_host}:{_port}. The server may not support IMAP or SSL/TLS.",
                ex);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An unexpected error occurred while connecting to IMAP server at {_host}:{_port}.", ex);
        }

        try
        {
            await _client.AuthenticateAsync(_username, _password, cancellationToken.Token);
            Console.WriteLine("Authenticated.");
        }
        catch (AuthenticationException ex)
        {
            throw new Exception(
                $"Authentication failed for user '{_username}'. Please check your username and password.",
                ex);
        }
        catch (ImapCommandException ex)
        {
            throw new Exception(
                "IMAP command error occurred during authentication. The server may have rejected the login attempt.",
                ex);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"An unexpected error occurred during authentication for user '{_username}'.", ex);
        }

        Console.WriteLine("Created connection.");

    }
}