namespace RangerCity.Lobby
{
    /// <summary>
    /// Interface cho mọi đối tượng có thể tương tác (NPC, Player khác).
    /// </summary>
    public interface IInteractable
    {
        string DisplayName { get; }
        string AvatarEmoji { get; }
        bool CanTalk { get; }
        bool CanBePunched { get; }
        InteractableType Type { get; }

        /// <summary>
        /// Lấy danh sách dialogue lines khi nói chuyện.
        /// </summary>
        string[] GetDialogueLines();
    }

    /// <summary>
    /// Interface cho đối tượng có thể bị đấm.
    /// </summary>
    public interface IPunchable
    {
        /// <summary>
        /// Nhận cú đấm với hướng knockback và lực.
        /// </summary>
        void ReceivePunch(UnityEngine.Vector3 knockDirection, float force);
    }

    public enum InteractableType
    {
        NPC,
        Player
    }
}
