using System;
using System.IO;
using System.Text;
using Anvil.CSharp.Data;

namespace TinyJSON
{
	public class Decoder : IDecoder
	{
		private const string whiteSpace = " \t\n\r";
		private const string wordBreak = " \t\n\r{}[],:\"";

        private enum Token
		{
			None,
			OpenBrace,
			CloseBrace,
			OpenBracket,
			CloseBracket,
			Colon,
			Comma,
			String,
			Number,
			True,
			False,
			Null
		}

		private StringReader json;

		public Decoder()
		{
        }

        public void Dispose()
		{
			json.Dispose();
			json = null;
		}

        public Variant Decode(string jsonString)
        {
            json = new StringReader( jsonString );
            return DecodeValue();
        }


		private ProxyObject DecodeObject()
		{
			ProxyObject proxy = new ProxyObject();

			// Ditch opening brace.
			json.Read();

			// {
			while (true)
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (NextToken)
				{
					case Token.None:
						return null;

					case Token.Comma:
						continue;

					case Token.CloseBrace:
						return proxy;

					default:
						// Key
						string key = DecodeString();
						if (key == null)
						{
							return null;
						}

						// :
						if (NextToken != Token.Colon)
						{
							return null;
						}

						json.Read();

						// Value
						proxy.Add( key, DecodeValue() );
						break;
				}
			}
		}


		private ProxyArray DecodeArray()
		{
			ProxyArray proxy = new ProxyArray();

			// Ditch opening bracket.
			json.Read();

			// [
			bool parsing = true;
			while (parsing)
			{
				Token nextToken = NextToken;

				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (nextToken)
				{
					case Token.None:
						return null;

					case Token.Comma:
						continue;

					case Token.CloseBracket:
						parsing = false;
						break;

					default:
						proxy.Add( DecodeByToken( nextToken ) );
						break;
				}
			}

			return proxy;
		}


		private Variant DecodeValue()
		{
			Token nextToken = NextToken;
			return DecodeByToken( nextToken );
		}


        private Variant DecodeByToken( Token token )
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (token)
            {
                case Token.String:
                    return DecodeString();
                case Token.Number:
                    return DecodeNumber();
                case Token.OpenBrace:
                    return DecodeObject();
                case Token.OpenBracket:
                    return DecodeArray();
                case Token.True:
                    return new ProxyBoolean(true);
                case Token.False:
                    return new ProxyBoolean(false);
                case Token.Null:
                default:
                    return null;
            }
        }


		private Variant DecodeString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			// ditch opening quote
			json.Read();

			bool parsing = true;
			while (parsing)
			{
				if (json.Peek() == -1)
				{
					// ReSharper disable once RedundantAssignment
					parsing = false;
					break;
				}

				char c = NextChar;
				switch (c)
				{
					case '"':
						parsing = false;
						break;

					case '\\':
						if (json.Peek() == -1)
						{
							parsing = false;
							break;
						}

						c = NextChar;

						// ReSharper disable once SwitchStatementMissingSomeCases
						switch (c)
						{
							case '"':
							case '\\':
							case '/':
								stringBuilder.Append( c );
								break;

							case 'b':
								stringBuilder.Append( '\b' );
								break;

							case 'f':
								stringBuilder.Append( '\f' );
								break;

							case 'n':
								stringBuilder.Append( '\n' );
								break;

							case 'r':
								stringBuilder.Append( '\r' );
								break;

							case 't':
								stringBuilder.Append( '\t' );
								break;

							case 'u':
								StringBuilder hex = new StringBuilder();

								for (int i = 0; i < 4; i++)
								{
									hex.Append( NextChar );
								}

								stringBuilder.Append( (char) Convert.ToInt32( hex.ToString(), 16 ) );
								break;

							//default:
							//	throw new DecodeException( @"Illegal character following escape character: " + c );
						}

						break;

					default:
						stringBuilder.Append( c );
						break;
				}
			}

			return new ProxyString( stringBuilder.ToString() );
		}


        private Variant DecodeNumber()
		{
			return new ProxyNumber( NextWord );
		}


        private void ConsumeWhiteSpace()
		{
			while (whiteSpace.IndexOf( PeekChar ) != -1)
			{
				json.Read();

				if (json.Peek() == -1)
				{
					break;
				}
			}
		}


        private char PeekChar
		{
			get
			{
				int peek = json.Peek();
				return peek == -1 ? '\0' : Convert.ToChar( peek );
			}
		}


        private char NextChar => Convert.ToChar( json.Read() );


        private string NextWord
		{
			get
			{
				StringBuilder word = new StringBuilder();

				while (wordBreak.IndexOf( PeekChar ) == -1)
				{
					word.Append( NextChar );

					if (json.Peek() == -1)
					{
						break;
					}
				}

				return word.ToString();
			}
		}


        private Token NextToken
		{
			get
			{
				ConsumeWhiteSpace();

				if (json.Peek() == -1)
				{
					return Token.None;
				}

				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (PeekChar)
				{
					case '{':
						return Token.OpenBrace;

					case '}':
						json.Read();
						return Token.CloseBrace;

					case '[':
						return Token.OpenBracket;

					case ']':
						json.Read();
						return Token.CloseBracket;

					case ',':
						json.Read();
						return Token.Comma;

					case '"':
						return Token.String;

					case ':':
						return Token.Colon;

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '-':
						return Token.Number;
				}

				// ReSharper disable once SwitchStatementMissingSomeCases
                switch (NextWord)
                {
                    case "false":
                        return Token.False;
                    case "true":
                        return Token.True;
                    case "null":
                        return Token.Null;
                    default:
                        return Token.None;
                }
            }
		}
	}
}
