using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ravenfield.Configuration;
using UnityEngine;

namespace RavenM
{
    internal static class Ea36Compat
    {
        public const int ExpectedBuildNumber = 36;

        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static bool HasInstantActionMenu => InstantActionConfigMenu.instance != null;

        public static int GameModeValue
        {
            get
            {
                var menu = InstantActionConfigMenu.instance;
                if (menu == null || menu.selectedGameMode == null || menu.gameModes == null)
                    return 0;

                int index = Array.IndexOf(menu.gameModes, menu.selectedGameMode.legacyType);
                return Math.Max(index, 0);
            }
            set
            {
                var menu = InstantActionConfigMenu.instance;
                if (menu == null || menu.gameModes == null || value < 0 || value >= menu.gameModes.Length)
                    return;

                var info = GetOfficialGameModeInfo(menu.gameModes[value]);
                if (info != null)
                    menu.SetGameMode(info, true);
            }
        }

        public static string GameModeText => InstantActionConfigMenu.instance?.selectedGameMode?.name ?? "Unknown";

        public static int TeamValue
        {
            get => GetProperty<int>(GetField<object>(InstantActionConfigMenu.instance, "playerTeamDD"), "value");
            set => SetProperty(GetField<object>(InstantActionConfigMenu.instance, "playerTeamDD"), "value", value);
        }

        public static bool NightMode
        {
            get => GetProperty<bool>(GetField<object>(InstantActionConfigMenu.instance, "nightToggle"), "isOn");
            set => SetProperty(GetField<object>(InstantActionConfigMenu.instance, "nightToggle"), "isOn", value);
        }

        public static bool ConfigFlags
        {
            get => GetProperty<bool>(GetField<object>(InstantActionConfigMenu.instance, "configFlagsToggle"), "isOn");
            set => SetProperty(GetField<object>(InstantActionConfigMenu.instance, "configFlagsToggle"), "isOn", value);
        }

        public static string BotNumberText
        {
            get
            {
                int eagle = ParseInt(GetProperty<string>(GetField<object>(InstantActionConfigMenu.instance, "botAmountEagleIF"), "text"));
                int raven = ParseInt(GetProperty<string>(GetField<object>(InstantActionConfigMenu.instance, "botAmountRavenIF"), "text"));
                return (eagle + raven).ToString();
            }
            set
            {
                int total = ParseInt(value);
                SetBotAmounts(total / 2, total - total / 2);
            }
        }

        public static float BalanceValue
        {
            get
            {
                int eagle = ParseInt(GetProperty<string>(GetField<object>(InstantActionConfigMenu.instance, "botAmountEagleIF"), "text"));
                int raven = ParseInt(GetProperty<string>(GetField<object>(InstantActionConfigMenu.instance, "botAmountRavenIF"), "text"));
                int total = eagle + raven;
                return total <= 0 ? 0.5f : (float)eagle / total;
            }
            set
            {
                int total = ParseInt(BotNumberText);
                int eagle = Mathf.RoundToInt(total * Mathf.Clamp01(value));
                SetBotAmounts(eagle, total - eagle);
            }
        }

        public static string RespawnTimeText
        {
            get => GetProperty<string>(GetField<object>(InstantActionConfigMenu.instance, "respawnTimeIF"), "text") ?? "";
            set => SetProperty(GetField<object>(InstantActionConfigMenu.instance, "respawnTimeIF"), "text", value);
        }

        public static int GameLengthValue
        {
            get => GetProperty<int>(GetField<object>(InstantActionConfigMenu.instance, "gameLengthDD"), "value");
            set => SetProperty(GetField<object>(InstantActionConfigMenu.instance, "gameLengthDD"), "value", value);
        }

        public static int LoadedLevelEntry
        {
            get
            {
                var selected = SelectedMap;
                if (selected == null)
                    return 0;

                var maps = MapEntries;
                int index = maps.FindIndex(x => SameMap(x, selected));
                return Math.Max(index, 0);
            }
            set
            {
                var maps = MapEntries;
                if (value >= 0 && value < maps.Count)
                    InstantActionConfigMenu.instance?.SelectMap(maps[value]);
            }
        }

        public static string SelectedMapKey
        {
            get
            {
                var selected = SelectedMap;
                return selected == null ? "" : GetMapKey(selected);
            }
        }

        public static bool SelectMapByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var maps = MapEntries;
            var selected = maps.FirstOrDefault(map => GetMapKey(map) == key)
                ?? maps.FirstOrDefault(map => GetMapFallbackKey(map) == key);

            if (selected == null)
                return false;

            InstantActionConfigMenu.instance?.SelectMap(selected);
            return true;
        }

        public static string SelectedMapName
        {
            get
            {
                var selected = SelectedMap;
                return selected == null ? "" : selected.GetOrLoadMetadata().displayName;
            }
        }

        public static List<MapEntryData> MapEntries => ModManager.instance == null
            ? new List<MapEntryData>()
            : ModManager.AllAvailableMaps().ToList();

        public static int SkinValue(int team) => 0;

        public static void SetSkinValue(int team, int value)
        {
        }

        public static void RefreshSkinList()
        {
            TeamConfigPanel.instance?.UpdateSaveDropdowns();
        }

        public static void StartGame()
        {
            InstantActionConfigMenu.instance?.StartGame();
        }

