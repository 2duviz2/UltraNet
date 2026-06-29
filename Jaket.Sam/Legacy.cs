using System;
using UnityEngine;

namespace Jaket.Sam;

public class Legacy
{
	public Sam Sam;

	private int A;

	private int X;

	private int Y;

	private int[] temp;

	public int[] StressOutput = new int[256];

	public int[] LengthOutput = new int[256];

	public int[] IndexOutput = new int[256];

	private int[] pitches = new int[256];

	private int[] sampledConsonantFlag = new int[256];

	private int[,] frequency = new int[3, 256];

	private int[,] amplitude = new int[3, 256];

	private int mem39;

	private int mem44;

	private int mem47;

	private int mem49;

	private int mem50;

	private int mem51;

	private int mem53;

	private int mem56;

	public Legacy(Sam sam)
	{
		Sam = sam;
	}

	public static void Inc(ref int value)
	{
		value = ++value & 0xFF;
	}

	public static void Dec(ref int value)
	{
		value = --value & 0xFF;
	}

	public bool Text2Phonemes(ref int[] input)
	{
		int mem56 = 255;
		int mem61 = 255;
		int mem64 = 0;
		temp = new int[256];
		temp[0] = 32;
		X = 1;
		Y = 0;
		do
		{
			A = input[Y] & 0x7F;
			if (A >= 112)
			{
				A &= 95;
			}
			else if (A >= 96)
			{
				A &= 79;
			}
			temp[X] = A;
			Inc(ref X);
			Inc(ref Y);
		}
		while (Y != 255);
		A = 255;
		X = 255;
		temp[X] = 27;
		while (true)
		{
			Inc(ref mem61);
			X = mem61;
			if (X < temp.Length)
			{
				mem64 = (A = temp[X]);
				if (A == 91)
				{
					Inc(ref mem56);
					X = mem56;
					A = 155;
					input[X] = 155;
					Inc(ref X);
					int[] copy = new int[X];
					Array.Copy(input, copy, X);
					input = copy;
					return true;
				}
				if (A == 46)
				{
					Inc(ref X);
					Y = temp[X];
					A = Constants.Tab1[Y] & 1;
					if (A == 0)
					{
						Inc(ref mem56);
						X = mem56;
						input[X] = 46;
						continue;
					}
				}
			}
			A = mem64;
			Y = A;
			A = Constants.Tab1[A];
			int mem65 = A;
			int mem66;
			if ((A & 2) != 0)
			{
				mem66 = 37541;
			}
			else
			{
				A = mem65;
				if (A == 0)
				{
					A = 32;
					if (X >= temp.Length)
					{
						return true;
					}
					temp[X] = 32;
					Inc(ref mem56);
					X = mem56;
					if (X <= 120)
					{
						input[X] = A;
						continue;
					}
					input[X] = 155;
					A = mem61;
					return true;
				}
				A = mem65 & 0x80;
				if (A == 0)
				{
					break;
				}
				X = mem64 - 65;
				mem66 = Constants.Tab2[X] | (Constants.Tab3[X] << 8);
			}
			int mem69;
			while (true)
			{
				IL_0377:
				Y = 0;
				do
				{
					mem66++;
					A = GetRuleByte(mem66, Y);
				}
				while ((A & 0x80) == 0);
				Inc(ref Y);
				while (true)
				{
					A = GetRuleByte(mem66, Y);
					if (A == 40)
					{
						break;
					}
					Inc(ref Y);
				}
				int mem67 = Y;
				do
				{
					Inc(ref Y);
					A = GetRuleByte(mem66, Y);
				}
				while (A != 41);
				int mem68 = Y;
				do
				{
					Inc(ref Y);
					A = GetRuleByte(mem66, Y);
					A &= 127;
				}
				while (A != 61);
				mem64 = Y;
				X = mem61;
				mem69 = X;
				Y = mem67;
				Inc(ref Y);
				while (true)
				{
					mem65 = temp[X];
					A = GetRuleByte(mem66, Y);
					if (A != mem65)
					{
						break;
					}
					Inc(ref Y);
					if (Y == mem68)
					{
						goto IL_051b;
					}
					Inc(ref X);
					mem69 = X;
				}
				continue;
				IL_051b:
				A = mem61;
				int mem70 = mem61;
				while (true)
				{
					Dec(ref mem67);
					Y = mem67;
					A = GetRuleByte(mem66, Y);
					mem65 = A;
					if ((A & 0x80) != 0)
					{
						break;
					}
					X = A & 0x7F;
					A = Constants.Tab1[X] & 0x80;
					if (A != 0)
					{
						X = mem70 - 1;
						A = temp[X];
						if (A != mem65)
						{
							goto IL_0377;
						}
						mem70 = X;
						continue;
					}
					A = mem65;
					if (A != 32)
					{
						if (A != 35)
						{
							if (A != 46)
							{
								if (A != 38)
								{
									if (A != 64)
									{
										if (A != 94)
										{
											if (A != 43)
											{
												if (A != 58)
												{
													return false;
												}
												while (true)
												{
													UnknownCode(mem70);
													A &= 32;
													if (A == 0)
													{
														break;
													}
													mem70 = X;
												}
												continue;
											}
											X = mem70;
											Dec(ref X);
											A = temp[X];
											if (A != 69 && A != 73 && A != 89)
											{
												goto IL_0377;
											}
										}
										else
										{
											UnknownCode(mem70);
											A &= 32;
											if (A == 0)
											{
												goto IL_0377;
											}
										}
										mem70 = X;
										continue;
									}
									UnknownCode(mem70);
									A &= 4;
									if (A == 0)
									{
										A = temp[X];
										if (A != 72 || (A != 84 && A != 67 && A != 83))
										{
											goto IL_0377;
										}
										mem70 = X;
										continue;
									}
								}
								else
								{
									UnknownCode(mem70);
									A &= 16;
									if (A == 0)
									{
										A = temp[X];
										if (A != 72)
										{
											goto IL_0377;
										}
										Dec(ref X);
										A = temp[X];
										if (A != 67 && A != 83)
										{
											goto IL_0377;
										}
									}
								}
							}
							else
							{
								UnknownCode(mem70);
								A &= 8;
								if (A == 0)
								{
									goto IL_0377;
								}
							}
							mem70 = X;
							continue;
						}
						UnknownCode(mem70);
						A &= 64;
						if (A == 0)
						{
							goto IL_0377;
						}
					}
					else
					{
						UnknownCode(mem70);
						A &= 128;
						if (A != 0)
						{
							goto IL_0377;
						}
					}
					mem70 = X;
				}
				A = mem69;
				int mem71 = A;
				while (true)
				{
					Y = mem68 + 1;
					if (Y == mem64)
					{
						break;
					}
					mem68 = Y;
					A = GetRuleByte(mem66, Y);
					mem65 = A;
					X = A;
					A = Constants.Tab1[X] & 0x80;
					if (A != 0)
					{
						X = mem71 + 1;
						A = temp[X];
						if (A != mem65)
						{
							goto IL_0377;
						}
						mem71 = X;
						continue;
					}
					A = mem65;
					if (A != 32)
					{
						if (A != 35)
						{
							if (A != 46)
							{
								if (A != 38)
								{
									if (A != 64)
									{
										if (A != 94)
										{
											if (A != 43)
											{
												if (A != 58)
												{
													if (A == 37 || A == 37)
													{
														X = mem71 + 1;
														A = temp[X];
														if (A == 69)
														{
															Inc(ref X);
															Y = temp[X];
															Dec(ref X);
															A = Constants.Tab1[Y] & 0x80;
															if (A != 0)
															{
																Inc(ref X);
																A = temp[X];
																if (A != 82 && A != 83 && A != 68)
																{
																	if (A == 76)
																	{
																		Inc(ref X);
																		A = temp[X];
																		if (A != 89)
																		{
																			goto IL_0377;
																		}
																	}
																	else
																	{
																		if (A != 70)
																		{
																			goto IL_0377;
																		}
																		Inc(ref X);
																		A = temp[X];
																		if (A != 85)
																		{
																			goto IL_0377;
																		}
																		Inc(ref X);
																		A = temp[X];
																		if (A != 76)
																		{
																			goto IL_0377;
																		}
																	}
																}
															}
														}
														else
														{
															if (A != 73)
															{
																goto IL_0377;
															}
															Inc(ref X);
															A = temp[X];
															if (A != 78)
															{
																goto IL_0377;
															}
															Inc(ref X);
															A = temp[X];
															if (A != 71)
															{
																goto IL_0377;
															}
														}
														mem71 = X;
														continue;
													}
													return false;
												}
												while (true)
												{
													UnknownCode2(mem71);
													A &= 32;
													if (A == 0)
													{
														break;
													}
													mem71 = X;
												}
												continue;
											}
											X = mem71;
											Inc(ref X);
											A = temp[X];
											if (A != 69 && A != 73 && A != 89)
											{
												goto IL_0377;
											}
										}
										else
										{
											UnknownCode2(mem71);
											A &= 32;
											if (A == 0)
											{
												goto IL_0377;
											}
										}
										mem71 = X;
										continue;
									}
									UnknownCode2(mem71);
									A &= 4;
									if (A == 0)
									{
										A = temp[X];
										if (A != 72 || (A != 84 && A != 67 && A != 83))
										{
											goto IL_0377;
										}
										mem71 = X;
										continue;
									}
								}
								else
								{
									UnknownCode2(mem71);
									A &= 16;
									if (A == 0)
									{
										A = temp[X];
										if (A != 72)
										{
											goto IL_0377;
										}
										Inc(ref X);
										A = temp[X];
										if (A != 67 && A != 83)
										{
											goto IL_0377;
										}
									}
								}
							}
							else
							{
								UnknownCode2(mem71);
								A &= 8;
								if (A == 0)
								{
									goto IL_0377;
								}
							}
							mem71 = X;
							continue;
						}
						UnknownCode2(mem71);
						A &= 64;
						if (A == 0)
						{
							goto IL_0377;
						}
					}
					else
					{
						UnknownCode2(mem71);
						A &= 128;
						if (A != 0)
						{
							goto IL_0377;
						}
					}
					mem71 = X;
				}
				break;
			}
			Y = mem64;
			mem61 = mem69;
			while (true)
			{
				A = GetRuleByte(mem66, Y);
				mem65 = A;
				A &= 127;
				if (A != 61)
				{
					Inc(ref mem56);
					X = mem56;
					input[X] = A;
				}
				if ((mem65 & 0x80) != 0)
				{
					break;
				}
				Inc(ref Y);
			}
		}
		return false;
		static int GetRuleByte(int num, int Y)
		{
			return (num >= 37541) ? Constants.RulesSet2[num - 37541 + Y] : Constants.RulesSet1[num - 32000 + Y];
		}
		void UnknownCode(int num)
		{
			A = Constants.Tab1[Y = temp[X = num - 1]];
		}
		void UnknownCode2(int x)
		{
			X = x;
			Inc(ref X);
			A = Constants.Tab1[Y = temp[X]];
		}
	}

