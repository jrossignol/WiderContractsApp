using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP;

namespace WiderContractsApp
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ObsoleteMessage : MonoBehaviour
    {
        public void Start()
        {
            var ainfoV = Attribute.GetCustomAttribute(typeof(ObsoleteMessage).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            string title = "Wider Contracts App";
            string message = "The functionality of Wider Contracts App is now part of Contract Configurator (as of version 1.13.0).  " +
                "The mod in GameData/WiderContractsApp no longer does anything (other than displaying this message) and can be removed.";
            DialogGUIButton dialogOption = new DialogGUIButton("Okay", new Callback(DoNothing), true);
            PopupDialog.SpawnPopupDialog(new MultiOptionDialog(message, title, UISkinManager.GetSkin("default"), dialogOption), false, UISkinManager.GetSkin("default"));

            Destroy(this);
        }

        private void DoNothing() { }
    }
}
