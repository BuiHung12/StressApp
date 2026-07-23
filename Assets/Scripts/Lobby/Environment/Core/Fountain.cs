using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public static partial class EnvironmentBuilder
    {
        public static void CreateFountain3D()
        {
            var fountain = new GameObject("Fountain");

            // Outer Marble Plaza Rim
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "FountainPlaza";
            plaza.transform.SetParent(fountain.transform, false);
            plaza.transform.localPosition = new Vector3(0, 0.015f, 0);
            plaza.transform.localScale = new Vector3(5.2f, 0.02f, 5.2f);
            plaza.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.82f, 0.8f, 0.78f));
            Object.Destroy(plaza.GetComponent<Collider>());

            // Outer Basin Base Ring
            var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "Base_Collider";
            baseRing.transform.SetParent(fountain.transform, false);
            baseRing.transform.localPosition = new Vector3(0, 0.28f, 0);
            baseRing.transform.localScale = new Vector3(4.0f, 0.28f, 4.0f);
            baseRing.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.32f, 0.35f, 0.42f));

            // Gold Trim Ring
            var goldTrim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goldTrim.name = "GoldTrim";
            goldTrim.transform.SetParent(fountain.transform, false);
            goldTrim.transform.localPosition = new Vector3(0, 0.43f, 0);
            goldTrim.transform.localScale = new Vector3(4.08f, 0.04f, 4.08f);
            goldTrim.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.92f, 0.76f, 0.28f));
            Object.Destroy(goldTrim.GetComponent<Collider>());

            // Main Crystal Pool Water
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "WaterPool";
            water.transform.SetParent(fountain.transform, false);
            water.transform.localPosition = new Vector3(0, 0.38f, 0);
            water.transform.localScale = new Vector3(3.7f, 0.06f, 3.7f);
            water.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.1f, 0.65f, 0.95f, 0.85f));
            Object.Destroy(water.GetComponent<Collider>());

            // Submerged LED Light Studs
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f * Mathf.Deg2Rad;
                var led = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                led.name = $"LED_{i}";
                led.transform.SetParent(fountain.transform, false);
                led.transform.localPosition = new Vector3(Mathf.Cos(ang) * 1.6f, 0.40f, Mathf.Sin(ang) * 1.6f);
                led.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
                led.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.95f, 1.0f));
                Object.Destroy(led.GetComponent<Collider>());
            }

            // Central Ornate Column
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(fountain.transform, false);
            pillar.transform.localPosition = new Vector3(0, 0.75f, 0);
            pillar.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            pillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.78f, 0.76f, 0.74f));
            Object.Destroy(pillar.GetComponent<Collider>());

            // Middle Basin Bowl
            var midBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midBowl.name = "MiddleBowl";
            midBowl.transform.SetParent(fountain.transform, false);
            midBowl.transform.localPosition = new Vector3(0, 1.12f, 0);
            midBowl.transform.localScale = new Vector3(2.3f, 0.12f, 2.3f);
            midBowl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.35f, 0.38f, 0.45f));
            Object.Destroy(midBowl.GetComponent<Collider>());

            var midWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midWater.name = "MiddleWater";
            midWater.transform.SetParent(fountain.transform, false);
            midWater.transform.localPosition = new Vector3(0, 1.19f, 0);
            midWater.transform.localScale = new Vector3(2.15f, 0.04f, 2.15f);
            midWater.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.25f, 0.78f, 0.98f, 0.8f));
            Object.Destroy(midWater.GetComponent<Collider>());

            // Upper Column & Basin
            var topPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topPillar.name = "TopPillar";
            topPillar.transform.SetParent(fountain.transform, false);
            topPillar.transform.localPosition = new Vector3(0, 1.55f, 0);
            topPillar.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            topPillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.82f, 0.8f, 0.78f));
            Object.Destroy(topPillar.GetComponent<Collider>());

            var topBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topBowl.name = "TopBowl";
            topBowl.transform.SetParent(fountain.transform, false);
            topBowl.transform.localPosition = new Vector3(0, 1.8f, 0);
            topBowl.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            topBowl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.32f, 0.35f, 0.42f));
            Object.Destroy(topBowl.GetComponent<Collider>());

            // Glowing Crystal Orb Apex
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "GlowingOrbApex";
            orb.transform.SetParent(fountain.transform, false);
            orb.transform.localPosition = new Vector3(0, 2.05f, 0);
            orb.transform.localScale = Vector3.one * 0.45f;
            orb.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.4f, 0.92f, 1.0f));
            Object.Destroy(orb.GetComponent<Collider>());

            // Cascading Water Jets & Spray Droplets
            Vector3[] jets = {
                new(0, 2.3f, 0),
                new(0.35f, 2.15f, 0.2f), new(-0.35f, 2.15f, -0.2f),
                new(-0.2f, 2.18f, 0.35f), new(0.2f, 2.18f, -0.35f),
                new(0.7f, 1.5f, 0.4f), new(-0.7f, 1.5f, -0.4f),
                new(-0.4f, 1.5f, 0.7f), new(0.4f, 1.5f, -0.7f)
            };
            Color waterColor = new Color(0.6f, 0.92f, 1.0f, 0.9f);
            for (int i = 0; i < jets.Length; i++)
            {
                var jet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                jet.name = $"WaterJet_{i}";
                jet.transform.SetParent(fountain.transform, false);
                jet.transform.localPosition = jets[i];
                jet.transform.localScale = Vector3.one * (i == 0 ? 0.28f : 0.18f);
                jet.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(waterColor);
                Object.Destroy(jet.GetComponent<Collider>());
            }
        }


    }
    }
