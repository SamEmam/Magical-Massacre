using HeathenEngineering.Events;
using HeathenEngineering.SteamApi.PlayerServices;
using UnityEngine;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// A custom <see cref="ItemGeneratorDefinition"/> unique to Spacewar
    /// </summary>
    /// <remarks>
    /// The main reason to create a unique item generator defintion is so that you can add refrences to additional data your specific game might want abotu any given item.
    /// In this case we have no special requirements at this time but can easily add data later.
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Spacewar/Inventory Item Generator")]
    public class SpacewarInventoryItemGenerator : ItemGeneratorDefinition
    {
        //We dont need any visual aids for this one
    }
}
