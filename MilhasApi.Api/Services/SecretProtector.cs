using System.Security.Cryptography;
using System.Text;

namespace MilhasApi.Api.Services;

public interface ISecretProtector
{
    string? Protect(string? plainText);
    string? Unprotect(string? cipherText);
}

/// <summary>Criptografa (AES-256-GCM) as senhas dos programas de milhas guardadas em repouso.</summary>
public class SecretProtector : ISecretProtector
{
    private const string Prefix = "v1:";
    private readonly byte[] _key;

    public SecretProtector(IConfiguration config)
    {
        var raw = config["Crypto:Key"];
        if (string.IsNullOrWhiteSpace(raw) || raw.Length < 16)
            throw new InvalidOperationException("Crypto:Key nao configurada (minimo 16 caracteres).");
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
    }

    public string? Protect(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return null;

        var nonce = RandomNumberGenerator.GetBytes(12);
        var data = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[data.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, data, cipher, tag);

        var payload = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);
        return Prefix + Convert.ToBase64String(payload);
    }

    public string? Unprotect(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return null;
        if (!cipherText.StartsWith(Prefix, StringComparison.Ordinal)) return null;

        try
        {
            var payload = Convert.FromBase64String(cipherText[Prefix.Length..]);
            var nonce = payload[..12];
            var tag = payload[12..28];
            var cipher = payload[28..];
            var data = new byte[cipher.Length];

            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(nonce, cipher, tag, data);
            return Encoding.UTF8.GetString(data);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
