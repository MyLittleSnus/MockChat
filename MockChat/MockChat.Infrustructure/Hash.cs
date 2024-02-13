using System.Security.Cryptography;
using System.Text;

namespace MockChat.Infrustructure;

public static class HashProvider
{
	public static string Sha512(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA512.HashData(inputBytes);

        StringBuilder stringBuilder = new();

        foreach (byte t in hashBytes)
            stringBuilder.Append(t.ToString("X2"));

        return stringBuilder.ToString();
    }
}