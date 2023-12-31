﻿namespace PrivateBinSharp.Crypto.util.io;

internal sealed class LimitedInputStream
	: BaseInputStream
{
	private readonly Stream m_stream;
	private long m_limit;

	internal LimitedInputStream(Stream stream, long limit)
	{
		m_stream = stream;
		m_limit = limit;
	}

	internal long CurrentLimit => m_limit;

	public override int Read(byte[] buffer, int offset, int count)
	{
		int numRead = m_stream.Read(buffer, offset, count);
		if (numRead > 0)
		{
			if ((m_limit -= numRead) < 0)
				throw new StreamOverflowException("Data Overflow");
		}
		return numRead;
	}

	public override int Read(Span<byte> buffer)
	{
		int numRead = m_stream.Read(buffer);
		if (numRead > 0)
		{
			if ((m_limit -= numRead) < 0)
				throw new StreamOverflowException("Data Overflow");
		}
		return numRead;
	}

	public override int ReadByte()
	{
		int b = m_stream.ReadByte();
		if (b >= 0)
		{
			if (--m_limit < 0)
				throw new StreamOverflowException("Data Overflow");
		}
		return b;
	}
}
