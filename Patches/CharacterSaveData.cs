using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Outerward
{
    [HarmonyPatch(typeof(CharacterSaveData))]
    public static class CharacterSaveDataPatch
    {
        [HarmonyPatch(MethodType.Constructor, argumentTypes: new[] {typeof(Character)})]
        public static void Constructor(CharacterSaveData __instance, Character _char)
        {
            ModdedCharacterVisualData mData;
            if (_char.VisualData.GetType() == typeof(ModdedCharacterVisualData))
                mData = new(_char.VisualData as ModdedCharacterVisualData);
            else
                mData = ModdedCharacterVisualData.FromVanillaData(_char.VisualData);
            __instance.VisualData = mData;
        }
    }
}
