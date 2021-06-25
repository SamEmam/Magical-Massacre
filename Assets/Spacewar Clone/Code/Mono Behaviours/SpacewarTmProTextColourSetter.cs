using HeathenEngineering.Scriptable;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.Spacewar
{
    public class SpacewarTmProTextColourSetter : MonoBehaviour
    {
        public ColorReference Value = new ColorReference(new Color(50 / 255, 50 / 255, 50 / 255));
        public List<TMPro.TextMeshProUGUI> Texts = new List<TMPro.TextMeshProUGUI>();

        private void OnEnable()
        {
            Refresh();
        }


        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (Texts == null)
                return;

            foreach (var text in Texts)
            {
                if (text != null)
                {
                    text.color = Value.Value;
                }
            }
        }

    }
}
