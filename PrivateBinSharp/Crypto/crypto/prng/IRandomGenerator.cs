namespace PrivateBinSharp.Crypto.crypto.prng;

/// <remarks>Generic interface for objects generating random bytes.</remarks>
internal interface IRandomGenerator
{
	/// <summary>Add more seed material to the generator.</summary>
	/// <param name="seed">A byte array to be mixed into the generator's state.</param>
	void AddSeedMaterial(byte[] seed);

	void AddSeedMaterial(ReadOnlySpan<byte> seed);

	/// <summary>Add more seed material to the generator.</summary>
	/// <param name="seed">A long value to be mixed into the generator's state.</param>
	void AddSeedMaterial(long seed);

	/// <summary>Fill byte array with random values.</summary>
	/// <param name="bytes">Array to be filled.</param>
	void NextBytes(byte[] bytes);

	/// <summary>Fill byte array with random values.</summary>
	/// <param name="bytes">Array to receive bytes.</param>
	/// <param name="start">Index to start filling at.</param>
	/// <param name="len">Length of segment to fill.</param>
	void NextBytes(byte[] bytes, int start, int len);

	void NextBytes(Span<byte> bytes);
}
