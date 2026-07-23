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
        public static void CreateFences(float lobbySize)
        {
            Color fenceColor = new Color(0.96f, 0.94f, 0.90f);
            float half = lobbySize * 0.45f;
            float step = 1.6f;

            for (float x = -half; x < half; x += step)
            {
                CreateFenceSegment(new Vector3(x, 0, half), new Vector3(x + step, 0, half), fenceColor);
                CreateFenceSegment(new Vector3(x, 0, -half), new Vector3(x + step, 0, -half), fenceColor);
            }
            for (float z = -half; z < half; z += step)
            {
                CreateFenceSegment(new Vector3(half, 0, z), new Vector3(half, 0, z + step), fenceColor, true);
                CreateFenceSegment(new Vector3(-half, 0, z), new Vector3(-half, 0, z + step), fenceColor, true);
            }
        }

        public static GameObject CreateFenceSegment(Vector3 start, Vector3 end, Color color, bool rotate90 = false)
        {
            var segment = new GameObject("FenceSegment");
            segment.transform.position = start;

            float length = Vector3.Distance(start, end);

            var fenceCollider = segment.AddComponent<BoxCollider>();
            fenceCollider.center = new Vector3(length * 0.5f, 0.5f, 0);
            fenceCollider.size = new Vector3(length, 1.0f, 0.2f);

            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(segment.transform, false);
            post.transform.localPosition = new Vector3(0, 0.5f, 0);
            post.transform.localScale = new Vector3(0.12f, 1.0f, 0.12f);
            post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            Object.Destroy(post.GetComponent<Collider>());

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "PostCap";
            cap.transform.SetParent(segment.transform, false);
            cap.transform.localPosition = new Vector3(0, 1.04f, 0);
            cap.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            cap.transform.localRotation = Quaternion.Euler(45, 45, 0);
            cap.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color * 0.9f);
            Object.Destroy(cap.GetComponent<Collider>());

            for (int i = 0; i < 2; i++)
            {
                float y = i == 0 ? 0.3f : 0.7f;
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = $"Rail_{i}";
                rail.transform.SetParent(segment.transform, false);
                rail.transform.localPosition = new Vector3(length * 0.5f, y, 0);
                rail.transform.localScale = new Vector3(length, 0.06f, 0.05f);
                rail.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color * 0.95f);
                Object.Destroy(rail.GetComponent<Collider>());
            }

            int picketsCount = 3;
            for (int i = 1; i <= picketsCount; i++)
            {
                float t = (float)i / (picketsCount + 1);
                float x = length * t;

                var picket = new GameObject($"Picket_{i}");
                picket.transform.SetParent(segment.transform, false);
                picket.transform.localPosition = new Vector3(x, 0, 0);

                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(picket.transform, false);
                body.transform.localPosition = new Vector3(0, 0.45f, 0);
                body.transform.localScale = new Vector3(0.08f, 0.9f, 0.03f);
                body.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
                Object.Destroy(body.GetComponent<Collider>());

                var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tip.name = "Tip";
                tip.transform.SetParent(picket.transform, false);
                tip.transform.localPosition = new Vector3(0, 0.93f, 0);
                tip.transform.localScale = new Vector3(0.08f, 0.08f, 0.03f);
                tip.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                tip.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
                Object.Destroy(tip.GetComponent<Collider>());
            }

            if (rotate90)
            {
                segment.transform.rotation = Quaternion.Euler(0, 90f, 0);
            }

            return segment;
        }

        public static void CreatePortals(float lobbySize)
        {
            float pathEnd = lobbySize * 0.8f * 0.5f - 0.5f;

            CreatePortal("GardenPortal", new Vector3(0, 0, pathEnd), 0f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.15f, 0.85f, 0.3f, 0.7f),
                "Vườn Trên Cao");

            CreatePortal("PrisonPortal", new Vector3(0, 0, -pathEnd), 0f,
                new Color(0.25f, 0.25f, 0.3f),
                new Color(0.85f, 0.15f, 0.15f, 0.7f),
                "Nhà Tù");

            CreatePortal("FishingPortal", new Vector3(pathEnd, 0, 0), 90f,
                new Color(0.3f, 0.35f, 0.4f),
                new Color(0.2f, 0.5f, 0.95f, 0.7f),
                "Khu Câu Cá");

            CreatePortal("StudyPortal", new Vector3(-pathEnd, 0, 0), 90f,
                new Color(0.4f, 0.32f, 0.22f),
                new Color(0.95f, 0.75f, 0.15f, 0.7f),
                "Khu Học Tập");
        }

        public static void CreatePortal(string name, Vector3 pos, float rotY, Color ringColor, Color energyColor, string label)
        {
            var portal = new GameObject(name);
            portal.transform.position = pos;
            portal.transform.rotation = Quaternion.Euler(0, rotY, 0);

            float padRadius = 0.7f;

            var basePad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePad.name = "TeleportPad";
            basePad.transform.SetParent(portal.transform, false);
            basePad.transform.localPosition = new Vector3(0, 0.02f, 0);
            basePad.transform.localScale = new Vector3(padRadius * 2, 0.02f, padRadius * 2);
            basePad.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.12f, 0.14f, 0.18f));
            Object.DestroyImmediate(basePad.GetComponent<Collider>());

            var neonBorder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonBorder.name = "NeonBorder";
            neonBorder.transform.SetParent(portal.transform, false);
            neonBorder.transform.localPosition = new Vector3(0, 0.03f, 0);
            neonBorder.transform.localScale = new Vector3(padRadius * 1.85f, 0.022f, padRadius * 1.85f);
            neonBorder.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor);
            Object.Destroy(neonBorder.GetComponent<Collider>());

            var innerPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerPlate.name = "InnerPlate";
            innerPlate.transform.SetParent(portal.transform, false);
            innerPlate.transform.localPosition = new Vector3(0, 0.04f, 0);
            innerPlate.transform.localScale = new Vector3(padRadius * 1.5f, 0.024f, padRadius * 1.5f);
            innerPlate.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.08f, 0.09f, 0.11f));
            Object.Destroy(innerPlate.GetComponent<Collider>());

            var coreGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreGlow.name = "CoreGlow";
            coreGlow.transform.SetParent(portal.transform, false);
            coreGlow.transform.localPosition = new Vector3(0, 0.05f, 0);
            coreGlow.transform.localScale = new Vector3(padRadius * 1.1f, 0.026f, padRadius * 1.1f);
            Color intenseEnergy = new Color(energyColor.r * 1.2f, energyColor.g * 1.2f, energyColor.b * 1.2f, 0.9f);
            coreGlow.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(intenseEnergy);
            Object.Destroy(coreGlow.GetComponent<Collider>());

            var holoBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            holoBeam.name = "HoloBeam";
            holoBeam.transform.SetParent(portal.transform, false);
            holoBeam.transform.localPosition = new Vector3(0, 0.6f, 0);
            holoBeam.transform.localScale = new Vector3(padRadius * 1.1f, 0.6f, padRadius * 1.1f);
            Color beamColor = new Color(energyColor.r, energyColor.g, energyColor.b, 0.22f);
            holoBeam.GetComponent<Renderer>().material = CharacterVisuals.CreateAdditiveMat(beamColor);
            Object.Destroy(holoBeam.GetComponent<Collider>());

            float dist = padRadius * 0.8f;
            Vector3[] beaconOffsets = {
                new(dist, 0.05f, dist),
                new(-dist, 0.05f, dist),
                new(dist, 0.05f, -dist),
                new(-dist, 0.05f, -dist)
            };

            foreach (var offset in beaconOffsets)
            {
                var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beacon.name = "BeaconStud";
                beacon.transform.SetParent(portal.transform, false);
                beacon.transform.localPosition = offset;
                beacon.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
                beacon.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.25f, 0.28f, 0.32f));
                Object.Destroy(beacon.GetComponent<Collider>());

                var laserLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                laserLine.name = "LaserLine";
                laserLine.transform.SetParent(portal.transform, false);
                laserLine.transform.localPosition = offset + new Vector3(0, 0.5f, 0);
                laserLine.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
                Color laserColor = new Color(energyColor.r * 1.5f, energyColor.g * 1.5f, energyColor.b * 1.5f, 0.4f);
                laserLine.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(laserColor);
                Object.Destroy(laserLine.GetComponent<Collider>());
            }

            var signObj = new GameObject("PortalSign");
            signObj.transform.SetParent(portal.transform, false);
            signObj.transform.localPosition = new Vector3(0, 1.8f, 0);

            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = label;
            signTmp.fontSize = 3.2f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;
            signObj.AddComponent<BillboardText>();

            var signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBg.name = "SignBg";
            signBg.transform.SetParent(signObj.transform, false);
            signBg.transform.localPosition = new Vector3(0, 0, 0.01f);
            signBg.transform.localScale = new Vector3(1.6f, 0.45f, 0.005f);
            Color glassColor = new Color(energyColor.r * 0.2f, energyColor.g * 0.2f, energyColor.b * 0.2f, 0.45f);
            signBg.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(glassColor);
            Object.Destroy(signBg.GetComponent<Collider>());

            var neonFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonFrame.name = "SignNeonFrame";
            neonFrame.transform.SetParent(signObj.transform, false);
            neonFrame.transform.localPosition = new Vector3(0, 0, 0.005f);
            neonFrame.transform.localScale = new Vector3(1.62f, 0.47f, 0.002f);
            neonFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(energyColor * 0.8f);
            Object.Destroy(neonFrame.GetComponent<Collider>());
        }

        public static GameObject CreateFlat(string name, Vector3 pos, Vector2 size, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            obj.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            obj.isStatic = true;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }

        public static GameObject CreateCircle(string name, Vector3 pos, float radius, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(radius * 2, 0.001f, radius * 2);
            obj.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(color);
            obj.isStatic = true;
            Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }
    }

    }
