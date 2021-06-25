using HeathenEngineering.SteamApi.PlayerServices;
using UnityEngine;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// A custom <see cref="InventoryItemDefinition"/> unique to Spacewar
    /// </summary>
    /// <remarks>
    /// The main reason to create a unique item defintion is so that you can add refrences to additional data your specific game might want abotu any given item.
    /// In this case we have added a display name, description, item type and a sprite to serve as the icon for use in game.
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Spacewar/Inventory Item Definition")]
    public class SpacewarInventoryItem : InventoryItemDefinition
    {
        /// <summary>
        /// A simple helper so we can know what type of item this is without needing to assume based on name or ID
        /// </summary>
        public enum ItemTypes
        {
            decoration,
            weapon,
            special
        }

        /// <summary>
        /// An icon we can use in the game's UI to represent this item
        /// </summary>
        public Sprite uiIcon;
        /// <summary>
        /// The name of the item as we would present it to the player
        /// </summary>
        public string displayName;
        /// <summary>
        /// The description of the item as we would present it to the player
        /// </summary>
        public string description;
        /// <summary>
        /// The type of item this is
        /// </summary>
        public ItemTypes itemType;
    }
}
