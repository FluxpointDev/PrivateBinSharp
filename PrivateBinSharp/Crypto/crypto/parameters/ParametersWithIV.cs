using System.Buffers;

namespace PrivateBinSharp.Crypto.crypto.parameters;

internal class ParametersWithIV
	: ICipherParameters
{
	public static ParametersWithIV Create<TState>(ICipherParameters parameter, int ivLength, TState state,
		SpanAction<byte, TState> action)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));
		if (ivLength < 0)
			throw new ArgumentOutOfRangeException(nameof(ivLength));

		ParametersWithIV result = new ParametersWithIV(parameter, ivLength);
		action(result.m_iv, state);
		return result;
	}

	internal static ICipherParameters ApplyOptionalIV(ICipherParameters parameters, byte[] iv)
	{
		return iv == null ? parameters : new ParametersWithIV(parameters, iv);
	}

	private readonly ICipherParameters m_parameters;
	private readonly byte[] m_iv;

	public ParametersWithIV(ICipherParameters parameters, byte[] iv)
		: this(parameters, iv, 0, iv.Length)
	{
		// NOTE: 'parameters' may be null to imply key re-use
		if (iv == null)
			throw new ArgumentNullException(nameof(iv));

		m_parameters = parameters;
		m_iv = (byte[])iv.Clone();
	}

	public ParametersWithIV(ICipherParameters parameters, byte[] iv, int ivOff, int ivLen)
	{
		// NOTE: 'parameters' may be null to imply key re-use
		if (iv == null)
			throw new ArgumentNullException(nameof(iv));

		m_parameters = parameters;
		m_iv = new byte[ivLen];
		Array.Copy(iv, ivOff, m_iv, 0, ivLen);
	}

	public ParametersWithIV(ICipherParameters parameters, ReadOnlySpan<byte> iv)
	{
		// NOTE: 'parameters' may be null to imply key re-use
		m_parameters = parameters;
		m_iv = iv.ToArray();
	}

	private ParametersWithIV(ICipherParameters parameters, int ivLength)
	{
		if (ivLength < 0)
			throw new ArgumentOutOfRangeException(nameof(ivLength));

		// NOTE: 'parameters' may be null to imply key re-use
		m_parameters = parameters;
		m_iv = new byte[ivLength];
	}

	public byte[] GetIV()
	{
		return (byte[])m_iv.Clone();
	}

	public int IVLength => m_iv.Length;

	public ICipherParameters Parameters => m_parameters;

	internal ReadOnlySpan<byte> IV => m_iv;
}
