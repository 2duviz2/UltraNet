using System.Collections.Generic;

namespace Jaket.Sam;

public class Buffer
{
	private List<int> data = new List<int>();

	private int lastTimeTableIndex;

	public int Position;

	public void Set(int pos, int value)
	{
		while (pos >= data.Count)
		{
			data.Add(0);
		}
		data[pos] = value;
	}

	public void WriteArray(int index, int[] array)
	{
		Position += Constants.TimeTable[lastTimeTableIndex, index];
		lastTimeTableIndex = index;
		for (int i = 0; i < 5; i++)
		{
			Set(Position / 50 + i, array[i]);
		}
	}

	public void Write(int index, int v)
	{
		WriteArray(index, new int[5] { v, v, v, v, v });
	}

	public float[] GetFloats()
	{
		float[] floats = new float[data.Count];
		for (int i = 0; i < data.Count; i++)
		{
			floats[i] = (float)(data[i] - 127) / 255f;
		}
		return floats;
	}
}
