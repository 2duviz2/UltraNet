using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraNet.Canvas;

public static class ChatParser
{
	public static List<ChatMessage> Parse(string json)
	{
		try
		{
			string wrapped = "{ \"messages\": " + json + "}";
			ChatMessageList result = JsonUtility.FromJson<ChatMessageList>(wrapped);
			return result.messages;
		}
		catch (Exception)
		{
			return new List<ChatMessage>();
		}
	}
}
