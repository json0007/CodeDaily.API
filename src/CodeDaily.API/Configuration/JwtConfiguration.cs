using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace CodeDaily.API.Features.Auth;

public class JwtConfiguration
{
    private readonly Lazy<RsaSecurityKey> _privateKey;
    private readonly Lazy<RsaSecurityKey> _publicKey;
    
    public int Expiration { get; }
    
    public RsaSecurityKey PrivateKey => _privateKey.Value;
    public RsaSecurityKey PublicKey => _publicKey.Value;

    public JwtConfiguration(string privateKey, string publicKey, int expirationInMinutes)
    {
        Expiration = expirationInMinutes;
        _privateKey = new Lazy<RsaSecurityKey>(() => LoadKey(privateKey));
        _publicKey = new Lazy<RsaSecurityKey>(() => LoadKey(publicKey));
    }

    private static RsaSecurityKey LoadKey(string key)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(key);
        return new RsaSecurityKey(rsa);
    }
}