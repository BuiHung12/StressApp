using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Procedural character visuals generator and synchronizer (3D Toy style).
    /// </summary>
    public static partial class CharacterVisuals
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
            neck.transform.SetParent(character.transform, false);
            neck.transform.localPosition = new Vector3(0, 1.12f, 0);
            neck.transform.localScale = new Vector3(0.14f, 0.08f, 0.14f);
            neck.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(neck.GetComponent<Collider>());

            // === HEAD (Cute Chibi Sphere) ===
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(character.transform, false);
            head.transform.localPosition = new Vector3(0, 1.38f, 0);
            head.transform.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            head.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(head.GetComponent<Collider>());

            // === NOSE ===
            var nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            nose.transform.SetParent(head.transform, false);
            nose.transform.localPosition = new Vector3(0f, -0.05f, 0.51f);
            nose.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
            nose.GetComponent<Renderer>().material = CreateMat(skinColor * 0.95f);
            Destroy(nose.GetComponent<Collider>());

            // === MOUTH (Cute Smile Arc) ===
            var mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.name = "Mouth";
            mouth.transform.SetParent(head.transform, false);
            mouth.transform.localPosition = new Vector3(0f, -0.18f, 0.48f);
            mouth.transform.localScale = new Vector3(0.14f, 0.04f, 0.06f);
            mouth.GetComponent<Renderer>().material = CreateMat(new Color(0.2f, 0.12f, 0.12f));
            Destroy(mouth.GetComponent<Collider>());

            var tongue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tongue.name = "Tongue";
            tongue.transform.SetParent(mouth.transform, false);
            tongue.transform.localPosition = new Vector3(0f, -0.25f, 0.2f);
            tongue.transform.localScale = new Vector3(0.75f, 0.55f, 0.5f);
            tongue.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.45f, 0.55f));
            Destroy(tongue.GetComponent<Collider>());

            // === EARS ===
            var earL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earL.name = "EarL";
            earL.transform.SetParent(head.transform, false);
            earL.transform.localPosition = new Vector3(-0.51f, 0f, 0f);
            earL.transform.localScale = new Vector3(0.12f, 0.16f, 0.1f);
            earL.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(earL.GetComponent<Collider>());

            var earR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earR.name = "EarR";
            earR.transform.SetParent(head.transform, false);
            earR.transform.localPosition = new Vector3(0.51f, 0f, 0f);
            earR.transform.localScale = new Vector3(0.12f, 0.16f, 0.1f);
            earR.GetComponent<Renderer>().material = CreateMat(skinColor);
            Destroy(earR.GetComponent<Collider>());

            // === EXPRESSIVE CATCHLIGHT EYES ===
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -0.19f : 0.19f;
                string sName = side == 0 ? "L" : "R";

                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = $"Eye{sName}";
                eye.transform.SetParent(head.transform, false);
                eye.transform.localPosition = new Vector3(x, 0.06f, 0.44f);
                eye.transform.localScale = new Vector3(0.13f, 0.17f, 0.08f);
                eye.GetComponent<Renderer>().material = CreateMat(Color.white);
                Destroy(eye.GetComponent<Collider>());

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.name = $"Pupil{sName}";
                pupil.transform.SetParent(eye.transform, false);
                pupil.transform.localPosition = new Vector3(0f, 0f, 0.45f);
                pupil.transform.localScale = new Vector3(0.6f, 0.6f, 0.3f);
                pupil.GetComponent<Renderer>().material = CreateMat(new Color(0.1f, 0.12f, 0.18f));
                Destroy(pupil.GetComponent<Collider>());

                var shiny1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shiny1.name = $"Shiny1{sName}";
                shiny1.transform.SetParent(pupil.transform, false);
                shiny1.transform.localPosition = new Vector3(-0.25f, 0.25f, 0.45f);
                shiny1.transform.localScale = new Vector3(0.38f, 0.38f, 0.2f);
                shiny1.GetComponent<Renderer>().material = CreateMat(Color.white);
                Destroy(shiny1.GetComponent<Collider>());

                var shiny2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shiny2.name = $"Shiny2{sName}";
                shiny2.transform.SetParent(pupil.transform, false);
                shiny2.transform.localPosition = new Vector3(0.2f, -0.2f, 0.45f);
                shiny2.transform.localScale = new Vector3(0.22f, 0.22f, 0.2f);
                shiny2.GetComponent<Renderer>().material = CreateMat(Color.white);
                Destroy(shiny2.GetComponent<Collider>());

                var brow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                brow.name = $"Brow{sName}";
                brow.transform.SetParent(head.transform, false);
                brow.transform.localPosition = new Vector3(x, 0.21f, 0.46f);
                brow.transform.localScale = new Vector3(0.14f, 0.025f, 0.04f);
                brow.transform.localRotation = Quaternion.Euler(0, 0, side == 0 ? 5f : -5f);
                brow.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.12f, 0.1f));
                Destroy(brow.GetComponent<Collider>());

                var cheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cheek.name = $"Cheek{sName}";
                cheek.transform.SetParent(head.transform, false);
                cheek.transform.localPosition = new Vector3(side == 0 ? -0.26f : 0.26f, -0.06f, 0.42f);
                cheek.transform.localScale = new Vector3(0.09f, 0.045f, 0.04f);
                cheek.GetComponent<Renderer>().material = CreateMat(new Color(1f, 0.45f, 0.55f, 0.65f));
                Destroy(cheek.GetComponent<Collider>());
            }

            // === CONTAINERS ===
            var hairContainer = new GameObject("HairContainer");
            hairContainer.transform.SetParent(character.transform, false);

            var torsoContainer = new GameObject("TorsoContainer");
            torsoContainer.transform.SetParent(character.transform, false);

            // === BELT & BUCKLE ===
            var belt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            belt.name = "Belt";
            belt.transform.SetParent(character.transform, false);
            belt.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            belt.transform.localScale = new Vector3(0.38f, 0.03f, 0.24f);
            belt.GetComponent<Renderer>().material = CreateMat(new Color(0.15f, 0.15f, 0.18f));
            Destroy(belt.GetComponent<Collider>());

            var buckle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buckle.name = "Buckle";
            buckle.transform.SetParent(belt.transform, false);
            buckle.transform.localPosition = new Vector3(0f, 0f, 0.52f);
            buckle.transform.localScale = new Vector3(0.22f, 1.2f, 0.12f);
            buckle.GetComponent<Renderer>().material = CreateMat(new Color(0.92f, 0.78f, 0.28f));
            Destroy(buckle.GetComponent<Collider>());

            // === SEAMLESS ANATOMICAL ARMS (Nối vai khít ngực) ===
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? -0.21f : 0.21f;
                string sName = side == 0 ? "Left" : "Right";

                // Shoulder Pivot GameObject (Anatomical Shoulder Joint)
                var shoulderPivot = new GameObject($"{sName}ShoulderPivot");
                shoulderPivot.transform.SetParent(character.transform, false);
                shoulderPivot.transform.localPosition = new Vector3(x, 0.86f, 0f);

                // Shoulder Sphere (Attached to pivot)
                var shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shoulder.name = $"{sName}Shoulder";
                shoulder.transform.SetParent(shoulderPivot.transform, false);
                shoulder.transform.localPosition = Vector3.zero;
                shoulder.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
                shoulder.GetComponent<Renderer>().material = CreateMat(bodyColor);
                Destroy(shoulder.GetComponent<Collider>());

                // Arm Capsule (Parented to Shoulder Pivot)
                var arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                arm.name = $"{sName}Arm";
                arm.transform.SetParent(shoulderPivot.transform, false);
                arm.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                arm.transform.localScale = new Vector3(0.13f, 0.24f, 0.13f);
                arm.GetComponent<Renderer>().material = CreateMat(skinColor);
                Destroy(arm.GetComponent<Collider>());

                var sleeve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sleeve.name = "Sleeve";
                sleeve.transform.SetParent(arm.transform, false);
                sleeve.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                sleeve.transform.localScale = new Vector3(1.15f, 0.55f, 1.15f);
                sleeve.GetComponent<Renderer>().material = CreateMat(bodyColor);
                Destroy(sleeve.GetComponent<Collider>());

                var hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hand.name = $"Hand{sName[0]}";
                hand.transform.SetParent(arm.transform, false);
                hand.transform.localPosition = new Vector3(0f, -1.02f, 0f);
                hand.transform.localScale = new Vector3(0.92f, 0.38f, 0.92f);
                hand.GetComponent<Renderer>().material = CreateMat(skinColor);
                Destroy(hand.GetComponent<Collider>());
            }

            // === LEGS CONTAINER ===
            var legsContainer = new GameObject("LegsContainer");
            legsContainer.transform.SetParent(character.transform, false);

            // === SHADOW (Ground Touch) ===
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "Shadow";
            shadow.transform.SetParent(character.transform, false);
            shadow.transform.localPosition = new Vector3(0.0f, 0.005f, 0.0f);
            shadow.transform.localScale = new Vector3(0.52f, 0.005f, 0.52f);
            shadow.GetComponent<Renderer>().material = CreateMat(new Color(0, 0, 0, 0.22f));
            Destroy(shadow.GetComponent<Collider>());

            character.transform.localScale = Vector3.one * 0.45f;
            character.AddComponent<CharacterAnimator>();

            ApplyCustomization(character, 0, 0, new Color(0.18f, 0.12f, 0.08f), 0, bodyColor, 0, new Color(0.25f, 0.35f, 0.55f));

            return character;
        }

        public static void ApplyCustomization(GameObject character, int gender, int hairStyle, Color hairColor, int outfitStyle, Color bodyColor, int pantsStyle, Color pantsColor)
        {
            if (character == null) return;
            if (NetworkSetup.IsHeadlessServer()) return;
            // Debug.Log($"[ApplyCustomization] Character: {character.name}, Gender: {gender}, HairStyle: {hairStyle}, OutfitStyle: {outfitStyle}, PantsStyle: {pantsStyle}");

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

        private static void AddShoeToLeg(GameObject leg, string shoeName)
        {
            // Main Sneaker Body
            var shoe = AddPrimChild(leg.transform, shoeName, PrimitiveType.Cube,
                new Vector3(0f, -0.55f, 0.22f), new Vector3(1.15f, 0.45f, 1.45f), new Color(0.12f, 0.14f, 0.18f));

            // Thick White Rubber Sneaker Sole
            AddPrimChild(shoe.transform, "Sole", PrimitiveType.Cube,
                new Vector3(0f, -0.45f, 0.05f), new Vector3(1.12f, 0.25f, 1.15f), Color.white);

            // Front Rubber Toe Cap
            AddPrimChild(shoe.transform, "ToeCap", PrimitiveType.Sphere,
                new Vector3(0f, -0.1f, 0.42f), new Vector3(0.95f, 0.7f, 0.45f), Color.white);

            // Shoe Laces / Tongue
            AddPrimChild(shoe.transform, "Laces", PrimitiveType.Cube,
                new Vector3(0f, 0.35f, 0.05f), new Vector3(0.75f, 0.18f, 0.45f), Color.white);

            // Side Accent Stripe
            AddPrimChild(shoe.transform, "StripeL", PrimitiveType.Cube,
                new Vector3(-0.52f, 0.0f, -0.05f), new Vector3(0.05f, 0.35f, 0.55f), new Color(0.02f, 0.65f, 1f));
            AddPrimChild(shoe.transform, "StripeR", PrimitiveType.Cube,
                new Vector3(0.52f, 0.0f, -0.05f), new Vector3(0.05f, 0.35f, 0.55f), new Color(0.02f, 0.65f, 1f));
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
