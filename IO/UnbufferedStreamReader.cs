using System.Diagnostics;
using System.IO;
using System.Text;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.IO
{
    /// <summary>
    /// A stream reader that does not use a buffer.
    /// <see cref="StreamReader"/> is more performant but advances its <see cref="BaseStream"/> past its own position an indeterminate amount.
    /// This stream is useful when reading files with mixed encoding or where the <see cref="BaseStream"/> needs to be advanced the minimum amount required.
    /// </summary>
    /// <remarks>
    /// This reader is not currently at feature parity with <see cref="StreamReader"/>. Features will be developed as needed.
    /// See: #87
    /// </remarks>
    public class UnbufferedStreamReader : AbstractAnvilBase
    {
        /// <summary>
        /// The stream being read.
        /// </summary>
        public Stream BaseStream { get; }
        /// <summary>
        /// The character encoding used to interpret the bytes of the stream.
        /// </summary>
        public Encoding CurrentEncoding { get; }

        private readonly bool m_LeaveBaseStreamOpen;
        private readonly Decoder m_Decoder;
        private readonly StringBuilder m_StringBuilder;

        // Used for APIs that require a buffer. All single length;
        private readonly byte[] m_SingleByteBuffer = new byte[1];
        private readonly char[] m_charBuffer = new char[1];

        private bool m_IsPreambleHandled;


        /// <summary>
        /// Creates an instance of <see cref="UnbufferedStreamReader"/>.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The encoding to use to decode the bytes to characters.</param>
        /// <param name="leaveBaseStreamOpen">
        /// If true, the <see cref="BaseStream"/> will not be closed when the reader is disposed.
        /// </param>
        public UnbufferedStreamReader(Stream stream, Encoding encoding, bool leaveBaseStreamOpen)
        {
            BaseStream = stream;
            CurrentEncoding = encoding;
            m_LeaveBaseStreamOpen = leaveBaseStreamOpen;

            m_Decoder = encoding.GetDecoder();
            m_StringBuilder = new StringBuilder();
            m_IsPreambleHandled = false;
        }

        protected override void DisposeSelf()
        {
            try
            {
                if (!m_LeaveBaseStreamOpen)
                {
                    BaseStream.Close();
                }
            }
            finally
            {
                m_StringBuilder.Clear();
                
                base.DisposeSelf();
            }
        }

        /// <summary>
        /// Closes the reader. Call <see cref="Dispose"/> instead.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Read the next character from the stream.
        /// </summary>
        /// <returns>The character read or default <see cref="char"/> if at the end of the stream.</returns>
        public char Read()
        {
            return ReadNextCharacter();
        }

        /// <summary>
        /// Reads the next line from the stream.
        /// </summary>
        /// <returns>The line read or <see cref="string.Empty"/> if already at the end of the stream.</returns>
        public string ReadLine()
        {
            if (!m_IsPreambleHandled)
            {
                CheckPreambleMatchesEncoding();
            }

            while (true)
            {
                char character = ReadNextCharacter();

                if (character == default || character == '\r' || character == '\n')
                {
                    return RenderAndClearStringBuilder();
                }

                m_StringBuilder.Append(character);
            }
        }

        /// <summary>
        /// Reads from the current stream position to the end of the stream.
        /// </summary>
        /// <returns>The content read or <see cref="string.Empty"/> if at the end of the stream.</returns>
        public string ReadToEnd()
        {
            if (!m_IsPreambleHandled)
            {
                CheckPreambleMatchesEncoding();
            }

            while (true)
            {
                char character = ReadNextCharacter();

                if (character == default)
                {
                    return RenderAndClearStringBuilder();
                }

                m_StringBuilder.Append(character);
            }
        }

        private void CheckPreambleMatchesEncoding()
        {
            if (m_IsPreambleHandled)
            {
                return;
            }

            byte[] preamble = CurrentEncoding.GetPreamble();
            for (int i = 0; i < preamble.Length; i++)
            {
                byte readByte = (byte)BaseStream.ReadByte();
                if ( readByte != preamble[i] )
                {
                    throw new InvalidDataException($"Preamble does not match encoding. Invalid byte index: {i}, value: {readByte}, expected: {preamble[i]}");
                }
            }

            m_IsPreambleHandled = true;
        }

        // Returns default char if at end of the stream
        private char ReadNextCharacter()
        {
            Debug.Assert(m_IsPreambleHandled);

            int decodedCharCount;
            do
            {
                m_SingleByteBuffer[0] = (byte)BaseStream.ReadByte();
                if (m_SingleByteBuffer[0] < 0)
                {
                    return default;
                }

                decodedCharCount = m_Decoder.GetChars(m_SingleByteBuffer, 0, 1, m_charBuffer, 0);
            } while (decodedCharCount < 1);

            return m_charBuffer[0];
        }

        private string RenderAndClearStringBuilder()
        {
            string result = m_StringBuilder.ToString();
            m_StringBuilder.Clear();
            return result;
        }
    }
}