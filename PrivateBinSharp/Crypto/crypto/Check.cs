﻿using System.Diagnostics.CodeAnalysis;

namespace PrivateBinSharp.Crypto.crypto;

internal static class Check
{
	internal static void DataLength(byte[] buf, int off, int len, string message)
	{
		if (off > buf.Length - len)
			ThrowDataLengthException(message);
	}

	internal static void OutputLength(byte[] buf, int off, int len, string message)
	{
		if (off > buf.Length - len)
			ThrowOutputLengthException(message);
	}

	internal static void DataLength<T>(ReadOnlySpan<T> input, int len, string message)
	{
		if (input.Length < len)
			ThrowDataLengthException(message);
	}

	internal static void OutputLength<T>(Span<T> output, int len, string message)
	{
		if (output.Length < len)
			ThrowOutputLengthException(message);
	}

	[DoesNotReturn]
	internal static void ThrowDataLengthException(string message) => throw new DataLengthException(message);

	[DoesNotReturn]
	internal static void ThrowOutputLengthException(string message) => throw new OutputLengthException(message);
}
