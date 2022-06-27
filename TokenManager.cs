using System.Security.Cryptography;
namespace DHT;

public class TokenManager : IDisposable
{
    public byte[] Token { get; private set; }
    private readonly Timer _timer;
    public TokenManager()
    {
        Token = GenerateToken();
        _timer = new Timer((obj) =>
        {
            Token = GenerateToken();
        }, null, 60000 * 15, 60000 * 15);
    }

    private static byte[] GenerateToken()
    {
        var bytes = new byte[8];
        using var ctx = RandomNumberGenerator.Create();
        ctx.GetBytes(bytes);
        return bytes;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