        public static GameModeType GetGameModeType(GameModeBase gameMode)
        {
            if (gameMode is BattleMode) return GameModeType.Battle;
            if (gameMode is DominationMode) return GameModeType.Domination;
            if (gameMode is PointMatch) return GameModeType.PointMatch;
            if (gameMode is SkirmishMode) return GameModeType.Skirmish;
            if (gameMode is SpecOpsMode) return GameModeType.SpecOps;
            if (gameMode is SpookOpsMode) return GameModeType.Haunted;
            return GameModeType.Test;
        }

        public static GameObject GetTeamVehiclePrefab(TeamInfo teamInfo, VehicleSpawner.VehicleSpawnType type)
        {
            if (teamInfo.vehicleSlot.TryGetValue(type, out var slot))
                return slot.All().FirstOrDefault()?.prefab;
            return null;
        }

        public static void SetTeamVehiclePrefab(TeamInfo teamInfo, VehicleSpawner.VehicleSpawnType type, GameObject prefab)
        {
            if (!teamInfo.vehicleSlot.TryGetValue(type, out var slot))
            {
                slot = new VehicleSlotInfo();
                teamInfo.vehicleSlot[type] = slot;
            }

            slot.ClearEntries();
            if (prefab != null)
                slot.SetVehicleTier(RarityTier.Common, VehicleInfo.GetFromPrefab(prefab));
        }

        public static GameObject GetTeamTurretPrefab(TeamInfo teamInfo, TurretSpawner.TurretSpawnType type)
        {
            if (teamInfo.turretSlot.TryGetValue(type, out var slot))
                return slot.All().FirstOrDefault()?.prefab;
            return null;
        }

        public static void SetTeamTurretPrefab(TeamInfo teamInfo, TurretSpawner.TurretSpawnType type, GameObject prefab)
        {
            if (!teamInfo.turretSlot.TryGetValue(type, out var slot))
            {
                slot = new VehicleSlotInfo();
                teamInfo.turretSlot[type] = slot;
            }

            slot.ClearEntries();
            if (prefab != null)
                slot.SetVehicleTier(RarityTier.Common, VehicleInfo.GetFromPrefab(prefab));
        }

        public static string SerializeMutatorField(MutatorEntryData mutator, SortableConfigurationField field)
        {
            try
            {
                return ModManager.GetMutatorConfiguration(mutator).SerializeField(field.id);
            }
            catch
            {
                return field.FormatValueAsString();
            }
        }

        public static void DeserializeMutatorField(MutatorEntryData mutator, SortableConfigurationField field, string value)
        {
            try
            {
                ModManager.GetMutatorConfiguration(mutator).Deserialize(field.id, value);
            }
            catch (Exception e)
            {
                Plugin.logger?.LogWarning($"Failed to deserialize mutator field {field.id}: {e.Message}");
            }
        }

        private static MapEntryData SelectedMap => GetField<MapEntryData>(InstantActionConfigMenu.instance, "selectedMap");

        private static GameModeInfo GetOfficialGameModeInfo(GameModeType type)
        {
            if (type == GameModeType.Battle) return GameModeInfo.officialBattle;
            if (type == GameModeType.Domination) return GameModeInfo.officialDomination;
            if (type == GameModeType.PointMatch) return GameModeInfo.officialPointMatch;
            if (type == GameModeType.Skirmish) return GameModeInfo.officialSkirmish;
            if (type == GameModeType.SpecOps) return GameModeInfo.officialSpecOps;
            if (type == GameModeType.Haunted) return GameModeInfo.officialHaunted;
            return null;
        }

        private static bool SameMap(MapEntryData a, MapEntryData b)
        {
            return a != null && b != null && a.sceneName == b.sceneName && a.GetName() == b.GetName();
        }

        private static string GetMapKey(MapEntryData map)
        {
            if (map == null)
                return "";

            return string.Join("|", new[]
            {
                map.IsOfficial() ? "official" : "custom",
                EscapeKeyPart(map.sceneName),
                EscapeKeyPart(map.GetName()),
                EscapeKeyPart(map.GetModTitle()),
            });
        }

        private static string GetMapFallbackKey(MapEntryData map)
        {
            if (map == null)
                return "";

            return string.Join("|", new[]
            {
                map.IsOfficial() ? "official" : "custom",
                "",
                EscapeKeyPart(map.GetName()),
                EscapeKeyPart(map.GetModTitle()),
            });
        }

        private static string EscapeKeyPart(string value)
        {
            return (value ?? "").Replace("\\", "/").Replace("|", "%7C");
        }

        private static void SetBotAmounts(int eagle, int raven)
        {
            SetProperty(GetField<object>(InstantActionConfigMenu.instance, "botAmountEagleIF"), "text", eagle.ToString());
            SetProperty(GetField<object>(InstantActionConfigMenu.instance, "botAmountRavenIF"), "text", raven.ToString());
            InstantActionConfigMenu.instance?.OnUpdateBotNumber();
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, out int parsed) ? parsed : 0;
        }

        private static T GetField<T>(object instance, string name)
        {
            if (instance == null)
                return default;
            var field = instance.GetType().GetField(name, InstanceFlags);
            return field == null ? default : (T)field.GetValue(instance);
        }

        private static T GetProperty<T>(object instance, string name)
        {
            if (instance == null)
                return default;
            var property = instance.GetType().GetProperty(name, InstanceFlags);
            return property == null ? default : (T)property.GetValue(instance, null);
        }

        private static void SetProperty(object instance, string name, object value)
        {
            if (instance == null)
                return;
            var property = instance.GetType().GetProperty(name, InstanceFlags);
            property?.SetValue(instance, value, null);
        }
    }
}
