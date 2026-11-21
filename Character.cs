using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Outerward
{
    [HarmonyPatch(typeof(Character))]
    public static class CharacterPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("LoadCharSave")]
        public static void LoadCharSave(Character __instance, CharacterSaveData _save)
        {
            if (_save.VisualData.GetType() == typeof(CharacterVisualData))
                _save.VisualData=ModdedCharacterVisualData.FromVanillaData(_save.VisualData);
            else
                _save.VisualData = new ModdedCharacterVisualData(_save.VisualData);
        }
        [HarmonyPatch("LoadCharSave")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoadCharSave_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo oldConstructor = AccessTools.Constructor(typeof(CharacterVisualData), new Type[1] { typeof(CharacterVisualData) });
            foreach (CodeInstruction ci in instructions)
                if (ci.opcode == OpCodes.Newobj && Equals(oldConstructor, ci.operand))
                {
                    ci.operand = AccessTools.Constructor(typeof(ModdedCharacterVisualData), new Type[1] { typeof(CharacterVisualData) }); ;
                    break;
                }
            return instructions;
        }
    }
}
