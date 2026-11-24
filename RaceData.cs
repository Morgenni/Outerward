using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Outerward
{
    public class RaceData
    {
        string GUID;
        public enum BodyPart
        {
            Head,
            Body,
            Feet,
            Hair,
            HairColor
        }
        internal Dictionary<BodyPart, Dictionary<Character.Gender, List<object>>> visualsLookup = [];
        public string Name
        {
            get
            {
                if (LocalizationManager.m_instance.m_generalLocalization.TryGetValue(GUID, out string value))
                    return value;
                return null;
            }
            set
            {
                if (LocalizationManager.m_instance.m_generalLocalization.TryGetValue(GUID, out _))
                    LocalizationManager.m_instance.m_generalLocalization[GUID] = value;
                LocalizationManager.m_instance.m_generalLocalization.Add(GUID, value);
            }
        }
        public bool AllowDefaultHairs = true;
        public bool ForceDefaultHead = false;
        public bool ForceDefaultBody = false;
        public bool ForceDefaultFeet = false;
        public bool UseRaceMaterial = true;
        public bool GenderHair = false;
        public Material GetRaceMaterial(Character.Gender gender)
        {
            if (RaceMaterial != null)
            {
                if (RaceMaterial.Length > 1)
                    return RaceMaterial[(int)gender];
                else
                    return RaceMaterial[0];
            }
            return null;
        }
        public RaceData(string guid)
        {
            GUID = guid;
        }
        public Material[] RaceMaterial = null;
        public List<GameObject> GetParts(BodyPart part, Character.Gender gender) => GetParts<GameObject>(part, gender);
        public List<T> GetParts<T>(BodyPart part, Character.Gender gender)
        {
            if (visualsLookup.TryGetValue(part, out var bodyPartList))
            {
                if (!bodyPartList.ContainsKey(gender) && gender == Character.Gender.Female)
                    gender = Character.Gender.Male;
                if (bodyPartList.TryGetValue(gender, out List<object> list))
                    return list.Cast<T>().ToList();
            }
            return null;
        }
        public GameObject GetPart(BodyPart part) => GetPart(part, Character.Gender.Male, 0);
        public GameObject GetPart(BodyPart part, int gender, int index) => GetPart(part, gender == 0 ? Character.Gender.Male : Character.Gender.Female, index);
        public GameObject GetPart(BodyPart part, Character.Gender gender) => GetPart(part, gender, 0);
        public GameObject GetPart(BodyPart part, Character.Gender gender, int index) => GetPart<GameObject>(part, gender, index);
        public T GetPart<T>(BodyPart part, int gender, int index) => GetPart<T>(part, gender == 0 ? Character.Gender.Male : Character.Gender.Female, index);
        public T GetPart<T>(BodyPart part, Character.Gender gender, int index)
        {
            List<T> list = GetParts<T>(part, gender);
            if (list != null)
                return list[index];
            object returnee = null;
            switch (part)
            {
                case BodyPart.Head:
                    if (gender == Character.Gender.Female)
                        returnee = CharacterManager.m_characterVisualsPresets.FHeadsWhite[index];
                    else
                        returnee = CharacterManager.m_characterVisualsPresets.MHeadsWhite[index];
                    break;
                case BodyPart.Body:
                    returnee = CharacterManager.m_characterVisualsPresets.Bodies[(int)gender];
                    break;
                case BodyPart.Feet:
                    returnee = CharacterManager.m_characterVisualsPresets.Feet[(int)gender];
                    break;
                case BodyPart.Hair:
                    returnee = CharacterManager.m_characterVisualsPresets.Hairs[index];
                    break;
                case BodyPart.HairColor:
                    returnee = CharacterManager.m_characterVisualsPresets.HairMaterials[index];
                    break;
                default:
                    break;
            }
            return (T)returnee;
        }
        public void SetRaceMaterial(Material skin) => SetRaceMaterial([skin]);
        public void SetRaceMaterial(Material[] skins)
        {
            for (int i = 0; i< skins.Length; i++)
            {
                Material mat = new Material(skins[i]);
                if (!mat.name.Contains("_skin"))
                    mat.name = mat.name + "_skin";
                skins[i] = mat;
            }
            RaceMaterial = skins;
        }
        public void AddHairMaterial(Material material) => AddHairMaterial(material, Character.Gender.Male);
        public void AddHairMaterial(Material material, Character.Gender gender)
        {
            if (!visualsLookup.ContainsKey(BodyPart.HairColor))
                visualsLookup.Add(BodyPart.HairColor, new Dictionary<Character.Gender, List<object>>());
            if (!visualsLookup[BodyPart.HairColor].ContainsKey(gender))
            {
                if (!GenderHair)
                    gender = Character.Gender.Male;
                visualsLookup[BodyPart.HairColor].Add(gender, new List<object>());
                if (AllowDefaultHairs)
                    foreach (Material mat in CharacterManager.m_characterVisualsPresets.HairMaterials)
                        visualsLookup[BodyPart.HairColor][gender].Add(mat);
            }
            if (material != null)
                visualsLookup[BodyPart.HairColor][gender].Add(material);
        }
        public void AddBodyPart(BodyPart part, GameObject gameObject, Character.Gender gender)
        {
            if (!visualsLookup.ContainsKey(part))
                visualsLookup.Add(part, new Dictionary<Character.Gender, List<object>>());
            if (!visualsLookup[part].ContainsKey(gender))
            {
                if (part == BodyPart.Hair)
                {
                    if (!GenderHair)
                        gender = Character.Gender.Male;
                    visualsLookup[part].Add(gender, new List<object>());
                    visualsLookup[part][gender].Add(new GameObject());//bald
                    if (AllowDefaultHairs)
                        foreach (GameObject go in CharacterManager.m_characterVisualsPresets.Hairs)
                            visualsLookup[part][gender].Add(go);
                }
                else
                    visualsLookup[part].Add(gender, new List<object>());
            }
            if (gameObject != null)
                visualsLookup[part][gender].Add(gameObject);
        }
        public void AddBodyPart(BodyPart part, GameObject gameObject) => AddBodyPart(part, gameObject, Character.Gender.Male);
    }
}
