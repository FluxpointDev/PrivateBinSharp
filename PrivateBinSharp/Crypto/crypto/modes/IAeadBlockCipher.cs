namespace PrivateBinSharp.Crypto.crypto.modes;

/// <summary>An IAeadCipher based on an IBlockCipher.</summary>
internal interface IAeadBlockCipher
	: IAeadCipher
{
	/// <returns>The block size for this cipher, in bytes.</returns>
	int GetBlockSize();

	/// <summary>The block cipher underlying this algorithm.</summary>
	IBlockCipher UnderlyingCipher { get; }
}
