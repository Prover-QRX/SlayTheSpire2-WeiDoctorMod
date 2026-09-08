using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using WeiDoctor.Content;

namespace WeiDoctor;

[ModInitializer("Initialize")]
public static class Entry
{
    public const string ModId = "WeiDoctor";
    public const string ResPath = "res://WeiDoctor";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileLog("Initialize entered.");

        try
        {
            RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
            ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
            WeiDoctorAssets.RegisterExternalAssetProviders();
            RitsuLibFramework.CreateContentPack(ModId)
                .SharedCardPool<DoctorCardPool>()
                .SharedRelicPool<DoctorRelicPool>()
            .SharedPotionPool<DoctorPotionPool>()
            .Character<DoctorCharacter>(entry =>
            {
                entry.AddStartingCard<TacticalFront>(4);
                entry.AddStartingCard<TacticalBack>(4);
                entry.AddStartingCard<YinxianOperator>(1);
                entry.AddStartingCard<JiaofengOperator>(1);
                entry.AddStartingRelic<DispatchCenterRelic>(1);
            })
            .Apply();

            Harmony harmony = new(ModId);
            harmony.PatchAll(assembly);
            int patchCount = harmony.GetPatchedMethods().Count();

            Logger.Info($"WeiDoctor v1.0 bootstrap initialized. Patch count: {patchCount}.");
            FileLog($"Initialize succeeded. Patch count: {patchCount}.");
        }
        catch (Exception ex)
        {
            Logger.Error($"WeiDoctor initialization failed: {ex}");
            FileLog($"Initialize failed: {ex}");
            throw;
        }
    }

    private static void FileLog(string message)
    {
        try
        {
            string? directory = Path.GetDirectoryName(typeof(Entry).Assembly.Location);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            string logPath = Path.Combine(directory, "WeiDoctor.load.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never prevent the mod from loading.
        }
    }
}
