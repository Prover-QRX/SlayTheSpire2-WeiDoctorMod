using System.IO;
using System.Text.Json;
using WeiDoctor.BetaAdapter;

namespace WeiDoctor;

public sealed class WeiDoctorMod
{
    public const string Version = "1.0.0";

    public void Initialize(ISts2BetaApi api, string dataDirectory)
    {
        var settings = JsonSerializer.Deserialize<DoctorModSettings>(
            File.ReadAllText(Path.Combine(dataDirectory, "config", "default_settings.json"))) 
            ?? new DoctorModSettings(false, 3, 10, 3);
        api.RegisterSettings(settings);

        // TODO(beta API): deserialize data/cards.json and data/relics.json,
        // then register the Doctor character, cards, relics, reward hooks, rest-site action,
        // dispatch-center reward filter, deployment triggers, and promotion watcher.
    }
}
