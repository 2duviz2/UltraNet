using System.Text;
using UnityEngine;

namespace Jaket.Sam;

public class Sam
{
	private int[] input = new int[256];

	private int[] phonemeStress = new int[256];

	private int[] phonemeLength = new int[256];

	private int[] phonemeIndex = new int[256];

	public int Speed;

	public int Pitch;

	public int Mouth;

	public int Throat;

	public Buffer Buffer;

	public Legacy Legacy;

	public Sam(int speed = 64, int pitch = 64, int mouth = 128, int throat = 128)
	{
		Speed = speed;
		Pitch = pitch;
		Mouth = mouth;
		Throat = throat;
		Legacy = new Legacy(this);
	}

	public string Cyrillic2Latin(string text)
	{
		StringBuilder builder = new StringBuilder();
		char prev = ' ';
		foreach (char current in text)
		{
			if (current == 'Е' && (prev == ' ' || prev == 'Ъ' || prev == 'Ь'))
			{
				builder.Append("YE");
			}
			else if (Constants.ISO9.ContainsKey(current))
			{
				builder.Append(Constants.ISO9[current]);
			}
			else
			{
				builder.Append(current);
			}
			prev = current;
		}
		return builder.ToString();
	}

	public void Text2Phonemes(string text, out int[] output)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(Cyrillic2Latin(text.ToUpper()));
		output = new int[256];
		for (int i = 0; i < bytes.Length; i++)
		{
			output[i] = bytes[i];
		}
		Legacy.Text2Phonemes(ref output);
	}

	public void SetInput(int[] input)
	{
		int length = Mathf.Min(input.Length, 254);
		for (int i = 0; i < length; i++)
		{
			this.input[i] = input[i];
		}
		this.input[length] = 255;
		this.input[255] = 255;
	}

	public void Insert(int pos, int phoneme, int length = 0, int stress = -1)
	{
		for (int i = 253; i >= pos; i--)
		{
			phonemeIndex[i + 1] = phonemeIndex[i];
			phonemeLength[i + 1] = phonemeLength[i];
			phonemeStress[i + 1] = phonemeStress[i];
		}
		phonemeIndex[pos] = phoneme;
		phonemeLength[pos] = length;
		phonemeStress[pos] = ((stress == -1) ? phonemeStress[pos - 1] : stress);
	}

	public Buffer GetBuffer()
	{
		for (int i = 0; i < 256; i++)
		{
			phonemeStress[i] = (phonemeLength[i] = 0);
		}
		if (!ParsePhonemes())
		{
			return null;
		}
		RewritePhonemes();
		SetPhonemeLength();
		InsertPauses();
		PrepareOutput();
		return Buffer;
	}

	public bool ParsePhonemes()
	{
		int outputPos = 0;
		for (int pos = 0; pos < input.Length; pos++)
		{
			char sign1 = (char)input[pos];
			char sign2 = (char)((pos == input.Length - 1) ? 32u : ((uint)input[pos + 1]));
			if (sign1 == '\u009b')
			{
				phonemeIndex[outputPos++] = 255;
				return true;
			}
			int match = Constants.FullMatch(sign1, sign2);
			if (match != -1)
			{
				pos++;
				phonemeIndex[outputPos++] = match;
				continue;
			}
			match = Constants.WildMatch(sign1);
			if (match != -1)
			{
				phonemeIndex[outputPos++] = match;
				continue;
			}
			match = Constants.StressCharTable.IndexOf(sign1);
			if (match != -1)
			{
				phonemeStress[outputPos - 1] = match + 1;
				continue;
			}
			return false;
		}
		phonemeIndex[255] = 255;
		return true;
	}

	public void RewritePhonemes()
	{
		int pos = 0;
		int phoneme;
		while ((phoneme = phonemeIndex[++pos]) != 255)
		{
			if (phoneme == 0)
			{
				continue;
			}
			if (Constants.HasFlag(phoneme, 16))
			{
				Insert(pos + 1, Constants.HasFlag(phoneme, 32) ? 21 : 20);
				HandleUW_CH_J(phoneme, pos);
				continue;
			}
			switch (phoneme)
			{
			case 78:
				Change2AX(pos, 24);
				continue;
			case 79:
				Change2AX(pos, 27);
				continue;
			case 80:
				Change2AX(pos, 28);
				continue;
			}
			if (Constants.HasFlag(phoneme, 128) && phonemeStress[pos] != 0 && pos <= 253)
			{
				if (phonemeIndex[pos + 1] == 0)
				{
					phoneme = phonemeIndex[pos + 2];
					if (phoneme != 0 && Constants.HasFlag(phoneme, 128) && phonemeStress[pos + 2] != 0)
					{
						Insert(pos + 2, 31, 0, 0);
					}
				}
			}
			else if (phonemeIndex[pos - 1] == 23)
			{
				HandleTR_DR_R(phonemeIndex[pos - 1], pos);
			}
			else if (Constants.HasFlag(phoneme, 1))
			{
				if (phonemeIndex[pos - 1] == 32)
				{
					phonemeIndex[pos] = phoneme - 12;
				}
			}
			else
			{
				HandleUW_CH_J(phoneme, pos);
			}
		}
		void Change2AX(int num, int suffix)
		{
			phonemeIndex[num] = 13;
			Insert(num + 1, suffix);
		}
		void HandleTR_DR_R(int num, int num2)
		{
			switch (num)
			{
			case 57:
				phonemeIndex[num2 - 1] = 44;
				break;
			case 69:
				phonemeIndex[num2 - 1] = 42;
				break;
			default:
				if (Constants.HasFlag(num, 128))
				{
					phonemeIndex[num2] = 18;
				}
				break;
			}
		}
		void HandleUW_CH_J(int num, int num2)
		{
			switch (num)
			{
			case 42:
				Insert(num2 + 1, 43);
				break;
			case 44:
				Insert(num2 + 1, 45);
				break;
			case 53:
				if (Constants.HasFlag(phonemeIndex[num2 - 1], 1024))
				{
					phonemeIndex[num2] = 16;
				}
				break;
			}
		}
	}

	public void SetPhonemeLength()
	{
		int pos = 0;
		int phoneme;
		while ((phoneme = phonemeIndex[++pos]) != 255)
		{
			int stress = phonemeStress[pos];
			if (stress == 0 || (stress & 0x80) != 0)
			{
				phonemeLength[pos] = Constants.PhonemeLengthTable[phoneme] & 0xFF;
			}
			else
			{
				phonemeLength[pos] = Constants.PhonemeLengthTable[phoneme] >> 8;
			}
		}
	}

	public void InsertPauses()
	{
		int pos = 0;
		int phoneme;
		while ((phoneme = phonemeIndex[++pos]) != 255)
		{
			if (phoneme != 254 && Constants.HasFlag(phoneme, 256))
			{
				Insert(++pos, 254);
			}
		}
	}

	public void PrepareOutput()
	{
		Buffer = new Buffer();
		int pos = 0;
		int outputPos = 0;
		int phoneme;
		while ((phoneme = phonemeIndex[++pos]) != 255)
		{
			switch (phoneme)
			{
			case 254:
				Legacy.IndexOutput[outputPos] = 255;
				Legacy.Render();
				outputPos = 0;
				break;
			default:
				Legacy.IndexOutput[outputPos] = phoneme;
				Legacy.LengthOutput[outputPos] = phonemeLength[pos];
				Legacy.StressOutput[outputPos] = phonemeStress[pos];
				outputPos++;
				break;
			case 0:
				break;
			}
		}
		Legacy.IndexOutput[outputPos] = 255;
		Legacy.Render();
	}
}