	private void AddInflection(int mem48, int phase1)
	{
		int Atemp = (mem49 = (A = X));
		A -= 30;
		if (Atemp <= 30)
		{
			A = 0;
		}
		X = A;
		while ((A = pitches[X]) == 127)
		{
			Inc(ref X);
		}
		while (true)
		{
			pitches[X] = (phase1 = (A + mem48) & 0xFF);
			do
			{
				Inc(ref X);
				if (X == mem49)
				{
					return;
				}
			}
			while (pitches[X] == 255);
			A = phase1;
		}
	}

	private void RenderSample(ref int mem66)
	{
		int tempA = 0;
		mem49 = Y;
		A = mem39 & 7;
		mem53 = Constants.Tab5[mem47 = (mem56 = (X = A - 1))];
		A = mem39 & 0xF8;
		if (A == 0)
		{
			A = pitches[mem49] >> 4;
			int phase1 = A ^ 0xFF;
			Y = mem66;
			do
			{
				mem56 = 8;
				A = Constants.SampleTable[mem47 * 256 + Y];
				do
				{
					tempA = A;
					A <<= 1;
					if ((tempA & 0x80) != 0)
					{
						Sam.Buffer.Write(3, 160);
					}
					else
					{
						Sam.Buffer.Write(4, 96);
					}
					Dec(ref mem56);
				}
				while (mem56 != 0);
				Inc(ref Y);
				Inc(ref phase1);
			}
			while (phase1 != 0);
			A = 1;
			mem44 = 1;
			mem66 = Y;
			Y = mem49;
			return;
		}
		Y = A ^ 0xFF;
		do
		{
			mem56 = 8;
			A = Constants.SampleTable[mem47 * 256 + Y];
			do
			{
				tempA = A;
				A <<= 1;
				if ((tempA & 0x80) == 0)
				{
					X = mem53;
					Sam.Buffer.Write(1, (X & 0xF) * 16);
					if (X != 0)
					{
						goto IL_0136;
					}
				}
				Sam.Buffer.Write(2, 80);
				goto IL_0136;
				IL_0136:
				X = 0;
				Dec(ref mem56);
			}
			while (mem56 != 0);
			Inc(ref Y);
		}
		while (Y != 0);
		mem44 = 1;
		Y = mem49;
	}

