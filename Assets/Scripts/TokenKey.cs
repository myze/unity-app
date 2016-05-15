using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
	// With solution from CraigTP, Stackoverflow
	// http://stackoverflow.com/a/10177020

	public class TokenKey
	{
		// Store as cipertext for preventing memory injection
		private string _cipherToken;

		private readonly string _devicePassPhrase;

		// This constant is used to determine the keysize of the encryption algorithm in bits.
		// We divide this by 8 within the code below to get the equivalent number of bytes.
		private const int Keysize = 256;

		// This constant determines the number of iterations for the password bytes generation function.
		private const int DerivationIterations = 1000;

		public TokenKey (string token, bool isEncrypted = false)
		{
			// Device pass phrase will be physical address + device unique id provided by UnityEngine
			_devicePassPhrase = 
#if !UNITY_IOS
			// iOS 7 later will not be able to retrieve MAC address
				Essentials.GetMAC ().ToString ().ToLower () + "-" +
#endif
			SystemInfo.deviceUniqueIdentifier;

			if (!isEncrypted)
				_cipherToken = Encrypt (token, _devicePassPhrase);
			else
				_cipherToken = token;
		}

		public string CipherToken {
			get { return _cipherToken; }
		}

		public string Token {
			get { return Decrypt (_cipherToken, _devicePassPhrase); }
		}

		public override string ToString ()
		{
			return Token;
		}

		private string Encrypt (string plainText, string passPhrase)
		{
			// Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
			// so that the same Salt and IV values can be used when decrypting.  
			var saltStringBytes = Generate256BitsOfRandomEntropy ();
			var ivStringBytes = Generate256BitsOfRandomEntropy ();
			var plainTextBytes = Encoding.UTF8.GetBytes (plainText);
			var password = new Rfc2898DeriveBytes (passPhrase, saltStringBytes, DerivationIterations);
			var keyBytes = password.GetBytes (Keysize / 8);
			using (var symmetricKey = new RijndaelManaged ()) {
				symmetricKey.BlockSize = 256;
				symmetricKey.Mode = CipherMode.CBC;
				symmetricKey.Padding = PaddingMode.PKCS7;
				using (var encryptor = symmetricKey.CreateEncryptor (keyBytes, ivStringBytes)) {
					using (var memoryStream = new MemoryStream ()) {
						using (var cryptoStream = new CryptoStream (memoryStream, encryptor, CryptoStreamMode.Write)) {
							cryptoStream.Write (plainTextBytes, 0, plainTextBytes.Length);
							cryptoStream.FlushFinalBlock ();
							// Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
							var cipherTextBytes = saltStringBytes;
							cipherTextBytes = cipherTextBytes.Concat (ivStringBytes).ToArray ();
							cipherTextBytes = cipherTextBytes.Concat (memoryStream.ToArray ()).ToArray ();
							memoryStream.Close ();
							cryptoStream.Close ();
							return Convert.ToBase64String (cipherTextBytes);
						}
					}
				}
			}
		}

		private string Decrypt (string cipherText, string passPhrase)
		{
			// Get the complete stream of bytes that represent:
			// [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
			var cipherTextBytesWithSaltAndIv = Convert.FromBase64String (cipherText);
			// Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
			var saltStringBytes = cipherTextBytesWithSaltAndIv.Take (Keysize / 8).ToArray ();
			// Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
			var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip (Keysize / 8).Take (Keysize / 8).ToArray ();
			// Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
			var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip ((Keysize / 8) * 2).Take (cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray ();

			var password = new Rfc2898DeriveBytes (passPhrase, saltStringBytes, DerivationIterations);
			var keyBytes = password.GetBytes (Keysize / 8);
			using (var symmetricKey = new RijndaelManaged ()) {
				symmetricKey.BlockSize = 256;
				symmetricKey.Mode = CipherMode.CBC;
				symmetricKey.Padding = PaddingMode.PKCS7;
				using (var decryptor = symmetricKey.CreateDecryptor (keyBytes, ivStringBytes)) {
					using (var memoryStream = new MemoryStream (cipherTextBytes)) {
						using (var cryptoStream = new CryptoStream (memoryStream, decryptor, CryptoStreamMode.Read)) {
							var plainTextBytes = new byte[cipherTextBytes.Length];
							var decryptedByteCount = cryptoStream.Read (plainTextBytes, 0, plainTextBytes.Length);
							memoryStream.Close ();
							cryptoStream.Close ();
							return Encoding.UTF8.GetString (plainTextBytes, 0, decryptedByteCount);
						}
					}
				}
			}
		}

		private byte[] Generate256BitsOfRandomEntropy ()
		{
			var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
			var rngCsp = new RNGCryptoServiceProvider ();
			// Fill the array with cryptographically secure random bytes.
			rngCsp.GetBytes (randomBytes);
			return randomBytes;
		}
	}
}