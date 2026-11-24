using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Outerward
{
    public static class SaveManagerExtension
    {
        public static CharacterSave CreateNewCharacterSave(this SaveManager __instance, bool isModded)
        {
            if (!isModded)
                return __instance.CreateNewCharacterSave();
            return new CharacterSave
            {
                PSave = new PlayerSaveData()
                {
                    UID = Global.GenerateUID(),
                    VisualData=new ModdedCharacterVisualData()
                }
            };
        }
    }
}
