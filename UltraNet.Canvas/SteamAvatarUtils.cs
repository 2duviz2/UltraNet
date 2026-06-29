using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace UltraNet.Canvas;

public static class SteamAvatarUtils
{
	public static async Task<Sprite> GetAvatarSpriteAsync(ulong steamId)
	{
		Image? image = await new Friend(steamId).GetLargeAvatarAsync();
		if (!image.HasValue)
		{
			return null;
		}
		Texture2D tex = ConvertSteamImageToTexture(image.Value);
		return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}

	public static Texture2D ConvertSteamImageToTexture(Image image)
	{
		Texture2D tex = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.RGBA32, mipChain: false);
		tex.LoadRawTextureData(image.Data);
		tex.Apply();
		return tex;
	}
}
