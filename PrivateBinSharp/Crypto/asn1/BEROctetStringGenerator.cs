using PrivateBinSharp.Crypto.util.io;

namespace PrivateBinSharp.Crypto.asn1
{
    public class BerOctetStringGenerator
        : BerGenerator
    {
        public BerOctetStringGenerator(Stream outStream)
            : base(outStream)
        {
            WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.OctetString);
        }

        public BerOctetStringGenerator(Stream outStream, int tagNo, bool isExplicit)
            : base(outStream, tagNo, isExplicit)
        {
            WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.OctetString);
        }

        /// <remarks>The caller is responsible for disposing the returned <see cref="Stream"/> before disposing
        /// this generator.</remarks>
		public Stream GetOctetOutputStream()
        {
            return GetOctetOutputStream(new byte[1000]); // limit for CER encoding.
        }

        /// <remarks>The caller is responsible for disposing the returned <see cref="Stream"/> before disposing
        /// this generator.</remarks>
		public Stream GetOctetOutputStream(int bufSize)
        {
            return bufSize < 1
                ? GetOctetOutputStream()
                : GetOctetOutputStream(new byte[bufSize]);
        }

        /// <remarks>The caller is responsible for disposing the returned <see cref="Stream"/> before disposing
        /// this generator.</remarks>
		public Stream GetOctetOutputStream(byte[] buf)
        {
            return new BufferedBerOctetStream(GetRawOutputStream(), buf);
        }

        private class BufferedBerOctetStream
            : BaseOutputStream
        {
            private byte[] _buf;
            private int _off;
            private readonly Asn1OutputStream _derOut;

            internal BufferedBerOctetStream(Stream outStream, byte[] buf)
            {
                _buf = buf;
                _off = 0;
                _derOut = Asn1OutputStream.Create(outStream, Asn1Encodable.Der, leaveOpen: true);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                Streams.ValidateBufferArguments(buffer, offset, count);

                Write(buffer.AsSpan(offset, count));
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                int bufLen = _buf.Length;
                int available = bufLen - _off;
                if (buffer.Length < available)
                {
                    buffer.CopyTo(_buf.AsSpan(_off));
                    _off += buffer.Length;
                    return;
                }

                if (_off > 0)
                {
                    DerOctetString.Encode(_derOut, _buf.AsSpan(0, _off), buffer[..available]);
                    buffer = buffer[available..];
                    //_off = 0;
                }

                while (buffer.Length >= bufLen)
                {
                    DerOctetString.Encode(_derOut, buffer[..bufLen]);
                    buffer = buffer[bufLen..];
                }

                buffer.CopyTo(_buf.AsSpan());
                _off = buffer.Length;
            }

            public override void WriteByte(byte value)
            {
                _buf[_off++] = value;

                if (_off == _buf.Length)
                {
                    DerOctetString.Encode(_derOut, _buf, 0, _off);
                    _off = 0;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_off != 0)
                    {
                        DerOctetString.Encode(_derOut, _buf, 0, _off);
                        _off = 0;
                    }

                    _derOut.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
