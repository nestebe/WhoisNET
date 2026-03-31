using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DevWorker.WhoisNET.Whois;

/// <summary>
/// Low-level WHOIS TCP client (port 43).
/// </summary>
internal sealed class WhoisTcpClient
{
    private const int WhoisPort = 43;
    private readonly ILogger _logger;

    internal WhoisTcpClient(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Sends a WHOIS query via TCP port 43.
    /// </summary>
    internal async Task<string> QueryAsync(
        string server,
        string query,
        Encoding? encoding = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        _logger.LogDebug("Connecting to WHOIS server {Server}:{Port}", server, WhoisPort);

        using var tcpClient = new TcpClient();

        try
        {
            await tcpClient.ConnectAsync(server, WhoisPort
#if NET6_0_OR_GREATER
                , cts.Token
#endif
            ).ConfigureAwait(false);

#if !NET6_0_OR_GREATER
            cts.Token.ThrowIfCancellationRequested();
#endif

            using var stream = tcpClient.GetStream();
            var queryBytes = encoding.GetBytes(query);
            await stream.WriteAsync(queryBytes, 0, queryBytes.Length
#if NET6_0_OR_GREATER
                , cts.Token
#endif
            ).ConfigureAwait(false);
            await stream.FlushAsync(
#if NET8_0_OR_GREATER
                cts.Token
#endif
            ).ConfigureAwait(false);

            using var reader = new StreamReader(stream, encoding);
            var response = await reader.ReadToEndAsync(
#if NET8_0_OR_GREATER
                cts.Token
#endif
            ).ConfigureAwait(false);

            _logger.LogDebug("Received {Length} chars from {Server}", response.Length, server);
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw new Exceptions.WhoisTimeoutException($"WHOIS query to {server} timed out after {effectiveTimeout.TotalSeconds}s.");
        }
        catch (SocketException ex)
        {
            throw new Exceptions.WhoisNetException($"Failed to connect to WHOIS server {server}:{WhoisPort}.", ex);
        }
    }
}
