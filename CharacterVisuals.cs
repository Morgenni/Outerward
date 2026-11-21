using HarmonyLib;
using UnityEngine;

namespace Outerward
{
    public static class CharacterVisualsExtensions
    {
        static Material MakeTransparent(Material matSource)
        {
            Material mat = new Material(matSource);
            mat.SetInt("_Mode", 3);
            mat.SetInt("_SrcBlend", 5);
            mat.SetInt("_DstBlend", 10);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 0;
            mat.color = new Color(0, 0, 0, 0);
            //I have no idea what these keywords actually do, google told me to add & remove them but removing opaque was the one that made everything work
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_OPAQUE");
            //mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            return mat;
        }
        public static bool RemoveSkin(this CharacterVisuals __instance, SkinnedMeshRenderer renderer)
        {
            int? skinMat = null;
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                if (materials[i].name.Contains("_skin"))
                {
                    skinMat = i;
                    materials[i] = MakeTransparent(materials[i]);
                    break;
                }
            renderer.sharedMaterials = materials;
            return skinMat != null;
        }
        public static void ApplySkinMaterial(this CharacterVisuals __instance, EquipmentSlot.EquipmentSlotIDs slot, SkinnedMeshRenderer renderer)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                switch (slot)
                {
                    case EquipmentSlot.EquipmentSlotIDs.Chest:
                        if (race.ForceDefaultBody)
                            __instance.RemoveSkin(renderer);
                        else
                            __instance.ApplySkinMaterial(renderer);
                        break;
                    case EquipmentSlot.EquipmentSlotIDs.Foot:
                        if (race.ForceDefaultFeet)
                            __instance.RemoveSkin(renderer);
                        else
                            __instance.ApplySkinMaterial(renderer);
                        break;
                    default:
                        break;
                }
            }
            else
                __instance.ApplySkinMaterial(renderer);
        }
        public static bool ApplyDefaultSkins(this CharacterVisuals __instance, SkinnedMeshRenderer renderer)
        {
            bool raceExists = false;
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                Material[] mats = renderer.sharedMaterials;
                Material raceSkin = race.GetRaceMaterial(moddedData.Gender);
                if (raceSkin != null)
                    raceSkin = new(raceSkin);
                for (int i = 0; i < mats.Length; i++)
                    if (mats[i].name.Contains("_skin"))
                    {
                        raceExists = true;
                        raceSkin.name = mats[i].name;
                        mats[i] = raceSkin;
                    }
                renderer.sharedMaterials = mats;
            }
            return raceExists;
        }
        public static void LoadCharacterCreationHair(this CharacterVisuals __instance, string raceId, int _gender, int _hairStyleIndex, int _hairColorIndex)
        {
            string previewId = $"previewVisualHair_{_gender}_{raceId}_{_hairStyleIndex}";
            RaceData race = ModdedCharacterVisualData.GetRace(raceId);
            if (race == null)
            {
                __instance.LoadCharacterCreationHair(_hairStyleIndex, _hairColorIndex);
                return;
            }
            if (__instance.m_armorVisualPreview.ContainsKey(previewId))
            {
                if (!__instance.m_armorVisualPreview[previewId].gameObject.activeSelf)
                    __instance.m_armorVisualPreview[previewId].gameObject.SetActive(true);
                if (_hairStyleIndex != 0)//not bald
                {
                    Material material = race.GetPart<Material>(RaceData.BodyPart.HairColor, _gender, _hairColorIndex);
                    __instance.m_armorVisualPreview[previewId].Renderer.material = material;
                }
                return;
            }
            else
            {
                __instance.UseDefaultVisuals = true;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Hair, _gender, _hairStyleIndex).transform, null);
                Material material = race.GetPart<Material>(RaceData.BodyPart.HairColor, _gender, _hairColorIndex);
                __instance.DefaultHairVisuals = transform.GetComponent<ArmorVisuals>();
                __instance.DefaultHairVisuals.ApplyToCharacterVisuals(__instance);
                if (!__instance.DefaultHairVisuals.gameObject.activeSelf)
                    __instance.DefaultHairVisuals.gameObject.SetActive(true);
                if (_hairStyleIndex != 0)//not bald
                    __instance.DefaultHairVisuals.Renderer.material = material;
                __instance.FinalizeSkinnedRenderer(__instance.DefaultHairVisuals.Renderer);
                __instance.m_armorVisualPreview.Add(previewId, __instance.DefaultHairVisuals);
            }
        }
        public static void LoadCharacterCreationHead(this CharacterVisuals __instance, string raceId, int _gender, int _headVariationIndex)
        {
            string previewId = $"previewVisualHead_{_gender}_{raceId}_{_headVariationIndex}";
            RaceData race = ModdedCharacterVisualData.GetRace(raceId);
            if (race == null)
            {
                __instance.LoadCharacterCreationHead(0,_gender, _headVariationIndex);
                return;
            }
            if (__instance.m_armorVisualPreview.ContainsKey(previewId))
            {
                if (!__instance.m_armorVisualPreview[previewId].gameObject.activeSelf)
                    __instance.m_armorVisualPreview[previewId].gameObject.SetActive(true);
            }
            else
            {
                __instance.UseDefaultVisuals = true;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Head, _gender, _headVariationIndex).transform, null);
                transform.SetParent(__instance.transform, false);
                transform.ResetLocal(true);
                __instance.DefaultHeadVisuals = transform.GetComponent<ArmorVisuals>();
                __instance.DefaultHeadVisuals.ApplyToCharacterVisuals(__instance);
                __instance.DefaultHeadVisuals.gameObject.SetActive(true);
                __instance.FinalizeSkinnedRenderer(__instance.DefaultHeadVisuals.Renderer);
                __instance.m_armorVisualPreview.Add(previewId, __instance.DefaultHeadVisuals);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterVisuals))]
    public static class CharacterVisualsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("VisualPresets",MethodType.Getter)]
        public static bool GetVisualPresets(CharacterVisuals __instance, ref CharacterVisualsPresets __result)
        {
            if (__instance.m_visualsPresets == null && __instance.UseDefaultVisuals)
            {
                __instance.m_visualsPresets = CharacterManager.CharacterVisualsPresets;
                if (__instance.VisualData is ModdedCharacterVisualData mcvd)
                {
                    RaceData race = ModdedCharacterVisualData.GetRace(mcvd.RaceGUID);
                    if(race != null)
                        __instance.m_skinMat = race.GetRaceMaterial(mcvd.Gender);
                }                
                else
                    __instance.m_skinMat = ((__instance.VisualData.Gender == Character.Gender.Male) ?
                        __instance.m_visualsPresets.MSkins[__instance.VisualData.SkinIndex] :
                        __instance.m_visualsPresets.FSkins[__instance.VisualData.SkinIndex]);
            }
            __result = __instance.m_visualsPresets;
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultHead")]
        public static bool LoadDefaultHead(CharacterVisuals __instance)
        {
            if(__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                if (race == null)
                    return false;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Head, moddedData.Gender, moddedData.HeadVariationIndex).transform);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    __instance.DefaultHeadVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultHeadVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultHeadVisuals.gameObject.SetActive(true);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultHeadVisuals.Renderer);
                    if (__instance.Head)
                        __instance.Head.gameObject.SetActive(false);
                    __instance.Head = __instance.m_defaultHeadVisuals.transform;
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultBody")]
        public static bool LoadDefaultBody(CharacterVisuals __instance)
        {
            if(__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                if (race == null)
                    return false;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Body, moddedData.Gender).transform, null);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    //TODO: handle armorvisuals for non-outward assets later
                    __instance.DefaultBodyVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultBodyVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultBodyVisuals.gameObject.SetActive(true);
                    __instance.ApplySkinMaterial(EquipmentSlot.EquipmentSlotIDs.Chest,__instance.DefaultBodyVisuals.Renderer);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultBodyVisuals.Renderer);
                    if (!Application.isPlaying)
                        __instance.EditorDefaultVisuals[1] = __instance.m_defaultBodyVisuals;
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultBoots")]
        public static bool LoadDefaultBoots(CharacterVisuals __instance)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                if (race == null)
                    return false;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Feet, moddedData.Gender).transform, null);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    __instance.DefaultFootVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultFootVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultFootVisuals.gameObject.SetActive(true);
                    __instance.ApplySkinMaterial(EquipmentSlot.EquipmentSlotIDs.Foot,__instance.DefaultFootVisuals.Renderer);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultFootVisuals.Renderer);
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("ApplySkinMaterial")]
        public static bool ApplySkinMaterial(CharacterVisuals __instance, SkinnedMeshRenderer _renderer)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                if (race == null)
                    return false;
                __instance.ApplyDefaultSkins(_renderer);
            }
            return false;
        }
        [HarmonyPostfix]
        [HarmonyPatch("EquipVisuals")]
        public static void EquipVisuals(CharacterVisuals __instance, EquipmentSlot.EquipmentSlotIDs _slotID, ArmorVisuals _visuals)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData && __instance.UseDefaultVisuals)
                __instance.ApplySkinMaterial(_slotID,_visuals.Renderer);
        }
        [HarmonyPrefix]
        [HarmonyPatch("ActivateDefaultVisuals")]
        public static bool ActivateDefaultVisuals(CharacterVisuals __instance, EquipmentSlot.EquipmentSlotIDs _slotID, bool _activate)
        {
            ArmorVisuals armorVisuals = null;
            RaceData race=null;
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
                race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
            if (race == null)
                return false;
            switch (_slotID)
            {
                case EquipmentSlot.EquipmentSlotIDs.Helmet:
                    if (_activate)
                    {
                        if (__instance.DefaultHelmVisuals == null)
                        {
                            __instance.LoadDefaultHelm();
                            if (__instance.DefaultHelmVisuals)
                                __instance.m_visualsSkinnedMeshCenterSize[1] = __instance.DefaultHelmVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                        }
                        else
                            __instance.DefaultHelmVisuals.gameObject.SetActive(true);
                    }
                    else if (!_activate && __instance.DefaultHelmVisuals != null)
                        __instance.DefaultHelmVisuals.gameObject.SetActive(race.ForceDefaultHead);
                    break;
                case EquipmentSlot.EquipmentSlotIDs.Chest:
                    if (_activate)
                    {
                        if (__instance.DefaultBodyVisuals == null)
                        {
                            __instance.LoadDefaultBody();
                            __instance.m_visualsSkinnedMeshCenterSize[2] = __instance.DefaultBodyVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                        }
                        else
                            __instance.DefaultBodyVisuals.gameObject.SetActive(true);
                    }
                    else if (!_activate && __instance.DefaultBodyVisuals != null)
                        __instance.DefaultBodyVisuals.gameObject.SetActive(race.ForceDefaultBody);
                    break;
                case EquipmentSlot.EquipmentSlotIDs.Foot:
                    if (_activate && __instance.DefaultFootVisuals == null)
                    {
                        __instance.LoadDefaultBoots();
                        __instance.m_visualsSkinnedMeshCenterSize[3] = __instance.DefaultFootVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                    }
                    else if (!_activate && __instance.DefaultFootVisuals != null)
                    {
                        armorVisuals = __instance.DefaultFootVisuals;
                        armorVisuals.gameObject.SetActive(race.ForceDefaultFeet);
                        __instance.DefaultFootVisuals = null;
                    }
                    break;
            }
            __instance.SetSkinnedMeshRendersLarge(__instance.m_largeBounds);
            if (armorVisuals!=null && !_activate)
            {
                armorVisuals.ReclaimVisuals();
                CharacterManager.PutBackDefaultVisuals(armorVisuals);
            }
            return false;
        }
        [HarmonyPatch("InitDefaultVisuals")]
        [HarmonyPrefix]
        public static void InitDefaultVisuals(CharacterVisuals __instance)
        {
            __instance.m_character.VisualData = new ModdedCharacterVisualData(__instance.m_character.VisualData);
        }
    }
}
