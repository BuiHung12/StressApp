using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Procedural character visuals generator and synchronizer (3D Toy style).
    /// </summary>
    public static class CharacterVisuals
    {
        public static GameObject CreateCharacterTopDown(string charName, Color bodyColor, Color skinColor)
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                return new GameObject(charName);
            }
            var character = new GameObject(charName);

            // === NECK ===
            var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            neck.transform.SetParent(character.transform);
            neck.transform.localPosition = new Vector3(0, 1.1f, 0);
            neck.transform.localScale = new Vector3(0.13f, 0.08f, 0.13f);
            neck.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(neck.GetComponent<Collider>());

            // === HEAD (sphere) ===
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(character.transform);
            head.transform.localPosition = new Vector3(0, 1.38f, 0);
            head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            head.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(head.GetComponent<Collider>());

            // === NOSE ===
            var nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform, false);
            nose.transform.localPosition = new Vector3(0f, -0.05f, 0.52f);
            nose.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            nose.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(nose.GetComponent<Collider>());

            // === MOUTH ===
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.name = "Mouth";
            mouth.transform.SetParent(head.transform, false);
            mouth.transform.localPosition = new Vector3(0f, -0.2f, 0.47f);
            mouth.transform.localScale = new Vector3(0.12f, 0.04f, 0.08f);
            mouth.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.1f, 0.1f));
            Destroy(mouth.GetComponent<Collider>());

            var tongue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tongue.name = "Tongue";
            tongue.transform.SetParent(mouth.transform, false);
            tongue.transform.localPosition = new Vector3(0f, -0.3f, 0.2f);
            tongue.transform.localScale = new Vector3(0.8f, 0.6f, 0.5f);
            tongue.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.45f, 0.5f));
            Destroy(tongue.GetComponent<Collider>());

            // === EARS ===
            var earL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earL.name = "EarL";
            earL.transform.SetParent(head.transform, false);
            earL.transform.localPosition = new Vector3(-0.52f, 0f, 0f);
            earL.transform.localScale = new Vector3(0.12f, 0.16f, 0.1f);
            earL.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(earL.GetComponent<Collider>());

            var earR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earR.name = "EarR";
            earR.transform.SetParent(head.transform, false);
            earR.transform.localPosition = new Vector3(0.52f, 0f, 0f);
            earR.transform.localScale = new Vector3(0.12f, 0.16f, 0.1f);
            earR.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(earR.GetComponent<Collider>());

            // === EYES ===
            var eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "EyeL";
            eyeL.transform.SetParent(head.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.2f, 0.06f, 0.44f);
            eyeL.transform.localScale = new Vector3(0.12f, 0.16f, 0.08f);
            eyeL.GetComponent<Renderer>().material = CreateMat(Color.white);
            Destroy(eyeL.GetComponent<Collider>());

            var pupilL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupilL.name = "PupilL";
            pupilL.transform.SetParent(eyeL.transform, false);
            pupilL.transform.localPosition = new Vector3(0f, 0f, 0.45f);
            pupilL.transform.localScale = new Vector3(0.55f, 0.55f, 0.3f);
            pupilL.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.1f, 0.12f));
            Destroy(pupilL.GetComponent<Collider>());

            var shinyL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shinyL.name = "ShinyL";
            shinyL.transform.SetParent(pupilL.transform, false);
            shinyL.transform.localPosition = new Vector3(-0.25f, 0.25f, 0.45f);
            shinyL.transform.localScale = new Vector3(0.35f, 0.35f, 0.2f);
            shinyL.GetComponent<Renderer>().material = CreateMat(Color.white);
            Destroy(shinyL.GetComponent<Collider>());

            var eyeR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeR.name = "EyeR";
            eyeR.transform.SetParent(head.transform, false);
            eyeR.transform.localPosition = new Vector3(0.2f, 0.06f, 0.44f);
            eyeR.transform.localScale = new Vector3(0.12f, 0.16f, 0.08f);
            eyeR.GetComponent<Renderer>().material = CreateMat(Color.white);
            Destroy(eyeR.GetComponent<Collider>());

            var pupilR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupilR.name = "PupilR";
            pupilR.transform.SetParent(eyeR.transform, false);
            pupilR.transform.localPosition = new Vector3(0f, 0f, 0.45f);
            pupilR.transform.localScale = new Vector3(0.55f, 0.55f, 0.3f);
            pupilR.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.1f, 0.12f));
            Destroy(pupilR.GetComponent<Collider>());

            var shinyR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shinyR.name = "ShinyR";
            shinyR.transform.SetParent(pupilR.transform, false);
            shinyR.transform.localPosition = new Vector3(-0.25f, 0.25f, 0.45f);
            shinyR.transform.localScale = new Vector3(0.35f, 0.35f, 0.2f);
            shinyR.GetComponent<Renderer>().material = CreateMat(Color.white);
            Destroy(shinyR.GetComponent<Collider>());

            // === EYEBROWS ===
            var browL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            browL.name = "BrowL";
            browL.transform.SetParent(head.transform, false);
            browL.transform.localPosition = new Vector3(-0.2f, 0.2f, 0.46f);
            browL.transform.localScale = new Vector3(0.14f, 0.025f, 0.04f);
            browL.transform.localRotation = Quaternion.Euler(0, 0, 5f);
            browL.GetComponent<Renderer>().material = CreateMat(new Color(0.12f, 0.08f, 0.06f));
            Destroy(browL.GetComponent<Collider>());

            var browR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            browR.name = "BrowR";
            browR.transform.SetParent(head.transform, false);
            browR.transform.localPosition = new Vector3(0.2f, 0.2f, 0.46f);
            browR.transform.localScale = new Vector3(0.14f, 0.025f, 0.04f);
            browR.transform.localRotation = Quaternion.Euler(0, 0, -5f);
            browR.GetComponent<Renderer>().material = CreateMat(new Color(0.12f, 0.08f, 0.06f));
            Destroy(browR.GetComponent<Collider>());

            // === CHEEKS ===
            var cheekL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cheekL.name = "CheekL";
            cheekL.transform.SetParent(head.transform, false);
            cheekL.transform.localPosition = new Vector3(-0.25f, -0.06f, 0.4f);
            cheekL.transform.localScale = new Vector3(0.08f, 0.04f, 0.04f);
            cheekL.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.5f, 0.5f, 0.6f));
            Destroy(cheekL.GetComponent<Collider>());

            var cheekR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cheekR.name = "CheekR";
            cheekR.transform.SetParent(head.transform, false);
            cheekR.transform.localPosition = new Vector3(0.25f, -0.06f, 0.4f);
            cheekR.transform.localScale = new Vector3(0.08f, 0.04f, 0.04f);
            cheekR.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.5f, 0.5f, 0.6f));
            Destroy(cheekR.GetComponent<Collider>());

            // === HAIR CONTAINER ===
            var hairContainer = new GameObject("HairContainer");
            hairContainer.transform.SetParent(character.transform, false);

            // === TORSO CONTAINER ===
            var torsoContainer = new GameObject("TorsoContainer");
            torsoContainer.transform.SetParent(character.transform, false);

            // === BELT ===
            var belt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            belt.name = "Belt";
            belt.transform.SetParent(character.transform, false);
            belt.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            belt.transform.localScale = new Vector3(0.38f, 0.02f, 0.24f);
            belt.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.15f, 0.15f));
            Destroy(belt.GetComponent<Collider>());

            var buckle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buckle.name = "Buckle";
            buckle.transform.SetParent(belt.transform, false);
            buckle.transform.localPosition = new Vector3(0f, 0f, 0.52f);
            buckle.transform.localScale = new Vector3(0.2f, 1.2f, 0.1f);
            buckle.GetComponent<Renderer>().material = CreateMat(new Color(0.9f, 0.75f, 0.2f));
            Destroy(buckle.GetComponent<Collider>());

            // === LEFT ARM ===
            var leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(character.transform);
            leftArm.transform.localPosition = new Vector3(-0.28f, 0.72f, 0);
            leftArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            leftArm.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(leftArm.GetComponent<Collider>());

            var sleeveL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sleeveL.name = "Sleeve";
            sleeveL.transform.SetParent(leftArm.transform, false);
            sleeveL.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            sleeveL.transform.localScale = new Vector3(1.15f, 0.45f, 1.15f);
            sleeveL.GetComponent<Renderer>().material = CreateMat(bodyColor);
            Destroy(sleeveL.GetComponent<Collider>());

            // === LEFT HAND ===
            var handL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handL.name = "HandL";
            handL.transform.SetParent(leftArm.transform, false);
            handL.transform.localPosition = new Vector3(0f, -1.05f, 0f);
            handL.transform.localScale = new Vector3(0.9f, 0.36f, 0.9f);
            handL.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(handL.GetComponent<Collider>());

            // === RIGHT ARM ===
            var rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(character.transform);
            rightArm.transform.localPosition = new Vector3(0.28f, 0.72f, 0);
            rightArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            rightArm.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(rightArm.GetComponent<Collider>());

            var sleeveR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sleeveR.name = "Sleeve";
            sleeveR.transform.SetParent(rightArm.transform, false);
            sleeveR.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            sleeveR.transform.localScale = new Vector3(1.15f, 0.45f, 1.15f);
            sleeveR.GetComponent<Renderer>().material = CreateMat(bodyColor);
            Destroy(sleeveR.GetComponent<Collider>());

            // === RIGHT HAND ===
            var handR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handR.name = "HandR";
            handR.transform.SetParent(rightArm.transform, false);
            handR.transform.localPosition = new Vector3(0f, -1.05f, 0f);
            handR.transform.localScale = new Vector3(0.9f, 0.36f, 0.9f);
            handR.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(handR.GetComponent<Collider>());

            // === LEGS CONTAINER (populated by ApplyPants) ===
            var legsContainer = new GameObject("LegsContainer");
            legsContainer.transform.SetParent(character.transform, false);

            // === SHADOW ===
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "Shadow";
            shadow.transform.SetParent(character.transform);
            shadow.transform.localPosition = new Vector3(0.05f, 0.005f, -0.05f);
            shadow.transform.localScale = new Vector3(0.5f, 0.005f, 0.5f);
            shadow.GetComponent<Renderer>().material = CreateMat(new Color(0, 0, 0, 0.2f));
            Destroy(shadow.GetComponent<Collider>());

            character.transform.localScale = Vector3.one * 0.45f;
            character.AddComponent<CharacterAnimator>();

            // Default customization
            ApplyCustomization(character, 0, 0, new Color(0.18f, 0.12f, 0.08f), 0, bodyColor, 0, new Color(0.25f, 0.35f, 0.55f));

            return character;
        }

        public static void ApplyCustomization(GameObject character, int gender, int hairStyle, Color hairColor, int outfitStyle, Color bodyColor, int pantsStyle, Color pantsColor)
        {
            if (character == null) return;
            if (NetworkSetup.IsHeadlessServer()) return;
            Debug.Log($"[ApplyCustomization] Character: {character.name}, Gender: {gender}, HairStyle: {hairStyle}, OutfitStyle: {outfitStyle}, PantsStyle: {pantsStyle}");

            // Check if this character has custom prefab models inside
            var maleModel = character.transform.Find("MaleModel");
            var femaleModel = character.transform.Find("FemaleModel");
            if (maleModel != null || femaleModel != null)
            {
                if (maleModel != null) maleModel.gameObject.SetActive(gender == 0);
                if (femaleModel != null) femaleModel.gameObject.SetActive(gender != 0);

                var activeModel = (gender == 0 && maleModel != null) ? maleModel.gameObject : (femaleModel != null ? femaleModel.gameObject : null);
                if (activeModel != null)
                {
                    ColorizePrefabMaterials(activeModel, bodyColor);
                }
                return;
            }

            ApplyHair(character, gender, hairStyle, hairColor);
            ApplyOutfit(character, gender, outfitStyle, bodyColor);
            ApplyPants(character, gender, pantsStyle, pantsColor);
        }

        private static void ColorizePrefabMaterials(GameObject obj, Color bodyColor)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.sharedMaterials != null)
                {
                    var mats = r.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        string matName = mats[i].name.ToLower();
                        string objName = r.gameObject.name.ToLower();
                        if (matName.Contains("cloth") || matName.Contains("shirt") || matName.Contains("body") || matName.Contains("pant") ||
                            objName.Contains("cloth") || objName.Contains("shirt") || objName.Contains("body") || objName.Contains("pant"))
                        {
                            mats[i].color = bodyColor;
                        }
                    }
                    r.materials = mats;
                }
            }
        }

        public static void ApplyCustomization(GameObject character, int hairStyle, Color hairColor, int outfitStyle, Color bodyColor, int pantsStyle, Color pantsColor)
        {
            ApplyCustomization(character, 0, hairStyle, hairColor, outfitStyle, bodyColor, pantsStyle, pantsColor);
        }

        private static void ApplyHair(GameObject character, int gender, int style, Color color)
        {
            var container = character.transform.Find("HairContainer");
            if (container == null)
            {
                var go = new GameObject("HairContainer");
                go.transform.SetParent(character.transform, false);
                container = go.transform;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
                Object.Destroy(container.GetChild(i).gameObject);

            style = Mathf.Clamp(style, 0, 5);

            if (gender == 0) // NAM
            {
                switch (style)
                {
                    case 0:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.42f, 0.25f, 0.42f), color);
                        AddPrimChild(container, "HairFringe", PrimitiveType.Sphere,
                            new Vector3(0, 1.42f, 0.18f), new Vector3(0.35f, 0.12f, 0.15f), color);
                        break;

                    case 1:
                        AddPrimChild(container, "MohawkBase", PrimitiveType.Cube,
                            new Vector3(0, 1.55f, 0f), new Vector3(0.06f, 0.25f, 0.35f), color);
                        AddPrimChild(container, "MohawkTip", PrimitiveType.Sphere,
                            new Vector3(0, 1.7f, -0.05f), new Vector3(0.08f, 0.12f, 0.2f), color);
                        AddPrimChild(container, "MohawkSideL", PrimitiveType.Sphere,
                            new Vector3(-0.2f, 1.38f, 0f), new Vector3(0.08f, 0.12f, 0.3f), color * 0.7f);
                        AddPrimChild(container, "MohawkSideR", PrimitiveType.Sphere,
                            new Vector3(0.2f, 1.38f, 0f), new Vector3(0.08f, 0.12f, 0.3f), color * 0.7f);
                        break;

                    case 2:
                        AddPrimChild(container, "AfroMain", PrimitiveType.Sphere,
                            new Vector3(0, 1.52f, 0f), new Vector3(0.65f, 0.55f, 0.6f), color);
                        AddPrimChild(container, "AfroFront", PrimitiveType.Sphere,
                            new Vector3(0, 1.45f, 0.15f), new Vector3(0.5f, 0.35f, 0.25f), color * 1.1f);
                        break;

                    case 3:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.46f, -0.02f), new Vector3(0.42f, 0.23f, 0.42f), color);
                        AddPrimChild(container, "Spike1", PrimitiveType.Cube,
                            new Vector3(0f, 1.58f, 0.05f), new Vector3(0.08f, 0.15f, 0.08f), color);
                        AddPrimChild(container, "Spike2", PrimitiveType.Cube,
                            new Vector3(-0.09f, 1.54f, 0f), new Vector3(0.08f, 0.15f, 0.08f), color);
                        AddPrimChild(container, "Spike3", PrimitiveType.Cube,
                            new Vector3(0.09f, 1.54f, 0f), new Vector3(0.08f, 0.15f, 0.08f), color);
                        break;

                    case 4:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.42f, 0.25f, 0.42f), color);
                        AddPrimChild(container, "UndercutSwept", PrimitiveType.Cube,
                            new Vector3(0.05f, 1.45f, 0.12f), new Vector3(0.32f, 0.08f, 0.18f), color * 0.9f);
                        break;

                    case 5:
                        // BASEBALL CAP (Mũ lưỡi trai)
                        AddPrimChild(container, "CapBase", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.44f, 0.28f, 0.44f), color);
                        AddPrimChild(container, "CapBrim", PrimitiveType.Cube,
                            new Vector3(0, 1.44f, 0.22f), new Vector3(0.35f, 0.02f, 0.18f), color);
                        break;
                }
            }
            else // NỮ
            {
                switch (style)
                {
                    case 0:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.44f, 0.25f, 0.42f), color);
                        AddPrimChild(container, "Ponytail", PrimitiveType.Capsule,
                            new Vector3(0, 1.25f, -0.2f), new Vector3(0.1f, 0.28f, 0.1f), color);
                        AddPrimChild(container, "PonytailBand", PrimitiveType.Sphere,
                            new Vector3(0, 1.38f, -0.18f), new Vector3(0.12f, 0.06f, 0.12f), color * 0.6f);
                        break;

                    case 1:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.5f, -0.02f), new Vector3(0.48f, 0.28f, 0.46f), color);
                        AddPrimChild(container, "HairBackL", PrimitiveType.Capsule,
                            new Vector3(-0.12f, 1.15f, -0.1f), new Vector3(0.18f, 0.35f, 0.15f), color);
                        AddPrimChild(container, "HairBackR", PrimitiveType.Capsule,
                            new Vector3(0.12f, 1.15f, -0.1f), new Vector3(0.18f, 0.35f, 0.15f), color);
                        AddPrimChild(container, "HairFringe", PrimitiveType.Sphere,
                            new Vector3(0, 1.42f, 0.2f), new Vector3(0.38f, 0.12f, 0.12f), color);
                        break;

                    case 2:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.44f, 0.25f, 0.42f), color);
                        AddPrimChild(container, "Bun", PrimitiveType.Sphere,
                            new Vector3(0f, 1.62f, -0.08f), new Vector3(0.2f, 0.2f, 0.2f), color);
                        break;

                    case 3:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.46f, 0.26f, 0.44f), color);
                        AddPrimChild(container, "BobL", PrimitiveType.Sphere,
                            new Vector3(-0.16f, 1.3f, 0.02f), new Vector3(0.18f, 0.25f, 0.18f), color);
                        AddPrimChild(container, "BobR", PrimitiveType.Sphere,
                            new Vector3(0.16f, 1.3f, 0.02f), new Vector3(0.18f, 0.25f, 0.18f), color);
                        break;

                    case 4:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.44f, 0.25f, 0.42f), color);
                        AddPrimChild(container, "BraidL", PrimitiveType.Capsule,
                            new Vector3(-0.16f, 1.1f, 0.06f), new Vector3(0.08f, 0.24f, 0.08f), color);
                        AddPrimChild(container, "BraidR", PrimitiveType.Capsule,
                            new Vector3(0.16f, 1.1f, 0.06f), new Vector3(0.08f, 0.24f, 0.08f), color);
                        break;

                    case 5:
                        // CAT EARS (Tai mèo)
                        var head = character.transform.Find("Head");
                        var skinColor = head != null ? head.GetComponent<Renderer>().material.color : new Color(1f, 0.88f, 0.7f);
                        
                        var catEarL = AddPrimChild(container, "CatEarL", PrimitiveType.Cube,
                            new Vector3(-0.13f, 1.57f, 0f), new Vector3(0.07f, 0.07f, 0.07f), skinColor);
                        catEarL.transform.localRotation = Quaternion.Euler(0, 0, 45);

                        AddPrimChild(catEarL.transform, "InnerEarL", PrimitiveType.Cube,
                            new Vector3(0f, 0f, 0.45f), new Vector3(0.6f, 0.6f, 0.2f), new Color(1f, 0.6f, 0.6f));

                        var catEarR = AddPrimChild(container, "CatEarR", PrimitiveType.Cube,
                            new Vector3(0.13f, 1.57f, 0f), new Vector3(0.07f, 0.07f, 0.07f), skinColor);
                        catEarR.transform.localRotation = Quaternion.Euler(0, 0, -45);

                        AddPrimChild(catEarR.transform, "InnerEarR", PrimitiveType.Cube,
                            new Vector3(0f, 0f, 0.45f), new Vector3(0.6f, 0.6f, 0.2f), new Color(1f, 0.6f, 0.6f));
                        break;
                }
            }
        }

        private static void ApplyOutfit(GameObject character, int gender, int style, Color color)
        {
            // Hide default renderers on the root or standard direct children to prevent overlap with the new procedural body
            var rootRenderer = character.GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            
            var defaultBody = character.transform.Find("Body");
            if (defaultBody != null && defaultBody.name != "TorsoContainer" && defaultBody.name != "LegsContainer")
            {
                var r = defaultBody.GetComponent<Renderer>();
                if (r != null) r.enabled = false;
            }
            
            var defaultTorso = character.transform.Find("Torso");
            if (defaultTorso != null && defaultTorso.name != "TorsoContainer" && defaultTorso.name != "LegsContainer")
            {
                var r = defaultTorso.GetComponent<Renderer>();
                if (r != null) r.enabled = false;
            }

            var container = character.transform.Find("TorsoContainer");
            if (container == null)
            {
                var go = new GameObject("TorsoContainer");
                go.transform.SetParent(character.transform, false);
                container = go.transform;
            }

            // Set the color of the arm sleeves to match the outfit shirt color!
            var armL = character.transform.Find("LeftArm");
            if (armL != null)
            {
                var sleeveL = armL.Find("Sleeve");
                if (sleeveL != null) sleeveL.GetComponent<Renderer>().material.color = color;
            }
            var armR = character.transform.Find("RightArm");
            if (armR != null)
            {
                var sleeveR = armR.Find("Sleeve");
                if (sleeveR != null) sleeveR.GetComponent<Renderer>().material.color = color;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
                Object.Destroy(container.GetChild(i).gameObject);

            style = Mathf.Clamp(style, 0, 4);

            switch (style)
            {
                case 0:
                    float scaleY = gender == 0 ? 0.45f : 0.35f;
                    float posY = gender == 0 ? 0.7f : 0.78f;
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, posY, 0), new Vector3(0.4f, scaleY, 0.25f), color);
                    break;

                case 1:
                    if (gender == 0)
                    {
                        AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                            new Vector3(0, 0.7f, 0), new Vector3(0.4f, 0.45f, 0.25f), color);
                        AddPrimChild(container, "CollarL", PrimitiveType.Cube,
                            new Vector3(-0.12f, 1.05f, 0.08f), new Vector3(0.1f, 0.12f, 0.06f), color * 0.8f);
                        AddPrimChild(container, "CollarR", PrimitiveType.Cube,
                            new Vector3(0.12f, 1.05f, 0.08f), new Vector3(0.1f, 0.12f, 0.06f), color * 0.8f);
                        AddPrimChild(container, "VestButton", PrimitiveType.Sphere,
                            new Vector3(0, 0.8f, 0.13f), new Vector3(0.04f, 0.04f, 0.02f), Color.white);
                        AddPrimChild(container, "VestButton2", PrimitiveType.Sphere,
                            new Vector3(0, 0.65f, 0.13f), new Vector3(0.04f, 0.04f, 0.02f), Color.white);
                    }
                    else
                    {
                        AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                            new Vector3(0, 0.75f, 0), new Vector3(0.38f, 0.38f, 0.23f), color);
                        AddPrimChild(container, "DressSkirt", PrimitiveType.Cylinder,
                            new Vector3(0, 0.42f, 0), new Vector3(0.42f, 0.2f, 0.42f), color);
                    }
                    break;

                case 2:
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.7f, 0), new Vector3(0.42f, 0.47f, 0.27f), color);
                    AddPrimChild(container, "Hood", PrimitiveType.Sphere,
                        new Vector3(0, 1.18f, -0.12f), new Vector3(0.32f, 0.25f, 0.25f), color * 0.9f);
                    AddPrimChild(container, "HoodiePouch", PrimitiveType.Cube,
                        new Vector3(0, 0.55f, 0.13f), new Vector3(0.2f, 0.1f, 0.04f), color * 0.85f);
                    break;

                case 3:
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.7f, 0), new Vector3(0.36f, 0.43f, 0.22f), color);
                    AddPrimChild(container, "StrapL", PrimitiveType.Cube,
                        new Vector3(-0.12f, 1.0f, 0.02f), new Vector3(0.04f, 0.15f, 0.04f), color);
                    AddPrimChild(container, "StrapR", PrimitiveType.Cube,
                        new Vector3(0.12f, 1.0f, 0.02f), new Vector3(0.04f, 0.15f, 0.04f), color);

                    // ADVENTURE BACKPACK (Balo/Cặp dã ngoại)
                    var backpack = AddPrimChild(container, "Backpack", PrimitiveType.Cube,
                        new Vector3(0f, 0.72f, -0.16f), new Vector3(0.22f, 0.28f, 0.12f), new Color(0.2f, 0.2f, 0.25f));
                    AddPrimChild(backpack.transform, "BackpackPocket", PrimitiveType.Cube,
                        new Vector3(0f, -0.2f, -0.52f), new Vector3(0.8f, 0.4f, 0.2f), new Color(0.3f, 0.3f, 0.38f));
                    break;

                case 4:
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.7f, 0), new Vector3(0.44f, 0.48f, 0.28f), color);
                    AddPrimChild(container, "JacketCollar", PrimitiveType.Cube,
                        new Vector3(0, 1.06f, 0.04f), new Vector3(0.3f, 0.08f, 0.08f), color * 0.75f);
                    AddPrimChild(container, "JacketFlapL", PrimitiveType.Cube,
                        new Vector3(-0.08f, 0.62f, 0.12f), new Vector3(0.12f, 0.3f, 0.03f), color * 0.9f);
                    AddPrimChild(container, "JacketFlapR", PrimitiveType.Cube,
                        new Vector3(0.08f, 0.62f, 0.12f), new Vector3(0.12f, 0.3f, 0.03f), color * 0.9f);
                    AddPrimChild(container, "JacketZipper", PrimitiveType.Cube,
                        new Vector3(0, 0.7f, 0.13f), new Vector3(0.02f, 0.35f, 0.01f), new Color(0.8f, 0.75f, 0.4f));
                    break;
            }
        }

        private static void ApplyPants(GameObject character, int gender, int style, Color color)
        {
            var container = character.transform.Find("LegsContainer");
            if (container == null)
            {
                var go = new GameObject("LegsContainer");
                go.transform.SetParent(character.transform, false);
                container = go.transform;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
                Object.Destroy(container.GetChild(i).gameObject);

            style = Mathf.Clamp(style, 0, 3);

            // Hip pivot Y position (at belt/waist level) — legs swing from here
            const float hipY = 0.45f;

            switch (style)
            {
                case 0: // === BASIC LONG PANTS ===
                    {
                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.1f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.14f, 0.25f, 0.14f), color);
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.14f, 0.25f, 0.14f), color);

                        AddShoeToLeg(leftLeg, "LeftShoe");
                        AddShoeToLeg(rightLeg, "RightShoe");
                    }
                    break;

                case 1: // === SHORTS (NAM) / SKIRT (NỮ) ===
                    if (gender == 0) // Nam — shorts with exposed shins
                    {
                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.1f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.17f, 0), new Vector3(0.15f, 0.18f, 0.15f), color);
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.17f, 0), new Vector3(0.15f, 0.18f, 0.15f), color);

                        var leftShin = AddPrimChild(leftLeg.transform, "LeftShin", PrimitiveType.Capsule,
                            new Vector3(0f, -0.9f, 0f), new Vector3(0.66f, 0.66f, 0.66f), new Color(1f, 0.88f, 0.7f));
                        var rightShin = AddPrimChild(rightLeg.transform, "RightShin", PrimitiveType.Capsule,
                            new Vector3(0f, -0.9f, 0f), new Vector3(0.66f, 0.66f, 0.66f), new Color(1f, 0.88f, 0.7f));

                        AddShoeToLeg(leftShin, "LeftShoe");
                        AddShoeToLeg(rightShin, "RightShoe");
                    }
                    else // Nữ — skirt + bare legs
                    {
                        // Skirt stays attached to container (doesn't swing)
                        AddPrimChild(container, "Skirt", PrimitiveType.Cylinder,
                            new Vector3(0, 0.32f, 0), new Vector3(0.32f, 0.15f, 0.32f), color);

                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.08f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.3f, 0), new Vector3(0.1f, 0.15f, 0.1f), new Color(1f, 0.88f, 0.7f));
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.3f, 0), new Vector3(0.1f, 0.15f, 0.1f), new Color(1f, 0.88f, 0.7f));

                        AddShoeToLeg(leftLeg, "LeftShoe");
                        AddShoeToLeg(rightLeg, "RightShoe");
                    }
                    break;

                case 2: // === CARGO (NAM) / CAPRI (NỮ) ===
                    if (gender == 0) // Nam — cargo pants with pockets
                    {
                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.1f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.16f, 0.25f, 0.16f), color);
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.16f, 0.25f, 0.16f), color);

                        AddPrimChild(leftLeg.transform, "PocketL", PrimitiveType.Cube,
                            new Vector3(-0.55f, 0f, 0.1f), new Vector3(0.4f, 0.4f, 0.4f), color * 0.85f);
                        AddPrimChild(rightLeg.transform, "PocketR", PrimitiveType.Cube,
                            new Vector3(0.55f, 0f, 0.1f), new Vector3(0.4f, 0.4f, 0.4f), color * 0.85f);

                        AddShoeToLeg(leftLeg, "LeftShoe");
                        AddShoeToLeg(rightLeg, "RightShoe");
                    }
                    else // Nữ — capri pants with exposed shins
                    {
                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.1f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.13f, 0), new Vector3(0.14f, 0.14f, 0.14f), color);
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.13f, 0), new Vector3(0.14f, 0.14f, 0.14f), color);

                        var leftShin = AddPrimChild(leftLeg.transform, "LeftShin", PrimitiveType.Capsule,
                            new Vector3(0f, -0.9f, 0f), new Vector3(0.66f, 0.66f, 0.66f), new Color(1f, 0.88f, 0.7f));
                        var rightShin = AddPrimChild(rightLeg.transform, "RightShin", PrimitiveType.Capsule,
                            new Vector3(0f, -0.9f, 0f), new Vector3(0.66f, 0.66f, 0.66f), new Color(1f, 0.88f, 0.7f));

                        AddShoeToLeg(leftShin, "LeftShoe");
                        AddShoeToLeg(rightShin, "RightShoe");
                    }
                    break;

                case 3: // === SLIM FIT PANTS ===
                    {
                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.1f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.13f, 0.25f, 0.13f), color);
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.23f, 0), new Vector3(0.13f, 0.25f, 0.13f), color);

                        AddShoeToLeg(leftLeg, "LeftShoe");
                        AddShoeToLeg(rightLeg, "RightShoe");
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates LeftHip and RightHip empty GameObjects at the hip joint position.
        /// The animator rotates these pivots so legs swing naturally from the hip.
        /// </summary>
        private static (GameObject leftHip, GameObject rightHip) CreateHipPivots(Transform container, float hipY, float hipX)
        {
            var leftHip = new GameObject("LeftHip");
            leftHip.transform.SetParent(container, false);
            leftHip.transform.localPosition = new Vector3(-hipX, hipY, 0);

            var rightHip = new GameObject("RightHip");
            rightHip.transform.SetParent(container, false);
            rightHip.transform.localPosition = new Vector3(hipX, hipY, 0);

            return (leftHip, rightHip);
        }

        private static void AddShoeToLeg(GameObject leg, string shoeName)
        {
            var shoe = AddPrimChild(leg.transform, shoeName, PrimitiveType.Cube,
                new Vector3(0f, -0.92f, 0.28f), new Vector3(0.92f, 0.32f, 1.42f), new Color(0.18f, 0.18f, 0.2f));

            AddPrimChild(shoe.transform, "Sole", PrimitiveType.Cube,
                new Vector3(0f, -0.52f, 0f), new Vector3(1.08f, 0.2f, 1.05f), Color.white);

            AddPrimChild(shoe.transform, "Laces", PrimitiveType.Cube,
                new Vector3(0f, 0.52f, 0.1f), new Vector3(0.65f, 0.1f, 0.4f), Color.white);
        }

        private static GameObject AddPrimChild(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var mat = new Material(shader);
            mat.color = color;

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.05f);
            }
            else if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0.05f);
            }
            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0.0f);
            }

            if (color.a < 1f)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }
            obj.GetComponent<Renderer>().material = mat;
            Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        public static Material CreateMat(Color color)
        {
            if (color.a < 0.98f)
            {
                return CreateAdditiveMat(color);
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("UI/Default");

            Material mat = null;
            try
            {
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.color = color;
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
                    else if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.05f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CharacterVisuals] Failed to create material: {e.Message}");
            }

            return mat;
        }

        public static Material CreateAdditiveMat(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = null;
            try
            {
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.color = color;
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

                    if (shader.name.Contains("Standard"))
                    {
                        mat.SetFloat("_Mode", 3);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CharacterVisuals] Failed to create additive material: {e.Message}");
            }
            return mat;
        }

        private static void Destroy(UnityEngine.Object obj)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }
}
