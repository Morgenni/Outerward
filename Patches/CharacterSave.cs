using HarmonyLib;
using System;
using System.Xml.Serialization;

namespace Outerward
{
    [HarmonyPatch(typeof(CharacterSave))]
    public static class CharacterSavePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("PrepareSave")]
        public static void PrepareSave(CharacterSave __instance)
        {
            if(__instance.PSave.VisualData.GetType() == typeof(CharacterVisualData))
            {
                Character character = CharacterManager.Instance.GetCharacter(__instance.PSave.UID);
                CharacterVisualData cvd = __instance.PSave.VisualData;
                ModdedCharacterVisualData mcvd = new ModdedCharacterVisualData(character.VisualData);
                __instance.PSave.VisualData = mcvd;
            }
        }
    }
}
