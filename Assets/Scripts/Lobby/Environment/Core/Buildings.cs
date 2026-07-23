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
        public static void CreateBuildings()
        {
            // ── 1. GÓC ĐÔNG BẮC (Top-Right): CỬA HÀNG TRANG PHỤC ──
            var outfitShop = CreateBuilding("OutfitShop", new Vector3(9, 0, 8),
                new Color(0.95f, 0.88f, 0.98f),
                new Color(0.6f, 0.25f, 0.85f),
                new Vector3(3.8f, 2.9f, 3.0f),
                "CỬA HÀNG TRANG PHỤC", new Color(0.85f, 0.35f, 0.9f));
            CreateFashionDisplayProps(outfitShop);

            // ── 2. GÓC TÂY BẮC (Top-Left): CỬA HÀNG PHƯƠNG TIỆN ──
            var vehicleShop = CreateBuilding("VehicleShop", new Vector3(-9, 0, 8),
                new Color(0.88f, 0.94f, 0.98f),
                new Color(0.15f, 0.45f, 0.85f),
                new Vector3(3.8f, 2.9f, 3.0f),
                "CỬA HÀNG PHƯƠNG TIỆN", new Color(0.1f, 0.7f, 0.95f));
            CreateVehicleDisplayProps(vehicleShop);

            // ── 3. GÓC TÂY NAM (Bottom-Left): SẮP RA MẮT ──
            CreateBuilding("FutureShop1", new Vector3(-9, 0, -8),
                new Color(0.9f, 0.92f, 0.94f),
                new Color(0.35f, 0.4f, 0.48f),
                new Vector3(3.8f, 2.9f, 3.0f),
                "SẮP RA MẮT", new Color(0.45f, 0.5f, 0.58f));

            // ── 4. GÓC ĐÔNG NAM (Bottom-Right): KHU THƯ GIÃN ──
            CreateBuilding("LoungeShop", new Vector3(9, 0, -8),
                new Color(0.94f, 0.96f, 0.88f),
                new Color(0.25f, 0.65f, 0.35f),
                new Vector3(3.8f, 2.9f, 3.0f),
                "KHU THƯ GIÃN", new Color(0.35f, 0.8f, 0.45f));
        }

        private static void CreateFashionDisplayProps(GameObject parent)
        {
            // Trưng bày 3 Bệ Ma-nơ-canh thời trang 3D cao cấp phía trước Cửa hàng trang phục
            Vector3[] positions = { new Vector3(-1.3f, 0f, -1.8f), new Vector3(0f, 0f, -2.1f), new Vector3(1.3f, 0f, -1.8f) };
            Color[] pedestalGlow = { new Color(0.85f, 0.35f, 0.9f), new Color(0.2f, 0.85f, 1f), new Color(1f, 0.84f, 0f) };
            int[] genders = { 0, 1, 0 };
            int[] styles = { 1, 0, 4 }; // 1=Luxury Suit, 0=Chic Pink, 4=Biker Leather Jacket

            for (int i = 0; i < 3; i++)
            {
                var pedestal = new GameObject($"FashionPedestal_{i}");
                pedestal.transform.SetParent(parent.transform, false);
                pedestal.transform.localPosition = positions[i];

                // Glowing Pedestal Base Platform
                var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                basePlate.name = "GlowingBase";
                basePlate.transform.SetParent(pedestal.transform, false);
                basePlate.transform.localPosition = new Vector3(0, 0.05f, 0);
                basePlate.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
                basePlate.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(pedestalGlow[i]);
                Object.Destroy(basePlate.GetComponent<Collider>());

                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "BaseRing";
                ring.transform.SetParent(pedestal.transform, false);
                ring.transform.localPosition = new Vector3(0, 0.02f, 0);
                ring.transform.localScale = new Vector3(0.76f, 0.02f, 0.76f);
                ring.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.12f, 0.14f, 0.2f));
                Object.Destroy(ring.GetComponent<Collider>());

                // 3D Fashion Character Model Mannequin
                var charModel = CharacterVisuals.CreateCharacterTopDown($"Mannequin_{i}",
                    pedestalGlow[i], new Color(0.95f, 0.9f, 0.88f));
                charModel.transform.SetParent(pedestal.transform, false);
                charModel.transform.localPosition = Vector3.zero;

                Color hairCol = i == 0 ? new Color(0.15f, 0.12f, 0.1f) : (i == 1 ? new Color(0.92f, 0.78f, 0.4f) : new Color(0.45f, 0.28f, 0.12f));
                Color outfitCol = i == 0 ? new Color(0.18f, 0.2f, 0.32f) : (i == 1 ? new Color(0.94f, 0.45f, 0.65f) : new Color(0.15f, 0.15f, 0.18f));
                Color pantsCol = i == 0 ? new Color(0.15f, 0.18f, 0.25f) : (i == 1 ? new Color(0.95f, 0.95f, 0.98f) : new Color(0.35f, 0.42f, 0.35f));

                CharacterVisuals.ApplyCustomization(charModel, genders[i], i == 1 ? 5 : i, hairCol, styles[i], outfitCol, i == 2 ? 2 : i, pantsCol);
            }
        }

        private static void CreateVehicleDisplayProps(GameObject parent)
        {
            // Trưng bày Xe Scooter / Go-Kart Thể Thao 3D phía trước cửa hàng phương tiện
            var vehicle = new GameObject("DisplayScooter3D");
            vehicle.transform.SetParent(parent.transform, false);
            vehicle.transform.localPosition = new Vector3(0f, 0f, -1.85f);
            vehicle.transform.localRotation = Quaternion.Euler(0, 25f, 0);

            // Chassis Base
            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis";
            chassis.transform.SetParent(vehicle.transform, false);
            chassis.transform.localPosition = new Vector3(0, 0.18f, 0);
            chassis.transform.localScale = new Vector3(0.48f, 0.12f, 1.1f);
            chassis.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.95f, 0.25f, 0.15f));
            Object.Destroy(chassis.GetComponent<Collider>());

            // Seat
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "ScooterSeat";
            seat.transform.SetParent(vehicle.transform, false);
            seat.transform.localPosition = new Vector3(0, 0.32f, -0.15f);
            seat.transform.localScale = new Vector3(0.36f, 0.14f, 0.48f);
            seat.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.15f, 0.15f, 0.18f));
            Object.Destroy(seat.GetComponent<Collider>());

            // Front Handlebar Post & Bar
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "HandlePost";
            post.transform.SetParent(vehicle.transform, false);
            post.transform.localPosition = new Vector3(0, 0.48f, 0.35f);
            post.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            post.transform.localRotation = Quaternion.Euler(-12f, 0, 0);
            post.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(Color.white);
            Object.Destroy(post.GetComponent<Collider>());

            var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.name = "HandleBar";
            bar.transform.SetParent(post.transform, false);
            bar.transform.localPosition = new Vector3(0, 0.95f, 0);
            bar.transform.localScale = new Vector3(0.03f, 0.42f, 0.03f);
            bar.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            bar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.2f, 0.2f, 0.2f));
            Object.Destroy(bar.GetComponent<Collider>());

            // Glowing Headlight
            var light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            light.name = "Headlight";
            light.transform.SetParent(post.transform, false);
            light.transform.localPosition = new Vector3(0, 0.72f, 0.1f);
            light.transform.localScale = Vector3.one * 0.16f;
            light.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1f, 0.95f, 0.5f));
            Object.Destroy(light.GetComponent<Collider>());

            // 2 Wheels (Front & Back)
            Vector3[] wheelPos = { new Vector3(0, 0.15f, 0.42f), new Vector3(0, 0.15f, -0.42f) };
            for (int i = 0; i < 2; i++)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel_{i}";
                wheel.transform.SetParent(vehicle.transform, false);
                wheel.transform.localPosition = wheelPos[i];
                wheel.transform.localScale = new Vector3(0.32f, 0.06f, 0.32f);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                wheel.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.12f, 0.12f, 0.14f));
                Object.Destroy(wheel.GetComponent<Collider>());
            }
        }

        public static GameObject CreateBuilding(string name, Vector3 pos, Color wallColor, Color roofColor, Vector3 size, string signText, Color stripeColor)
        {
            var building = new GameObject(name);

            // Foundation Plinth
            var plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "Plinth";
            plinth.transform.SetParent(building.transform, false);
            plinth.transform.localPosition = new Vector3(0, 0.1f, 0);
            plinth.transform.localScale = new Vector3(size.x + 0.3f, 0.2f, size.z + 0.3f);
            plinth.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.45f, 0.42f, 0.4f));
            Object.Destroy(plinth.GetComponent<Collider>());

            // Main Walls
            var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Walls_Collider";
            walls.transform.SetParent(building.transform, false);
            walls.transform.localPosition = new Vector3(0, size.y * 0.5f + 0.1f, 0);
            walls.transform.localScale = size;
            walls.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(wallColor);

            // 2-Tiered Roof with Overhangs
            float roofThickness = 0.18f;
            float roofWidth = size.x * 0.62f;
            float roofLen = size.z + 0.5f;

            var roofLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofLeft.name = "RoofLeft";
            roofLeft.transform.SetParent(building.transform, false);
            roofLeft.transform.localPosition = new Vector3(-size.x * 0.26f, size.y + 0.42f, 0);
            roofLeft.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofLeft.transform.localRotation = Quaternion.Euler(0, 0, -25f);
            roofLeft.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(roofColor);
            Object.Destroy(roofLeft.GetComponent<Collider>());

            var roofRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofRight.name = "RoofRight";
            roofRight.transform.SetParent(building.transform, false);
            roofRight.transform.localPosition = new Vector3(size.x * 0.26f, size.y + 0.42f, 0);
            roofRight.transform.localScale = new Vector3(roofWidth, roofThickness, roofLen);
            roofRight.transform.localRotation = Quaternion.Euler(0, 0, 25f);
            roofRight.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(roofColor);
            Object.Destroy(roofRight.GetComponent<Collider>());

            // Corner Wooden Beams
            Color woodDark = new Color(0.32f, 0.2f, 0.12f);
            Vector3[] cornerOffsets = {
                new(-size.x * 0.5f, size.y * 0.5f + 0.1f, -size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f + 0.1f, -size.z * 0.5f),
                new(-size.x * 0.5f, size.y * 0.5f + 0.1f, size.z * 0.5f),
                new(size.x * 0.5f, size.y * 0.5f + 0.1f, size.z * 0.5f)
            };
            for (int i = 0; i < cornerOffsets.Length; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = $"CornerPillar_{i}";
                pillar.transform.SetParent(building.transform, false);
                pillar.transform.localPosition = cornerOffsets[i];
                pillar.transform.localScale = new Vector3(0.18f, size.y, 0.18f);
                pillar.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(pillar.GetComponent<Collider>());
            }

            // Ornate Wooden Door
            var doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorFrame.name = "DoorFrame";
            doorFrame.transform.SetParent(building.transform, false);
            doorFrame.transform.localPosition = new Vector3(0, 0.6f, -size.z * 0.5f - 0.02f);
            doorFrame.transform.localScale = new Vector3(0.72f, 1.2f, 0.04f);
            doorFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
            Object.Destroy(doorFrame.GetComponent<Collider>());

            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "DoorPanel";
            door.transform.SetParent(building.transform, false);
            door.transform.localPosition = new Vector3(0, 0.58f, -size.z * 0.5f - 0.03f);
            door.transform.localScale = new Vector3(0.62f, 1.12f, 0.03f);
            door.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.48f, 0.32f, 0.18f));
            Object.Destroy(door.GetComponent<Collider>());

            // Brass Door Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "DoorHandle";
            handle.transform.SetParent(building.transform, false);
            handle.transform.localPosition = new Vector3(0.22f, 0.58f, -size.z * 0.5f - 0.05f);
            handle.transform.localScale = Vector3.one * 0.08f;
            handle.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.95f, 0.8f, 0.25f));
            Object.Destroy(handle.GetComponent<Collider>());

            // Double Windows with Flower Boxes
            for (int w = 0; w < 2; w++)
            {
                float xOff = w == 0 ? -size.x * 0.28f : size.x * 0.28f;

                var winFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winFrame.name = $"WindowFrame_{w}";
                winFrame.transform.SetParent(building.transform, false);
                winFrame.transform.localPosition = new Vector3(xOff, 1.45f, -size.z * 0.5f - 0.02f);
                winFrame.transform.localScale = new Vector3(0.65f, 0.65f, 0.04f);
                winFrame.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(winFrame.GetComponent<Collider>());

                var winGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winGlass.name = $"WindowGlass_{w}";
                winGlass.transform.SetParent(winFrame.transform, false);
                winGlass.transform.localPosition = new Vector3(0, 0, -0.1f);
                winGlass.transform.localScale = new Vector3(0.85f, 0.85f, 1.1f);
                winGlass.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1.0f, 0.92f, 0.6f));
                Object.Destroy(winGlass.GetComponent<Collider>());

                // Flower Box under window
                var flowerBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flowerBox.name = $"FlowerBox_{w}";
                flowerBox.transform.SetParent(building.transform, false);
                flowerBox.transform.localPosition = new Vector3(xOff, 1.05f, -size.z * 0.5f - 0.08f);
                flowerBox.transform.localScale = new Vector3(0.75f, 0.16f, 0.22f);
                flowerBox.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(woodDark);
                Object.Destroy(flowerBox.GetComponent<Collider>());

                Color[] fCols = { new Color(1f, 0.2f, 0.4f), new Color(1f, 0.8f, 0.1f), new Color(0.3f, 0.85f, 1f) };
                for (int f = 0; f < 3; f++)
                {
                    var fl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    fl.name = $"Flower_{f}";
                    fl.transform.SetParent(flowerBox.transform, false);
                    fl.transform.localPosition = new Vector3(-0.25f + f * 0.25f, 0.55f, 0);
                    fl.transform.localScale = Vector3.one * 0.75f;
                    fl.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(fCols[f % fCols.Length]);
                    Object.Destroy(fl.GetComponent<Collider>());
                }
            }

            // Striped Canopy Awning over Entrance
            var awning = new GameObject("Awning");
            awning.transform.SetParent(building.transform, false);
            awning.transform.localPosition = new Vector3(0, size.y - 0.1f, -size.z * 0.5f - 0.45f);
            awning.transform.localRotation = Quaternion.Euler(-18f, 0, 0);

            float stripeWidth = (size.x + 0.4f) / 6f;
            for (int i = 0; i < 6; i++)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = $"Stripe_{i}";
                stripe.transform.SetParent(awning.transform, false);
                stripe.transform.localPosition = new Vector3(-size.x * 0.5f - 0.1f + (i + 0.5f) * stripeWidth, 0, 0);
                stripe.transform.localScale = new Vector3(stripeWidth, 0.06f, 0.9f);
                Color c = (i % 2 == 0) ? new Color(0.96f, 0.94f, 0.9f) : stripeColor;
                stripe.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(c);
                Object.Destroy(stripe.GetComponent<Collider>());
            }

            // Chimney with Puffy Smoke
            var chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chimney.name = "Chimney";
            chimney.transform.SetParent(building.transform, false);
            chimney.transform.localPosition = new Vector3(size.x * 0.35f, size.y + 0.9f, -size.z * 0.2f);
            chimney.transform.localScale = new Vector3(0.3f, 0.7f, 0.3f);
            chimney.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.38f, 0.35f, 0.32f));
            Object.Destroy(chimney.GetComponent<Collider>());

            for (int i = 0; i < 3; i++)
            {
                var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = $"Smoke_{i}";
                smoke.transform.SetParent(building.transform, false);
                smoke.transform.localPosition = new Vector3(size.x * 0.35f + i * 0.08f, size.y + 1.4f + i * 0.18f, -size.z * 0.2f);
                smoke.transform.localScale = Vector3.one * (0.22f + i * 0.1f);
                smoke.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.92f, 0.95f, 0.7f));
                Object.Destroy(smoke.GetComponent<Collider>());
            }

            // Shop Signboard (Clean Floating 3D Text)
            var signObj = new GameObject("Sign");
            signObj.transform.SetParent(building.transform, false);
            signObj.transform.localPosition = new Vector3(0, size.y + 0.8f, -size.z * 0.5f - 0.1f);
            var signTmp = signObj.AddComponent<TextMeshPro>();
            signTmp.text = $"<b>{signText}</b>";
            signTmp.fontSize = 4.5f;
            signTmp.alignment = TextAlignmentOptions.Center;
            signTmp.color = Color.white;
            signObj.AddComponent<BillboardText>();

            building.transform.position = pos;
            return building;
        }


    }
    }
