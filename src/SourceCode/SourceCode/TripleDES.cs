using System;
using System.Security.Cryptography;
using System.Text;

public static class TripleDES
{
	public static void NotEmpty(string str, string paramName)
	{
		NotEmpty(str, paramName, null);
	}

	public static void NotEmpty(string str, string paramName, string message)
	{
		if (str == null || str.Trim().Length <= 0)
		{
			throw new ArgumentException(message, paramName);
		}
	}

	public static string EncryptUnicode(string plaintext, string key)
	{
		NotEmpty(key, "key");
		if (string.IsNullOrEmpty(plaintext))
		{
			return null;
		}
		ICryptoTransform cryptoTransform = CreateProviderUnicode(key).CreateEncryptor();
		byte[] bytes = Encoding.Unicode.GetBytes(plaintext);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	public static string DecryptUnicode(string ciphertext, string key)
	{
		NotEmpty(key, "key");
		if (string.IsNullOrEmpty(ciphertext))
		{
			return null;
		}
		try
		{
			ICryptoTransform cryptoTransform = CreateProviderUnicode(key).CreateDecryptor();
			byte[] array = Convert.FromBase64String(ciphertext);
			byte[] array2 = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			return Encoding.Unicode.GetString(array2, 0, array2.Length);
		}
		catch
		{
			UtDebug.LogWarning("TripleDES DecryptUnicode exception caught. Possible key mismatch.", 0);
			return null;
		}
	}

	private static TripleDESCryptoServiceProvider CreateProviderUnicode(string key)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = mD5CryptoServiceProvider.ComputeHash(Encoding.Unicode.GetBytes(key));
		tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
		return tripleDESCryptoServiceProvider;
	}

	public static string EncryptASCII(string plaintext, string key)
	{
		NotEmpty(key, "key");
		if (string.IsNullOrEmpty(plaintext))
		{
			return null;
		}
		ICryptoTransform cryptoTransform = CreateProviderASCII(key).CreateEncryptor();
		byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	public static string DecryptASCII(string ciphertext, string key)
	{
		NotEmpty(key, "key");
		if (string.IsNullOrEmpty(ciphertext))
		{
			return null;
		}
		try
		{
			ICryptoTransform cryptoTransform = CreateProviderASCII(key).CreateDecryptor();
			byte[] array = Convert.FromBase64String(ciphertext);
			array = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			return Encoding.UTF8.GetString(array, 0, array.Length);
		}
		catch
		{
			UtDebug.LogWarning("TripleDES DecryptASCII exception caught. Possible key mismatch.", 0);
			return null;
		}
	}

	private static TripleDESCryptoServiceProvider CreateProviderASCII(string key)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(key));
		tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
		return tripleDESCryptoServiceProvider;
	}
}
