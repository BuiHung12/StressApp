using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public static partial class CharacterVisuals
    {
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
                Object.DestroyImmediate(container.GetChild(i).gameObject);

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
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.52f, 0.32f, 0.52f), color);
                        AddPrimChild(container, "HairFront", PrimitiveType.Sphere,
                            new Vector3(0, 1.46f, 0.12f), new Vector3(0.44f, 0.22f, 0.25f), color * 1.05f);
                        AddPrimChild(container, "HairSideL", PrimitiveType.Capsule,
                            new Vector3(-0.25f, 1.35f, 0.05f), new Vector3(0.12f, 0.22f, 0.12f), color);
                        AddPrimChild(container, "HairSideR", PrimitiveType.Capsule,
                            new Vector3(0.25f, 1.35f, 0.05f), new Vector3(0.12f, 0.22f, 0.12f), color);
                        AddPrimChild(container, "Ponytail", PrimitiveType.Capsule,
                            new Vector3(0, 1.25f, -0.22f), new Vector3(0.14f, 0.32f, 0.14f), color);
                        AddPrimChild(container, "PonytailBand", PrimitiveType.Sphere,
                            new Vector3(0, 1.4f, -0.2f), new Vector3(0.14f, 0.08f, 0.14f), new Color(1f, 0.3f, 0.5f));
                        break;

                    case 1:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.5f, -0.02f), new Vector3(0.53f, 0.34f, 0.52f), color);
                        AddPrimChild(container, "HairBackL", PrimitiveType.Capsule,
                            new Vector3(-0.16f, 1.15f, -0.1f), new Vector3(0.18f, 0.38f, 0.16f), color);
                        AddPrimChild(container, "HairBackR", PrimitiveType.Capsule,
                            new Vector3(0.16f, 1.15f, -0.1f), new Vector3(0.18f, 0.38f, 0.16f), color);
                        AddPrimChild(container, "HairFringe", PrimitiveType.Sphere,
                            new Vector3(0, 1.44f, 0.2f), new Vector3(0.42f, 0.16f, 0.15f), color * 1.1f);
                        break;

                    case 2:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.52f, 0.32f, 0.5f), color);
                        AddPrimChild(container, "Bun", PrimitiveType.Sphere,
                            new Vector3(0f, 1.66f, -0.08f), new Vector3(0.24f, 0.24f, 0.24f), color);
                        break;

                    case 3:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.53f, 0.32f, 0.52f), color);
                        AddPrimChild(container, "BobL", PrimitiveType.Sphere,
                            new Vector3(-0.18f, 1.3f, 0.02f), new Vector3(0.22f, 0.28f, 0.22f), color);
                        AddPrimChild(container, "BobR", PrimitiveType.Sphere,
                            new Vector3(0.18f, 1.3f, 0.02f), new Vector3(0.22f, 0.28f, 0.22f), color);
                        break;

                    case 4:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.52f, 0.32f, 0.52f), color);
                        AddPrimChild(container, "BraidL", PrimitiveType.Capsule,
                            new Vector3(-0.18f, 1.08f, 0.06f), new Vector3(0.1f, 0.26f, 0.1f), color);
                        AddPrimChild(container, "BraidR", PrimitiveType.Capsule,
                            new Vector3(0.18f, 1.08f, 0.06f), new Vector3(0.1f, 0.26f, 0.1f), color);
                        break;

                    case 5:
                        AddPrimChild(container, "HairTop", PrimitiveType.Sphere,
                            new Vector3(0, 1.48f, -0.02f), new Vector3(0.52f, 0.32f, 0.52f), color);
                        var catEarL = AddPrimChild(container, "CatEarL", PrimitiveType.Cube,
                            new Vector3(-0.15f, 1.62f, 0f), new Vector3(0.09f, 0.09f, 0.09f), color);
                        catEarL.transform.localRotation = Quaternion.Euler(0, 0, 45);

                        AddPrimChild(catEarL.transform, "InnerEarL", PrimitiveType.Cube,
                            new Vector3(0f, 0f, 0.45f), new Vector3(0.6f, 0.6f, 0.2f), new Color(1f, 0.6f, 0.6f));

                        var catEarR = AddPrimChild(container, "CatEarR", PrimitiveType.Cube,
                            new Vector3(0.15f, 1.62f, 0f), new Vector3(0.09f, 0.09f, 0.09f), color);
                        catEarR.transform.localRotation = Quaternion.Euler(0, 0, -45);

                        AddPrimChild(catEarR.transform, "InnerEarR", PrimitiveType.Cube,
                            new Vector3(0f, 0f, 0.45f), new Vector3(0.6f, 0.6f, 0.2f), new Color(1f, 0.6f, 0.6f));
                        break;
                }
            }
        }

        private static void ApplyOutfit(GameObject character, int gender, int style, Color color)
        {
            // Hide all default mesh renderers that are not part of custom procedural body
            foreach (var r in character.GetComponentsInChildren<Renderer>(true))
            {
                string pName = r.transform.name;
                Transform p = r.transform.parent;
                bool isCustom = pName == "Head" || pName == "Neck" || pName == "Nose" || pName == "Mouth" || pName == "Belt" ||
                                (p != null && (p.name == "HairContainer" || p.name == "TorsoContainer" || p.name == "LegsContainer" || p.name == "Head" || p.name == "LeftHip" || p.name == "RightHip" || p.name.StartsWith("Left") || p.name.StartsWith("Right")));
                if (!isCustom) r.enabled = false;
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
                Object.DestroyImmediate(container.GetChild(i).gameObject);

            style = Mathf.Clamp(style, 0, 4);

            switch (style)
            {
                case 0: // === STREETWEAR T-SHIRT ===
                    float scaleY = gender == 0 ? 0.45f : 0.35f;
                    float posY = gender == 0 ? 0.7f : 0.78f;
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, posY, 0), new Vector3(0.42f, scaleY, 0.26f), color);
                    
                    // Lower Hem Trim
                    AddPrimChild(container, "ShirtHem", PrimitiveType.Cylinder,
                        new Vector3(0, posY - 0.22f, 0), new Vector3(0.44f, 0.04f, 0.28f), color * 0.9f);

                    // Ribbed Neck Collar Rim
                    AddPrimChild(container, "NeckRim", PrimitiveType.Cylinder,
                        new Vector3(0, 1.05f, 0), new Vector3(0.24f, 0.04f, 0.24f), Color.white);
                    break;

                case 1: // === LUXURY SUIT BLAZER & CHIC DRESS ===
                    if (gender == 0) // NAM — K-Fashion Luxury Suit Blazer
                    {
                        AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                            new Vector3(0, 0.7f, 0), new Vector3(0.46f, 0.48f, 0.28f), color);
                        
                        // Flared Suit Jacket Lower Hem
                        AddPrimChild(container, "JacketLowerHem", PrimitiveType.Cube,
                            new Vector3(0, 0.48f, 0.02f), new Vector3(0.48f, 0.22f, 0.3f), color * 0.92f);

                        // Inner Collared White Shirt
                        AddPrimChild(container, "InnerShirt", PrimitiveType.Cube,
                            new Vector3(0, 0.74f, 0.1f), new Vector3(0.18f, 0.38f, 0.06f), Color.white);
                        
                        // Red Silk Necktie
                        AddPrimChild(container, "Tie", PrimitiveType.Cube,
                            new Vector3(0, 0.72f, 0.13f), new Vector3(0.06f, 0.34f, 0.02f), new Color(0.85f, 0.15f, 0.15f));

                        // Notched Lapel Collars
                        AddPrimChild(container, "CollarL", PrimitiveType.Cube,
                            new Vector3(-0.14f, 1.05f, 0.08f), new Vector3(0.11f, 0.14f, 0.06f), color * 0.8f);
                        AddPrimChild(container, "CollarR", PrimitiveType.Cube,
                            new Vector3(0.14f, 1.05f, 0.08f), new Vector3(0.11f, 0.14f, 0.06f), color * 0.8f);

                        AddPrimChild(container, "VestButton", PrimitiveType.Sphere,
                            new Vector3(0.08f, 0.8f, 0.14f), new Vector3(0.035f, 0.035f, 0.02f), new Color(0.92f, 0.78f, 0.28f));
                        AddPrimChild(container, "VestButton2", PrimitiveType.Sphere,
                            new Vector3(0.08f, 0.65f, 0.14f), new Vector3(0.035f, 0.035f, 0.02f), new Color(0.92f, 0.78f, 0.28f));
                    }
                    else // NỮ — Chic Pink Flared A-Line Dress (Váy Bồng Bềnh)
                    {
                        AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                            new Vector3(0, 0.78f, 0), new Vector3(0.38f, 0.35f, 0.23f), color);
                        
                        // Broad Flared A-Line Pleated Skirt (Váy xòe bồng)
                        var dressSkirt = AddPrimChild(container, "DressSkirt", PrimitiveType.Cylinder,
                            new Vector3(0, 0.44f, 0), new Vector3(0.58f, 0.26f, 0.58f), color);
                        
                        // White Lace Ruffle Border on Skirt
                        AddPrimChild(dressSkirt.transform, "LaceRuffle", PrimitiveType.Cylinder,
                            new Vector3(0, -0.45f, 0), new Vector3(1.06f, 0.12f, 1.06f), Color.white);

                        // Waist Ribbon Bow
                        AddPrimChild(container, "WaistRibbon", PrimitiveType.Sphere,
                            new Vector3(0, 0.58f, 0.13f), new Vector3(0.12f, 0.08f, 0.06f), Color.white);
                    }
                    break;

                case 2: // === OVERSIZED CYBER NEON HOODIE ===
                    // Baggy Oversized Torso
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.72f, 0), new Vector3(0.48f, 0.48f, 0.32f), color);

                    // Loose Flared Waist Hem
                    AddPrimChild(container, "HoodieWaistHem", PrimitiveType.Cylinder,
                        new Vector3(0, 0.48f, 0), new Vector3(0.5f, 0.08f, 0.34f), color * 0.88f);

                    // Thick Volumetric Hood Resting on Shoulders
                    AddPrimChild(container, "Hood", PrimitiveType.Sphere,
                        new Vector3(0, 1.18f, -0.12f), new Vector3(0.42f, 0.28f, 0.28f), color * 0.88f);
                    AddPrimChild(container, "HoodInner", PrimitiveType.Sphere,
                        new Vector3(0, 1.18f, -0.08f), new Vector3(0.36f, 0.22f, 0.22f), color * 0.75f);

                    // Flared Kangaroo Pouch Pocket
                    AddPrimChild(container, "HoodiePouch", PrimitiveType.Cube,
                        new Vector3(0, 0.58f, 0.15f), new Vector3(0.28f, 0.14f, 0.05f), color * 0.85f);

                    // Hanging Drawstrings
                    AddPrimChild(container, "DrawstringL", PrimitiveType.Cylinder,
                        new Vector3(-0.07f, 0.88f, 0.16f), new Vector3(0.015f, 0.14f, 0.015f), Color.white);
                    AddPrimChild(container, "DrawstringR", PrimitiveType.Cylinder,
                        new Vector3(0.07f, 0.88f, 0.16f), new Vector3(0.015f, 0.14f, 0.015f), Color.white);
                    break;

                case 3: // === ADVENTURE EXPLORER TANK & BACKPACK ===
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.7f, 0), new Vector3(0.38f, 0.43f, 0.24f), color);
                    AddPrimChild(container, "StrapL", PrimitiveType.Cube,
                        new Vector3(-0.12f, 1.0f, 0.02f), new Vector3(0.04f, 0.15f, 0.04f), color);
                    AddPrimChild(container, "StrapR", PrimitiveType.Cube,
                        new Vector3(0.12f, 1.0f, 0.02f), new Vector3(0.04f, 0.15f, 0.04f), color);

                    var backpack = AddPrimChild(container, "Backpack", PrimitiveType.Cube,
                        new Vector3(0f, 0.72f, -0.16f), new Vector3(0.26f, 0.32f, 0.16f), new Color(0.18f, 0.22f, 0.3f));
                    AddPrimChild(backpack.transform, "BackpackPocket", PrimitiveType.Cube,
                        new Vector3(0f, -0.2f, -0.52f), new Vector3(0.85f, 0.42f, 0.22f), new Color(0.28f, 0.34f, 0.45f));
                    AddPrimChild(backpack.transform, "ZipLine", PrimitiveType.Cube,
                        new Vector3(0f, 0.1f, -0.53f), new Vector3(0.7f, 0.05f, 0.02f), new Color(0.92f, 0.78f, 0.28f));
                    break;

                case 4: // === BIKER LEATHER JACKET (BROAD SHOULDERS) ===
                    AddPrimChild(container, "Torso", PrimitiveType.Capsule,
                        new Vector3(0, 0.72f, 0), new Vector3(0.46f, 0.48f, 0.3f), color);
                    
                    // Broad 3D Shoulder Epaulet Pads
                    AddPrimChild(container, "ShoulderPadL", PrimitiveType.Cube,
                        new Vector3(-0.25f, 1.02f, 0), new Vector3(0.12f, 0.06f, 0.28f), color * 0.7f);
                    AddPrimChild(container, "ShoulderPadR", PrimitiveType.Cube,
                        new Vector3(0.25f, 1.02f, 0), new Vector3(0.12f, 0.06f, 0.28f), color * 0.7f);

                    AddPrimChild(container, "JacketCollar", PrimitiveType.Cube,
                        new Vector3(0, 1.08f, 0.04f), new Vector3(0.36f, 0.1f, 0.1f), color * 0.75f);
                    AddPrimChild(container, "JacketFlapL", PrimitiveType.Cube,
                        new Vector3(-0.1f, 0.64f, 0.14f), new Vector3(0.15f, 0.32f, 0.04f), color * 0.88f);
                    AddPrimChild(container, "JacketFlapR", PrimitiveType.Cube,
                        new Vector3(0.1f, 0.64f, 0.14f), new Vector3(0.15f, 0.32f, 0.04f), color * 0.88f);
                    AddPrimChild(container, "JacketZipper", PrimitiveType.Cube,
                        new Vector3(0.02f, 0.7f, 0.155f), new Vector3(0.025f, 0.38f, 0.02f), new Color(0.85f, 0.82f, 0.5f));
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
                Object.DestroyImmediate(container.GetChild(i).gameObject);

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
                    else // Nữ — Flared Pleated Skirt (Chân váy xếp ly bồng bềnh)
                    {
                        var skirt = AddPrimChild(container, "Skirt", PrimitiveType.Cylinder,
                            new Vector3(0, 0.36f, 0), new Vector3(0.54f, 0.22f, 0.54f), color);

                        // White Bottom Hem Ruffle Trim
                        AddPrimChild(skirt.transform, "SkirtRuffle", PrimitiveType.Cylinder,
                            new Vector3(0, -0.45f, 0), new Vector3(1.05f, 0.12f, 1.05f), Color.white);

                        var (leftHip, rightHip) = CreateHipPivots(container, hipY, 0.08f);

                        var leftLeg = AddPrimChild(leftHip.transform, "LeftLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.3f, 0), new Vector3(0.12f, 0.16f, 0.12f), new Color(1f, 0.88f, 0.7f));
                        var rightLeg = AddPrimChild(rightHip.transform, "RightLeg", PrimitiveType.Capsule,
                            new Vector3(0, -0.3f, 0), new Vector3(0.12f, 0.16f, 0.12f), new Color(1f, 0.88f, 0.7f));

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


    }
    }
