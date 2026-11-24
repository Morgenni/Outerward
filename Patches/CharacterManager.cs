using HarmonyLib;

namespace Outerward
{
    [HarmonyPatch(typeof(CharacterManager), "CharacterVisualsPresets",MethodType.Getter)]
    public static class CharacterManager_CharacterVisualsPresets
    {
        [HarmonyPostfix]
        public static void Postfix(CharacterManager __instance)
        {
            if (!OuterwardPlugin.ResourceLoaded)
                OuterwardPlugin.InitializeDefaultRaces();
        }
    }
    public static class CharacterVisualPresetsExtensions
    {
        public static int GetTotalRaces(this CharacterVisualsPresets __instance) => ModdedCharacterVisualData.RacesByIndex.Count;
    }
}
