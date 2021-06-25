using HeathenEngineering.Scriptable;
using HeathenEngineering.SteamApi.Networking;
using System;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// An example of creating a custom complex variable reference.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A variable reference is a class derived from <see cref="VariableReference{T}"/>.
    /// These types can be used in place of T type they imploy and can then act as a standard constant field or as a variable field.
    /// A constant field is a field such as you would get with a primative type such as float and in the Unity Inspector you can simply type in its value.
    /// A variable field takes a reference to a ScriptableObject of the base type allowing you to reference a float you have defined in your asset folders.
    /// </para>
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <para>
    /// <see cref="HeathenGameServerBrowser"/> is the base T of this reference. It is a mono behaviour so the primative form of a reference to it is a GameObject reference.
    /// In this example we show how to create a variable reference to the same T type such that you can use a direct in scene reference as you normally would or a reference to a ScriptableVariable representaiton of it.
    /// </para>
    /// </description>
    /// <code>
    /// public HeathenGameServerBrowser primativeReference;
    /// public GameServerBrowserReference variableReference;
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    [Serializable]
    public class GameServerBrowserReference : VariableReference<HeathenGameServerBrowser>
    {
        /// <summary>
        /// A reference to the variable representation of the base type
        /// </summary>
        public ServerBrowserVariable Variable;

        public override IDataVariable<HeathenGameServerBrowser> m_variable => Variable;

        /// <summary>
        /// Creates a new reference prepopulated with the input value
        /// </summary>
        /// <param name="value"></param>
        public GameServerBrowserReference(HeathenGameServerBrowser value) : base(value)
        { }

        /// <summary>
        /// Implicitly convert the reference type into the base type
        /// </summary>
        /// <param name="reference"></param>
        public static implicit operator HeathenGameServerBrowser(GameServerBrowserReference reference)
        {
            return reference.Value;
        }
    }
}
