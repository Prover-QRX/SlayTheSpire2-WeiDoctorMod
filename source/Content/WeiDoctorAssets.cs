using System;
using System.IO;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace WeiDoctor.Content;

public static class WeiDoctorAssets
{
    private static readonly object TextureLock = new();
    private static readonly Dictionary<string, Texture2D?> TextureCache = new(StringComparer.OrdinalIgnoreCase);

    public static string ModDirectory
    {
        get
        {
            string? directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return string.IsNullOrWhiteSpace(directory) ? AppContext.BaseDirectory : directory;
        }
    }

    public static string? PathOrFallback(string relativePath, string? fallback)
    {
        string path = AbsolutePath(relativePath);
        return File.Exists(path) ? path : fallback;
    }

    public static string AbsolutePath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(ModDirectory, relativePath));
    }

    public static Texture2D? LoadTexture(string relativePath)
    {
        string path = AbsolutePath(relativePath);
        lock (TextureLock)
        {
            if (TextureCache.TryGetValue(path, out Texture2D? cached))
            {
                return cached;
            }

            Texture2D? texture = null;
            try
            {
                if (File.Exists(path))
                {
                    Image image = Image.LoadFromFile(path);
                    texture = ImageTexture.CreateFromImage(image);
                }
            }
            catch (Exception ex)
            {
                Entry.Logger.Warn($"[Assets] Failed to load texture '{path}': {ex.Message}");
            }

            TextureCache[path] = texture;
            return texture;
        }
    }

    public static void RegisterExternalAssetProviders()
    {
        ExternalAssetOverrideRegistry.RegisterRelicIconTextureProvider(Entry.ModId, RelicTexture);
        ExternalAssetOverrideRegistry.RegisterRelicIconOutlineTextureProvider(Entry.ModId, RelicTexture);
        ExternalAssetOverrideRegistry.RegisterRelicBigIconTextureProvider(Entry.ModId, RelicTexture);
    }

    private static Texture2D? RelicTexture(RelicModel relic)
    {
        return relic is DispatchCenterRelic
            ? LoadTexture("assets/icons/盟约图标/远见.png") ?? LoadTexture("assets/icons/机制图标/炎佑_令召唤物替代.png")
            : null;
    }
}
