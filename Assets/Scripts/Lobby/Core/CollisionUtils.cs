using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Utility chung cho kiểm tra va chạm với chướng ngại vật.
    /// Thay thế code duplicate trong PlayerController, NPCController, FakePlayerController.
    /// </summary>
    public static class CollisionUtils
    {
        /// <summary>
        /// Kiểm tra một vị trí có hợp lệ (không đụng chướng ngại vật) không.
        /// Bỏ qua collider của chính đối tượng và trigger colliders.
        /// </summary>
        /// <param name="pos">Vị trí cần kiểm tra</param>
        /// <param name="selfRoot">Root transform của đối tượng cần bỏ qua (tránh tự detect chính mình)</param>
        /// <param name="radius">Bán kính kiểm tra (mặc định 0.45f)</param>
        public static bool IsValidPosition(Vector3 pos, Transform selfRoot, float radius = 0.45f)
        {
            Collider[] hits = Physics.OverlapSphere(pos + Vector3.up * 0.5f, radius);
            foreach (var hit in hits)
            {
                if (hit.transform.root == selfRoot) continue;
                if (hit.isTrigger) continue;

                if (IsObstacle(hit.gameObject.name))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Kiểm tra tên object có phải chướng ngại vật không.
        /// Một chỗ duy nhất để thêm obstacle type mới.
        /// </summary>
        public static bool IsObstacle(string objectName)
        {
            return objectName.Contains("Collider") ||
                   objectName.Contains("Obstacle") ||
                   objectName.Contains("Walls") ||
                   objectName.Contains("Tree") ||
                   objectName.Contains("Post") ||
                   objectName.Contains("Picket") ||
                   objectName.Contains("Seat") ||
                   objectName.Contains("Base") ||
                   objectName.Contains("Pillar") ||
                   objectName.Contains("Bowl") ||
                   objectName.Contains("Bench") ||
                   objectName.Contains("Fountain") ||
                   objectName.Contains("Fence") ||
                   objectName.Contains("House") ||
                   objectName.Contains("Shop");
        }
    }
}
