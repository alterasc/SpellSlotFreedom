using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// thanks to ADDB, specifically Playable Navigator mod for most of this code

namespace SpellSlotFreedom;

public static class Storage
{
    public class PerSaveSettings
    {
        public const string ID = "SpellSlotFreedom.PerSaveSettings";

        [JsonProperty]
        public Dictionary<string, int> SpellsOnHigherLevels = [];
    }

    private static PerSaveSettings cachedPerSave = null;
    public static void ClearCachedPerSave() => cachedPerSave = null;
    public static void ReloadPerSaveSettings()
    {
        var player = Game.Instance?.Player;
        if (player == null || Game.Instance.SaveManager.CurrentState == SaveManager.State.Loading) return;
        if (player.SettingsList.TryGetValue(PerSaveSettings.ID, out var obj) && obj is string json)
        {
            try
            {
                cachedPerSave = JsonConvert.DeserializeObject<PerSaveSettings>(json);
            }
            catch (Exception)
            {
            }
        }
        if (cachedPerSave == null)
        {
            cachedPerSave = new PerSaveSettings();
            SavePerSaveSettings();
        }
    }

    public static void SavePerSaveSettings()
    {
        var player = Game.Instance?.Player;
        if (player == null) return;
        if (cachedPerSave == null)
            ReloadPerSaveSettings();
        var json = JsonConvert.SerializeObject(cachedPerSave);
        player.SettingsList[PerSaveSettings.ID] = json;
    }

    public static PerSaveSettings PerSave
    {
        get
        {
            try
            {
                if (cachedPerSave != null) return cachedPerSave;
                ReloadPerSaveSettings();
            }
            catch (Exception) { }
            return cachedPerSave;
        }
    }
}