﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace AppHarbor.Web.Security
{
	public class SymmetricEncryption : Encryption
	{
		private readonly SymmetricAlgorithm _algorithm;
		private readonly byte[] _secretKey;

		public SymmetricEncryption(SymmetricAlgorithm algorithm, byte[] secretKey)
		{
			_algorithm = algorithm;
			_secretKey = secretKey;
		}

		public override void Dispose()
		{
		    _algorithm.Clear();
			((IDisposable)_algorithm).Dispose();
		}

        public override byte[] Encrypt(byte[] valueBytes)
        {
            return Encrypt(valueBytes, null);
        }

		public override byte[] Encrypt(byte[] valueBytes, byte[] initializationVector)
		{
			bool generateRandomIV = initializationVector == null;
			if (generateRandomIV)
			{
				initializationVector = new byte[_algorithm.BlockSize / 8];
			    var rng = RandomNumberGenerator.Create();
			    try
			    {
                    rng.GetBytes(initializationVector);
			    }
			    finally 
			    {
                    if (rng != null)
                    {
                        ((IDisposable)rng).Dispose();
                    }
			    }
                
			}
			using (var output = new MemoryStream())
			{
				if (generateRandomIV)
				{
					output.Write(initializationVector, 0, initializationVector.Length);
				}
				using (var cryptoOutput = new CryptoStream(output, _algorithm.CreateEncryptor(_secretKey, initializationVector), CryptoStreamMode.Write))
				{
					cryptoOutput.Write(valueBytes, 0, valueBytes.Length);
				}

				return output.ToArray();
			}
		}

        public override byte[] Decrypt(byte[] encryptedValue)
        {
            return Decrypt(encryptedValue, null);
        }

		public override byte[] Decrypt(byte[] encryptedValue, byte[] initializationVector)
		{
			int dataOffset = 0;
			if (initializationVector == null)
			{
				initializationVector = new byte[_algorithm.BlockSize / 8];
				Buffer.BlockCopy(encryptedValue, 0, initializationVector, 0, initializationVector.Length);
				dataOffset = initializationVector.Length;
			}
			using (var output = new MemoryStream())
			{
				using (var cryptoOutput = new CryptoStream(output, _algorithm.CreateDecryptor(_secretKey, initializationVector), CryptoStreamMode.Write))
				{
					cryptoOutput.Write(encryptedValue, dataOffset, encryptedValue.Length - dataOffset);
				}

				return output.ToArray();
			}
		}
	}

	public sealed class SymmetricEncryption<T> : SymmetricEncryption where T : SymmetricAlgorithm, new()
	{
		public SymmetricEncryption(byte[] secretKey)
			: base(new T(), secretKey)
		{
		}
	}
}