	public void Render()
	{
		int phase1 = 0;
		int mem66 = 0;
		if (IndexOutput[0] == 255)
		{
			return;
		}
		A = (X = (mem44 = 0));
		int mem67;
		int phase2;
		do
		{
			mem56 = (A = IndexOutput[Y = mem44]);
			if (A == 255)
			{
				break;
			}
			if (A == 1)
			{
				AddInflection(mem67 = (A = 1), phase1);
			}
			if (A == 2)
			{
				AddInflection(mem67 = 255, phase1);
			}
			phase1 = Constants.Tab4[StressOutput[Y] + 1];
			phase2 = LengthOutput[Y];
			Y = mem56;
			do
			{
				pitches[X] = Sam.Pitch + phase1;
				sampledConsonantFlag[X] = Constants.SampledConsonantFlags[Y];
				frequency[0, X] = Constants.PhonemeFrequencyTable[Y] & 0xFF;
				frequency[1, X] = (Constants.PhonemeFrequencyTable[Y] >> 8) & 0xFF;
				frequency[2, X] = (Constants.PhonemeFrequencyTable[Y] >> 16) & 0xFF;
				amplitude[0, X] = Constants.PhonemeAmplitudesTable[Y] & 0xFF;
				amplitude[1, X] = (Constants.PhonemeAmplitudesTable[Y] >> 8) & 0xFF;
				amplitude[2, X] = (Constants.PhonemeAmplitudesTable[Y] >> 16) & 0xFF;
				Inc(ref X);
				Dec(ref phase2);
			}
			while (phase2 != 0);
			Inc(ref mem44);
		}
		while (mem44 != 0);
		X = (mem44 = (mem49 = 0));
		int phase3;
		int speedcounter;
		int mem68;
		while (true)
		{
			Y = IndexOutput[X];
			A = IndexOutput[X + 1];
			if (A == 255)
			{
				break;
			}
			mem56 = Constants.BlendRank[X = A];
			A = Constants.BlendRank[Y];
			if (A == mem56)
			{
				phase1 = Constants.OutBlend[Y];
				phase2 = Constants.OutBlend[X];
			}
			else if (A < mem56)
			{
				phase1 = Constants.InBlend[X];
				phase2 = Constants.OutBlend[X];
			}
			else
			{
				phase1 = Constants.OutBlend[Y];
				phase2 = Constants.InBlend[Y];
			}
			Y = mem44;
			mem49 = (A = mem49 + LengthOutput[mem44]);
			A += phase2;
			speedcounter = A;
			mem47 = 168;
			phase3 = mem49 - phase1;
			mem68 = (A = phase1 + phase2);
			X = A - 2;
			if ((X & 0x80) == 0)
			{
				do
				{
					int mem69 = mem68;
					if (mem47 == 168)
					{
						int mem70 = LengthOutput[mem44] >> 1;
						int mem71 = LengthOutput[mem44 + 1] >> 1;
						mem69 = mem70 + mem71;
						mem71 += mem49;
						mem70 = mem49 - mem70;
						A = Read(mem47, mem71);
						Y = mem70;
						mem53 = A - Read(mem47, mem70);
					}
					else
					{
						A = Read(mem47, speedcounter);
						Y = phase3;
						mem53 = A - Read(mem47, phase3);
					}
					mem50 = mem53 & 0x80;
					int m53abs = Mathf.Abs(mem53);
					mem51 = m53abs % mem69;
					mem53 /= mem69;
					X = mem69;
					Y = phase3;
					mem56 = 0;
					while (true)
					{
						mem67 = (A = Read(mem47, Y) + mem53);
						Inc(ref Y);
						Dec(ref X);
						if (X == 0)
						{
							break;
						}
						mem56 += mem51;
						if (mem56 >= mem69)
						{
							mem56 -= mem69;
							if ((mem50 & 0x80) == 0)
							{
								if (mem67 != 0)
								{
									Inc(ref mem67);
								}
							}
							else
							{
								Dec(ref mem67);
							}
						}
						Write(mem47, Y, mem67);
					}
					Inc(ref mem47);
				}
				while (mem47 != 175);
			}
			Inc(ref mem44);
			X = mem44;
		}
		mem67 = mem49 + LengthOutput[mem44];
		for (int i = 0; i < 256; i++)
		{
			pitches[i] -= frequency[0, i] >> 1;
		}
		phase1 = (phase2 = (phase3 = (mem49 = 0)));
		speedcounter = 72;
		for (int i2 = 255; i2 >= 0; i2--)
		{
			amplitude[0, i2] = Constants.AmplitudeRescale[amplitude[0, i2]];
			amplitude[1, i2] = Constants.AmplitudeRescale[amplitude[1, i2]];
			amplitude[2, i2] = Constants.AmplitudeRescale[amplitude[2, i2]];
		}
		Y = 0;
		mem44 = (A = (X = pitches[0]));
		mem68 = A - (A >> 2);
		bool unknownBool = false;
		while (true)
		{
			mem39 = (A = sampledConsonantFlag[Y]);
			A &= 248;
			if (A != 0)
			{
				RenderSample(ref mem66);
				Y += 2;
				mem67 -= 2;
			}
			else
			{
				int[] ary = new int[5];
				int p1 = phase1 * 256;
				int p2 = phase2 * 256;
				int p3 = phase3 * 256;
				for (int k = 0; k < 5; k++)
				{
					int sp1 = Constants.Sinus[0xFF & (p1 >> 8)];
					int sp2 = Constants.Sinus[0xFF & (p2 >> 8)];
					int rp3 = ((((p3 >> 8) & 0xFF) <= 127) ? 144 : 112);
					int sin1 = sp1 * (amplitude[0, Y] & 0xF);
					int sin2 = sp2 * (amplitude[1, Y] & 0xF);
					int rect = rp3 * (amplitude[2, Y] & 0xF);
					ary[k] = ((sin1 + sin2 + rect) / 32 + 128) & 0xFF;
					p1 += frequency[0, Y] * 64;
					p2 += frequency[1, Y] * 64;
					p3 += frequency[2, Y] * 64;
				}
				Sam.Buffer.WriteArray(0, ary);
				Dec(ref speedcounter);
				if (speedcounter != 0)
				{
					goto IL_0953;
				}
				Inc(ref Y);
				Dec(ref mem67);
			}
			if (mem67 == 0)
			{
				break;
			}
			speedcounter = Sam.Speed;
			goto IL_0953;
			IL_0953:
			Dec(ref mem44);
			while (true)
			{
				if (mem44 == 0 || unknownBool)
				{
					unknownBool = false;
					mem44 = (A = pitches[Y]);
					mem68 = (A -= A >> 2);
					phase1 = (phase2 = (phase3 = 0));
					break;
				}
				Dec(ref mem68);
				if (mem68 != 0 || mem39 == 0)
				{
					phase1 = (phase1 + frequency[0, Y]) & 0xFF;
					phase2 = (phase2 + frequency[1, Y]) & 0xFF;
					phase3 = (phase3 + frequency[2, Y]) & 0xFF;
					break;
				}
				RenderSample(ref mem66);
				unknownBool = true;
			}
		}
		int Read(int type, int index)
		{
			if (1 == 0)
			{
			}
			int result = type switch
			{
				168 => pitches[index], 
				169 => frequency[0, index], 
				170 => frequency[1, index], 
				171 => frequency[2, index], 
				172 => amplitude[0, index], 
				173 => amplitude[1, index], 
				174 => amplitude[2, index], 
				_ => 0, 
			};
			if (1 == 0)
			{
			}
			return result;
		}
		void Write(int type, int index, int value)
		{
			switch (type)
			{
			case 168:
				pitches[Y] = value;
				break;
			case 169:
				frequency[0, Y] = value;
				break;
			case 170:
				frequency[1, Y] = value;
				break;
			case 171:
				frequency[2, Y] = value;
				break;
			case 172:
				amplitude[0, Y] = value;
				break;
			case 173:
				amplitude[1, Y] = value;
				break;
			case 174:
				amplitude[2, Y] = value;
				break;
			}
		}
	}
}
