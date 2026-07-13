# Project 2 — Ranger City Lobby (Unity)

## Cách dùng

### Bước 1: Copy lên server
Copy thư mục `project2` lên server Linux:
```bash
scp -r project2 hung@100.89.39.103:/home/hung/
```

### Bước 2: Mở bằng Unity Hub
1. Mở Unity Hub trên server (qua TeamViewer)
2. Add project: `/home/hung/project2`
3. Chọn Editor version: `6000.0.23f1`
4. Mở project

### Bước 3: Setup Scene
1. **File > New Scene > Basic (Built-in)**
2. Xóa Camera và Light mặc định
3. Tạo Empty GameObject → đặt tên `LobbyManager`
4. Kéo thả script `LobbySetup.cs` vào LobbyManager
5. **Window > AI > Navigation** → click **Bake** NavMesh
6. Nhấn **▶ Play**!

### Bước 4: Chơi thử
- **WASD / Arrow keys**: Di chuyển
- **Click chuột**: Click-to-move
- **Space**: Đấm
- Đi gần NPC/Player khác → hiện nút 💬 Talk / 👊 Punch
- Đấm → đối tượng bay ra + hiện mặt sưng 😵

## Cấu trúc project

```
project2/
├── ProjectSettings/          # Unity project settings
│   ├── ProjectSettings.asset
│   ├── ProjectVersion.txt    # Unity 6000.0.23f1
│   ├── GraphicsSettings.asset
│   └── InputManager.asset
├── Packages/
│   └── manifest.json         # Dependencies (TMP, InputSystem)
└── Assets/
    └── Scripts/
        └── Lobby/
            ├── RangerCity.Lobby.asmdef
            ├── Interfaces.cs         # IInteractable, IPunchable
            ├── PlayerController.cs   # Nhân vật chính
            ├── NPCController.cs      # NPC (3 con)
            ├── FakePlayerController.cs # Fake players (6 con)
            ├── LobbyCamera.cs        # Camera follow mượt
            ├── LobbyUI.cs            # Talk/Punch buttons + Dialogue
            ├── LobbySetup.cs         # Auto-generate toàn bộ sảnh
            ├── SwollenFaceEffect.cs  # Mặt sưng 😵 khi bị đấm
            └── BillboardText.cs      # Name tag hướng camera
```

## Tính năng

| # | Tính năng | Trạng thái |
|---|-----------|-----------|
| 1 | Sảnh chờ 3D với cây cối, đường đi, đài phun nước | ✅ |
| 2 | Nhân vật chính di chuyển (WASD + click) | ✅ |
| 3 | Camera follow mượt | ✅ |
| 4 | 3 NPC đi lại + nói chuyện | ✅ |
| 5 | 6 Fake players (wander/patrol/idle) | ✅ |
| 6 | Proximity detection → hiện nút Talk/Punch | ✅ |
| 7 | Dialogue box với typing effect | ✅ |
| 8 | Đấm → knockback + mặt sưng 😵 + sao bay | ✅ |
| 9 | Nhân vật bé xíu kiểu farm game | ✅ |

## Kiến trúc

**Đơn giản, không framework phức tạp:**
- Pure MonoBehaviour — không VContainer, không R3
- NavMeshAgent cho di chuyển
- Collider overlap cho proximity detection
- TextMeshPro cho UI text
- Reflection helper cho runtime setup (SetPrivateField)

**Lý do giản lược:** Project 1 dùng quá nhiều framework → nhiều lỗi.
Project 2 ưu tiên **chạy được** trước, thêm pattern sau.
