using PrivateBinSharp.Crypto.util;

namespace PrivateBinSharp.Crypto.crypto.prng;

/**
 * Random generation based on the digest with counter. Calling AddSeedMaterial will
 * always increase the entropy of the hash.
 * <p>
 * Internal access to the digest is synchronized so a single one of these can be shared.
 * </p>
 */
internal sealed class DigestRandomGenerator
	: IRandomGenerator
{
	private const long CYCLE_COUNT = 10;

	private long stateCounter;
	private long seedCounter;
	private IDigest digest;
	private byte[] state;
	private byte[] seed;

	public DigestRandomGenerator(IDigest digest)
	{
		this.digest = digest;

		seed = new byte[digest.GetDigestSize()];
		seedCounter = 1;

		state = new byte[digest.GetDigestSize()];
		stateCounter = 1;
	}

	public void AddSeedMaterial(byte[] inSeed)
	{
		lock (this)
		{
			if (!Arrays.IsNullOrEmpty(inSeed))
			{
				DigestUpdate(inSeed);
			}
			DigestUpdate(seed);
			DigestDoFinal(seed);
		}
	}

	public void AddSeedMaterial(ReadOnlySpan<byte> inSeed)
	{
		lock (this)
		{
			if (!inSeed.IsEmpty)
			{
				DigestUpdate(inSeed);
			}
			DigestUpdate(seed);
			DigestDoFinal(seed);
		}
	}

	public void AddSeedMaterial(long rSeed)
	{
		lock (this)
		{
			DigestAddCounter(rSeed);
			DigestUpdate(seed);
			DigestDoFinal(seed);
		}
	}

	public void NextBytes(byte[] bytes)
	{
		NextBytes(bytes, 0, bytes.Length);
	}

	public void NextBytes(byte[] bytes, int start, int len)
	{
		NextBytes(bytes.AsSpan(start, len));
	}

	public void NextBytes(Span<byte> bytes)
	{
		lock (this)
		{
			int stateOff = 0;

			GenerateState();

			for (int i = 0; i < bytes.Length; ++i)
			{
				if (stateOff == state.Length)
				{
					GenerateState();
					stateOff = 0;
				}
				bytes[i] = state[stateOff++];
			}
		}
	}

	private void CycleSeed()
	{
		DigestUpdate(seed);
		DigestAddCounter(seedCounter++);
		DigestDoFinal(seed);
	}

	private void GenerateState()
	{
		DigestAddCounter(stateCounter++);
		DigestUpdate(state);
		DigestUpdate(seed);
		DigestDoFinal(state);

		if (stateCounter % CYCLE_COUNT == 0)
		{
			CycleSeed();
		}
	}

	private void DigestAddCounter(long seedVal)
	{
		Span<byte> bytes = stackalloc byte[8];
		Pack.UInt64_To_LE((ulong)seedVal, bytes);
		digest.BlockUpdate(bytes);
	}

	private void DigestUpdate(ReadOnlySpan<byte> inSeed)
	{
		digest.BlockUpdate(inSeed);
	}

	private void DigestDoFinal(Span<byte> result)
	{
		digest.DoFinal(result);
	}
}
