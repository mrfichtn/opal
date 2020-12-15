using Opal;
using System;
using System.Text;

namespace perftests
{
	public class OpalSwitchScanner: IDisposable
	{
		public const int Eof = -1;

		private readonly IBuffer buffer;
		private int ch;
		private int line;
		private int column;

		private int lastAcceptingState;
		private int lastAcceptingPosition;
		private int lastLine;
		private int lastColumn;

		public OpalSwitchScanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column)
		{ }

		public OpalSwitchScanner(IBuffer buffer, int line = 1, int column = 0)
		{
			this.buffer = buffer;
			this.line = line;
			this.column = column;
			NextChar();
		}

		public void Dispose()
		{
			buffer.Dispose();
			GC.SuppressFinalize(this);
		}

		public string FilePath { get; private set; }

		/// <summary>Skipping ignore, returns next token</summary>
		public Token NextToken()
		{
			Token token;
			do { token = RawNextToken(); }
			while (token.State < TokenStates.SyntaxError);
			return token;
		}

		public Token RawNextToken()
		{
			Token token;
			if (ch == Eof)
				return new Token(line, column, buffer.Position);

			var startPosition = new Position(line, column, buffer.Position - 1);
			MarkAccepting(TokenStates.SyntaxError);

			if (ch == '0') goto State2;
			if (ch == '[') goto State3;
			if (ch == '\"') goto State4;
			if (ch == 'u') goto State5;
			if (ch == '$') goto State7;
			if (ch == '\'') goto State8;
			if (ch == '/') goto State10;
			if (ch == '*') goto State11;
			if (ch == 'n') goto State12;
			if (ch == ';') goto State13;
			if (ch == 't') goto State14;
			if (ch == 'O') goto State15;
			if (ch == '=') goto State16;
			if (ch == 'C') goto State17;
			if (ch == '+') goto State18;
			if (ch == '-') goto State19;
			if (ch == '!') goto State20;
			if (ch == 'T') goto State21;
			if (ch == '<') goto State22;
			if (ch == '>') goto State23;
			if (ch == '|') goto State24;
			if (ch == '?') goto State25;
			if (ch == '{') goto State26;
			if (ch == '}') goto State27;
			if (ch == ',') goto State28;
			if (ch == '(') goto State29;
			if (ch == ')') goto State30;
			if (ch == 'P') goto State31;
			if (ch == ':') goto State32;
			if (ch == 'f') goto State33;
			if (ch == '.') goto State34;
			if (ch == '\t' || ch == '\n' || ch == '\r' || ch == ' ') goto State9;
			if ((ch >= '1' && ch <= '9')) goto State6;
			if ((ch >= '@' && ch <= 'B') || (ch >= 'D' && ch <= 'N') || (ch >= 'Q' && ch <= 'S') || (ch >= 'U' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'e') || (ch >= 'g' && ch <= 'm') || (ch >= 'o' && ch <= 's') || (ch >= 'v' && ch <= 'z')) goto State1;
			goto EndState2;
		State1:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State2:
			MarkAccepting(TokenStates.integer);
			NextChar();
			goto EndState;
		State3:
			NextChar();
			if (ch == '\\') goto State113;
			if (ch == ']') goto State114;
			if (!((ch == -1) || ch == '\t' || ch == '\n' || ch == '\r' || (ch >= '[' && ch <= ']'))) goto State112;
			goto EndState;
		State4:
			NextChar();
			if (ch == '\"') goto State110;
			if (ch == '\\') goto State111;
			if (!((ch == -1) || ch == '\n' || ch == '\r' || ch == '\"' || ch == '\\')) goto State4;
			goto EndState;
		State5:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State106;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State6:
			MarkAccepting(TokenStates.integer);
			NextChar();
			if ((ch >= '0' && ch <= '9')) goto State6;
			goto EndState;
		State7:
			NextChar();
			if (ch == '0') goto State104;
			if ((ch >= '1' && ch <= '9')) goto State105;
			goto EndState;
		State8:
			NextChar();
			if (ch == '\\') goto State95;
			if (!((ch == -1) || ch == '\n' || ch == '\r' || ch == '\"' || ch == '\\')) goto State94;
			goto EndState;
		State9:
			MarkAccepting(TokenStates.Space);
			NextChar();
			if (ch == '\t' || ch == '\n' || ch == '\r' || ch == ' ') goto State9;
			goto EndState;
		State10:
			NextChar();
			if (ch == '/') goto State90;
			if (ch == '*') goto State91;
			goto EndState;
		State11:
			MarkAccepting(TokenStates.Asterisk);
			NextChar();
			goto EndState;
		State12:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State80;
			if (ch == 'a') goto State81;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'b' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State13:
			MarkAccepting(TokenStates.Semicolon);
			NextChar();
			goto EndState;
		State14:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'r') goto State77;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'q') || (ch >= 's' && ch <= 'z')) goto State1;
			goto EndState;
		State15:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'p') goto State71;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'o') || (ch >= 'q' && ch <= 'z')) goto State1;
			goto EndState;
		State16:
			MarkAccepting(TokenStates.Equal);
			NextChar();
			goto EndState;
		State17:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'o') goto State54;
			if (ch == 'h') goto State55;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'g') || (ch >= 'i' && ch <= 'n') || (ch >= 'p' && ch <= 'z')) goto State1;
			goto EndState;
		State18:
			MarkAccepting(TokenStates.Plus);
			NextChar();
			goto EndState;
		State19:
			MarkAccepting(TokenStates.Minus);
			NextChar();
			goto EndState;
		State20:
			MarkAccepting(TokenStates.Exclamation);
			NextChar();
			goto EndState;
		State21:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'o') goto State49;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'n') || (ch >= 'p' && ch <= 'z')) goto State1;
			goto EndState;
		State22:
			MarkAccepting(TokenStates.LessThan);
			NextChar();
			goto EndState;
		State23:
			MarkAccepting(TokenStates.GreaterThan);
			NextChar();
			goto EndState;
		State24:
			MarkAccepting(TokenStates.VerticalBar);
			NextChar();
			goto EndState;
		State25:
			MarkAccepting(TokenStates.QuestionMark);
			NextChar();
			goto EndState;
		State26:
			MarkAccepting(TokenStates.LeftCurly);
			NextChar();
			goto EndState;
		State27:
			MarkAccepting(TokenStates.RightCurly);
			NextChar();
			goto EndState;
		State28:
			MarkAccepting(TokenStates.Comma);
			NextChar();
			goto EndState;
		State29:
			MarkAccepting(TokenStates.LeftParen);
			NextChar();
			goto EndState;
		State30:
			MarkAccepting(TokenStates.RightParen);
			NextChar();
			goto EndState;
		State31:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'r') goto State39;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'q') || (ch >= 's' && ch <= 'z')) goto State1;
			goto EndState;
		State32:
			MarkAccepting(TokenStates.Colon);
			NextChar();
			goto EndState;
		State33:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'a') goto State35;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'b' && ch <= 'z')) goto State1;
			goto EndState;
		State34:
			MarkAccepting(TokenStates.Period);
			NextChar();
			goto EndState;
		State35:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'l') goto State36;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'k') || (ch >= 'm' && ch <= 'z')) goto State1;
			goto EndState;
		State36:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State37;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State37:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State38;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State38:
			MarkAccepting(TokenStates.@false);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State39:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'o') goto State40;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'n') || (ch >= 'p' && ch <= 'z')) goto State1;
			goto EndState;
		State40:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'd') goto State41;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'c') || (ch >= 'e' && ch <= 'z')) goto State1;
			goto EndState;
		State41:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'u') goto State42;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 't') || (ch >= 'v' && ch <= 'z')) goto State1;
			goto EndState;
		State42:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'c') goto State43;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || ch == 'a' || ch == 'b' || (ch >= 'd' && ch <= 'z')) goto State1;
			goto EndState;
		State43:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 't') goto State44;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 's') || (ch >= 'u' && ch <= 'z')) goto State1;
			goto EndState;
		State44:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'i') goto State45;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'h') || (ch >= 'j' && ch <= 'z')) goto State1;
			goto EndState;
		State45:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'o') goto State46;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'n') || (ch >= 'p' && ch <= 'z')) goto State1;
			goto EndState;
		State46:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'n') goto State47;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'm') || (ch >= 'o' && ch <= 'z')) goto State1;
			goto EndState;
		State47:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State48;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State48:
			MarkAccepting(TokenStates.@Productions);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State49:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'k') goto State50;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'j') || (ch >= 'l' && ch <= 'z')) goto State1;
			goto EndState;
		State50:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State51;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State51:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'n') goto State52;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'm') || (ch >= 'o' && ch <= 'z')) goto State1;
			goto EndState;
		State52:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State53;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State53:
			MarkAccepting(TokenStates.@Tokens);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State54:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'n') goto State64;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'm') || (ch >= 'o' && ch <= 'z')) goto State1;
			goto EndState;
		State55:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'a') goto State56;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'b' && ch <= 'z')) goto State1;
			goto EndState;
		State56:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'r') goto State57;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'q') || (ch >= 's' && ch <= 'z')) goto State1;
			goto EndState;
		State57:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'a') goto State58;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'b' && ch <= 'z')) goto State1;
			goto EndState;
		State58:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'c') goto State59;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || ch == 'a' || ch == 'b' || (ch >= 'd' && ch <= 'z')) goto State1;
			goto EndState;
		State59:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 't') goto State60;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 's') || (ch >= 'u' && ch <= 'z')) goto State1;
			goto EndState;
		State60:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State61;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State61:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'r') goto State62;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'q') || (ch >= 's' && ch <= 'z')) goto State1;
			goto EndState;
		State62:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State63;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State63:
			MarkAccepting(TokenStates.@Characters);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State64:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'f') goto State65;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'e') || (ch >= 'g' && ch <= 'z')) goto State1;
			goto EndState;
		State65:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'l') goto State66;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'k') || (ch >= 'm' && ch <= 'z')) goto State1;
			goto EndState;
		State66:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'i') goto State67;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'h') || (ch >= 'j' && ch <= 'z')) goto State1;
			goto EndState;
		State67:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'c') goto State68;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || ch == 'a' || ch == 'b' || (ch >= 'd' && ch <= 'z')) goto State1;
			goto EndState;
		State68:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 't') goto State69;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 's') || (ch >= 'u' && ch <= 'z')) goto State1;
			goto EndState;
		State69:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State70;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State70:
			MarkAccepting(TokenStates.@Conflicts);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State71:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 't') goto State72;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 's') || (ch >= 'u' && ch <= 'z')) goto State1;
			goto EndState;
		State72:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'i') goto State73;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'h') || (ch >= 'j' && ch <= 'z')) goto State1;
			goto EndState;
		State73:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'o') goto State74;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'n') || (ch >= 'p' && ch <= 'z')) goto State1;
			goto EndState;
		State74:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'n') goto State75;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'm') || (ch >= 'o' && ch <= 'z')) goto State1;
			goto EndState;
		State75:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State76;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State76:
			MarkAccepting(TokenStates.@Options);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State77:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'u') goto State78;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 't') || (ch >= 'v' && ch <= 'z')) goto State1;
			goto EndState;
		State78:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State79;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State79:
			MarkAccepting(TokenStates.@true);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State80:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'w') goto State89;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'v') || (ch >= 'x' && ch <= 'z')) goto State1;
			goto EndState;
		State81:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'm') goto State82;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'l') || (ch >= 'n' && ch <= 'z')) goto State1;
			goto EndState;
		State82:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State83;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State83:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 's') goto State84;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'r') || (ch >= 't' && ch <= 'z')) goto State1;
			goto EndState;
		State84:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'p') goto State85;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'o') || (ch >= 'q' && ch <= 'z')) goto State1;
			goto EndState;
		State85:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'a') goto State86;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'b' && ch <= 'z')) goto State1;
			goto EndState;
		State86:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'c') goto State87;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || ch == 'a' || ch == 'b' || (ch >= 'd' && ch <= 'z')) goto State1;
			goto EndState;
		State87:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'e') goto State88;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'd') || (ch >= 'f' && ch <= 'z')) goto State1;
			goto EndState;
		State88:
			MarkAccepting(TokenStates.@namespace);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State89:
			MarkAccepting(TokenStates.@new);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State90:
			MarkAccepting(TokenStates.Comment);
			NextChar();
			if (!((ch == -1) || ch == '\n')) goto State90;
			goto EndState;
		State91:
			NextChar();
			if (ch == '*') goto State92;
			if (!((ch == -1) || ch == '*')) goto State91;
			goto EndState;
		State92:
			NextChar();
			if (ch == '/') goto State93;
			if (!((ch == -1) || ch == '/')) goto State91;
			goto EndState;
		State93:
			MarkAccepting(TokenStates.multi_line_comment);
			NextChar();
			goto EndState;
		State94:
			NextChar();
			if (ch == '\'') goto State100;
			goto EndState;
		State95:
			NextChar();
			if (ch == 'u') goto State96;
			if (ch == 'x') goto State97;
			if (ch == '\"' || ch == '\'' || ch == '0' || ch == '\\' || ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v') goto State94;
			goto EndState;
		State96:
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f')) goto State102;
			goto EndState;
		State97:
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f')) goto State98;
			goto EndState;
		State98:
			NextChar();
			if (ch == '\'') goto State100;
			if ((ch >= '0' && ch <= '9') || ch == 'A' || ch == 'B' || (ch >= 'D' && ch <= 'F') || ch == 'b') goto State99;
			goto EndState;
		State99:
			NextChar();
			if (ch == '\'') goto State100;
			if ((ch >= '0' && ch <= '9') || ch == 'A' || ch == 'B' || (ch >= 'D' && ch <= 'F') || ch == 'b') goto State101;
			goto EndState;
		State100:
			MarkAccepting(TokenStates.char_string);
			NextChar();
			goto EndState;
		State101:
			NextChar();
			if (ch == '\'') goto State100;
			if ((ch >= '0' && ch <= '9') || ch == 'A' || ch == 'B' || (ch >= 'D' && ch <= 'F') || ch == 'b') goto State94;
			goto EndState;
		State102:
			NextChar();
			if ((ch >= '0' && ch <= '9') || ch == 'A' || ch == 'B' || (ch >= 'D' && ch <= 'F') || ch == 'b') goto State103;
			goto EndState;
		State103:
			NextChar();
			if ((ch >= '0' && ch <= '9') || ch == 'A' || ch == 'B' || (ch >= 'D' && ch <= 'F') || ch == 'b') goto State101;
			goto EndState;
		State104:
			MarkAccepting(TokenStates.arg);
			NextChar();
			goto EndState;
		State105:
			MarkAccepting(TokenStates.arg);
			NextChar();
			if ((ch >= '0' && ch <= '9')) goto State105;
			goto EndState;
		State106:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'i') goto State107;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'h') || (ch >= 'j' && ch <= 'z')) goto State1;
			goto EndState;
		State107:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'n') goto State108;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'm') || (ch >= 'o' && ch <= 'z')) goto State1;
			goto EndState;
		State108:
			MarkAccepting(TokenStates.identifier);
			NextChar();
			if (ch == 'g') goto State109;
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'f') || (ch >= 'h' && ch <= 'z')) goto State1;
			goto EndState;
		State109:
			MarkAccepting(TokenStates.@using);
			NextChar();
			if ((ch >= '0' && ch <= '9') || (ch >= '@' && ch <= 'Z') || ch == '_' || (ch >= 'a' && ch <= 'z')) goto State1;
			goto EndState;
		State110:
			MarkAccepting(TokenStates.String);
			NextChar();
			goto EndState;
		State111:
			NextChar();
			if (ch == '\"' || ch == '\'' || ch == '0' || ch == '\\' || ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v') goto State4;
			goto EndState;
		State112:
			NextChar();
			if (ch == '\\') goto State113;
			if (ch == ']') goto State114;
			if (!((ch == -1) || ch == '\t' || ch == '\n' || ch == '\r' || (ch >= '[' && ch <= '^'))) goto State112;
			goto EndState;
		State113:
			NextChar();
			if (ch == '\"' || ch == '\'' || ch == '0' || (ch >= '[' && ch <= '^') || ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v') goto State112;
			goto EndState;
		State114:
			MarkAccepting(TokenStates.char_class);
			NextChar();
			goto EndState;
		EndState:
			if (lastAcceptingState == TokenStates.SyntaxError)
			{
				lastAcceptingPosition = buffer.Position - 1;
				lastAcceptingState = -1;
			}
			token = new Token(startPosition,
				new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
				lastAcceptingState,
				buffer.GetString(startPosition.Ch, lastAcceptingPosition));
			if (buffer.Position != lastAcceptingPosition)
			{
				buffer.Position = lastAcceptingPosition;
				line = lastLine;
				column = lastColumn;
				NextChar();
			}
			return token;

		EndState2:
			token = new Token(startPosition,
				new Position(lastLine, lastColumn, lastAcceptingPosition - 1),
				lastAcceptingState,
				buffer.GetString(startPosition.Ch, lastAcceptingPosition));
			NextChar();
			return token;
		}

		void MarkAccepting(int type)
		{
			lastAcceptingState = type;
			lastAcceptingPosition = buffer.Position;
			lastLine = line;
			lastColumn = column;
		}

		/// <summary>
		/// Retrieves the next character, adjusting position information
		/// </summary>
		private void NextChar()
		{
			ch = buffer.Read();
			if (ch == '\n')
			{
				++line;
				column = 0;

				prevLine = curLine.ToString();
				curLine.Clear();
			}
			else if (ch == '\r')
			{
				++column;
			}
			else
			{
				++column;
				curLine.Append((char)ch);
			}
		}

		private string prevLine;
		private StringBuilder curLine = new StringBuilder();

		public string Line(int position)
		{
			if (position + 1 == line)
				return prevLine;
			if (position == line)
				return curLine.ToString() + buffer.PeekLine();
			return string.Empty;
		}

		public class TokenStates
		{
			public const int SyntaxError = -1;
			public const int Empty = 0;
			public const int identifier = 1;
			public const int char_class = 2;
			public const int String = 3;
			public const int char_string = 4;
			public const int integer = 5;
			public const int arg = 6;
			public const int @using = 7;
			public const int Semicolon = 8;
			public const int @namespace = 9;
			public const int @Options = 10;
			public const int Equal = 11;
			public const int @Characters = 12;
			public const int Plus = 13;
			public const int Minus = 14;
			public const int Exclamation = 15;
			public const int @Tokens = 16;
			public const int LessThan = 17;
			public const int GreaterThan = 18;
			public const int VerticalBar = 19;
			public const int Asterisk = 20;
			public const int QuestionMark = 21;
			public const int LeftCurly = 22;
			public const int RightCurly = 23;
			public const int Comma = 24;
			public const int LeftParen = 25;
			public const int RightParen = 26;
			public const int @Productions = 27;
			public const int @new = 28;
			public const int Colon = 29;
			public const int @Conflicts = 30;
			public const int Period = 31;
			public const int @true = 32;
			public const int @false = 33;
			public const int Space = -2;
			public const int Comment = -3;
			public const int multi_line_comment = -4;
		}
	}
}
