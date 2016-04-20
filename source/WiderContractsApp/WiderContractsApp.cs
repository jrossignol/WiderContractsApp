using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP;
using KSP.UI;
using KSP.UI.Screens;

namespace WiderContractsApp
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class WiderContractsApp : MonoBehaviour
    {
        private GenericAppFrame contractsFrame = null;
        private GenericAppFrame engineerFrame = null;

        // Reflection fields we operate on
        static IEnumerable<FieldInfo> intFields = typeof(GenericAppFrame).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(mi => mi.FieldType == typeof(int));
        static FieldInfo widthField = intFields.First();
        static FieldInfo heightField = intFields.ElementAt(1);
        static FieldInfo transformField = typeof(GenericAppFrame).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(mi => mi.FieldType == typeof(RectTransform)).First();
        static FieldInfo uiListPrefabField = typeof(ContractsApp).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(mi => mi.FieldType == typeof(UIListItem)).First();
        static IEnumerable<FieldInfo> cascadingListFields = typeof(ContractsApp).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(mi => mi.FieldType == typeof(GenericCascadingList));
        static MethodInfo refreshMethod = typeof(ContractsApp).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(mi => mi.Name == "CreateContractsList").First();

        const float RESIZE_FACTOR = 1.6f;

        void Start()
        {
            // Check for the correct scenes
            if (HighLogic.LoadedScene != GameScenes.EDITOR &&
                HighLogic.LoadedScene != GameScenes.FLIGHT &&
                HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.TRACKSTATION)
            {
                Destroy(this);
            }
            // Check for the corect game mode
            else if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
            }
            // Check that we have a UI camera to attach to
            else if (UIMainCamera.Camera && UIMainCamera.Camera.gameObject)
            {
                WiderContractsApp component = UIMainCamera.Camera.gameObject.GetComponent<WiderContractsApp>();
                if (component == null)
                {
                    // Add to the UI camera so we get our PreCull call
                    UIMainCamera.Camera.gameObject.AddComponent<WiderContractsApp>();

                    // Destroy this object - otherwise we'll have two
                    Destroy(this);
                }
                else if (component != this)
                {
                    Destroy(this);
                }
            }
            else
            {
                Destroy(this);
            }
        }

        public void OnPreCull()
        {
            // Try to find the app frame for the contracts window.  Note that we may pick up
            // the ones from the Engineer's report in the VAB/SPH instead, so check by name.
            if (!contractsFrame)
            {
                // Check if this scene even has a contracts app
                IEnumerable<GenericAppFrame> frames = Resources.FindObjectsOfTypeAll<GenericAppFrame>();
                if (!frames.Any())
                {
                    Destroy(this);
                    return;
                }

                foreach (GenericAppFrame appFrame in frames)
                {
                    if (appFrame.header.text == "Contracts")
                    {
                        contractsFrame = appFrame;
                    }
                    else if (appFrame.header.text == "Engineer's Report")
                    {
                        engineerFrame = appFrame;
                    }
                }
            }

            if (contractsFrame)
            {
                Debug.Log("WiderContractsApp: Making adjustments to contract frame!");
                
                // Set the new width and height (old value * factor)
                int width = (int)(166 * RESIZE_FACTOR);
                int height = (int)(176 * 2.5);
                widthField.SetValue(contractsFrame, width);

                // Apply the changes
                RectTransform rectTransform = (RectTransform) transformField.GetValue(contractsFrame);
                rectTransform.sizeDelta = new Vector2((float)width, (float)height);

                // Remove the limit on max height (technically should be a little less than screen height, but close enough)
                contractsFrame.maxHeight = Screen.height;

                // Apply the changes
                contractsFrame.Reposition();

                if (ContractsApp.Instance != null)
                {
                    // Find the cascading list field
                    foreach (FieldInfo cascadingListField in cascadingListFields)
                    {
                        GenericCascadingList ccl = (GenericCascadingList)cascadingListField.GetValue(ContractsApp.Instance);
                        if (ccl != null)
                        {
                            // Set the body width (I think this is used for word wrap logic)
                            ccl.bodyTextStartWidth = (int)(166 * RESIZE_FACTOR - (166 - 151));

                            // Fix the prefab for the header - will apply to any newly added contracts
                            FixListItem(ccl.cascadeHeader);

                            // Fix the prefab for the body - used for contract notes
                            FixListItem(ccl.cascadeBody);
                        }
                    }

                    // Fix the prefab used for contract parameters
                    UIListItem uiListPrefab = (UIListItem)uiListPrefabField.GetValue(ContractsApp.Instance);
                    Text text = uiListPrefab.GetComponentsInChildren<Text>(true).FirstOrDefault();
                    if (text)
                    {
                        text.rectTransform.sizeDelta = new Vector2(166 * RESIZE_FACTOR - (166 - 163), 14.0f);
                    }

                    // Refresh the contract list, forces everything to get recreated
                    refreshMethod.Invoke(ContractsApp.Instance, new object[]{});

                    // No need to hang around, the changes will stick for the lifetime of the app
                    Destroy(this);
                }
            }
            else
            {
                Destroy(this);
            }

            // In the past we needed to do some stuff to prevent leakage to the Engineer's frame, but that's no longer needed.
            // Keeping this around in case we need to do something with the engineer report in the future.
            if (engineerFrame)
            {
            }
        }

        private void FixListItem(UIListItem item)
        {
            float width = 166 * RESIZE_FACTOR - (166 - 163);

            // Resize the item
            Text t = item.GetComponentsInChildren<Text>(true).FirstOrDefault();
            if (t)
            {
                t.rectTransform.sizeDelta = new Vector2(width, t.rectTransform.sizeDelta.y);
            }
        }
    }
}
