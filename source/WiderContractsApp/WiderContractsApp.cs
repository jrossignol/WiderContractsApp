using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace WiderContractsApp
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class WiderContractsApp : MonoBehaviour
    {
        private static GenericAppFrame contractsFrame = null;
        private static GenericAppFrame engineerFrame = null;

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
            // Check that we have a UI camera to attach to
            else if (ScreenSafeUI.referenceCam != null &&
                ScreenSafeUI.referenceCam.gameObject != null)
            {
                WiderContractsApp component = ScreenSafeUI.referenceCam.gameObject.GetComponent<WiderContractsApp>();
                if (component == null)
                {
                    ScreenSafeUI.referenceCam.gameObject.AddComponent<WiderContractsApp>();

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
            // Try to find the cascading list in the contracts window.  Note that we may pick up
            // the ones from the Engineer's report in the VAB/SPH instead.
            if (contractsFrame == null)
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

            if (contractsFrame != null)
            {
                // Set the background images to their new width
                if (contractsFrame.gfxBg.width < 200)
                {
                    // Set the widths of graphics/buttons
                    contractsFrame.gfxBg.width *= RESIZE_FACTOR;
                    contractsFrame.gfxHeader.width *= RESIZE_FACTOR;
                    contractsFrame.gfxFooter.width *= RESIZE_FACTOR;
                    contractsFrame.hoverComponent.width *= RESIZE_FACTOR;

                    // Don't limit max height
                    contractsFrame.maxHeight = Screen.height;

                    // Set the default size to something reasonable
                    int oldMin = contractsFrame.minHeight;
                    contractsFrame.minHeight = (int)(contractsFrame.minHeight * 2.5);
                    contractsFrame.UpdateDraggingBounds(contractsFrame.minHeight, -contractsFrame.minHeight);
                    contractsFrame.minHeight = oldMin;

                    // Apply changes
                    contractsFrame.Reposition();
                    contractsFrame.gfxHeader.SetSize(contractsFrame.gfxHeader.width, contractsFrame.gfxHeader.height);
                    contractsFrame.gfxBg.SetSize(contractsFrame.gfxBg.width, contractsFrame.gfxBg.height);
                    contractsFrame.gfxFooter.SetSize(contractsFrame.gfxFooter.width, contractsFrame.gfxFooter.height);
                }

                // Deal with the list of contracts
                UIScrollList list = contractsFrame.scrollList;
                if (list != null)
                {
                    bool heightChanged = false;

                    // Set the viewable area
                    if (list.viewableArea.x < 200)
                    {
                        float shiftSize = list.viewableArea.x * (RESIZE_FACTOR - 1.0f) / 2.0f;
                        list.SetupCameraAndSizes();
                        list.viewableArea.x *= RESIZE_FACTOR;
                        list.transform.Translate(new Vector3(shiftSize, 0.0f, 0.0f));

                        // This should work - but doesn't!  It offsets the list of contracts to
                        // the right by the amount we increased it by.  Need to figure out how to
                        // fix the clipping rectangle without that side effect.
                        list.SetViewableAreaPixelDimensions(list.renderCamera, (int)list.viewableArea.x, (int)list.viewableArea.y);
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        UIListItemContainer listObject = (UIListItemContainer)list.GetItem(i);

                        // Buttons for the contract header and text background
                        BTButton btn = (listObject.GetElement("bg") ?? listObject.GetElement("button")) as BTButton;
                        if (btn != null)
                        {
                            // Widen the button
                            if (btn.width < 200)
                            {
                                btn.SetSize(btn.width * RESIZE_FACTOR, btn.height);
                            }

                            // Find the associated text
                            SpriteTextRich richText = listObject.GetRichTextElement(btn.name == "bg" ? "keyRich" : "labelRich");
                            if (richText != null)
                            {
                                if (richText.maxWidth < 200)
                                {
                                    richText.maxWidth *= RESIZE_FACTOR;
                                    richText.UpdateMesh();
                                }

                                // Resize in the y dimension to match text
                                float h = (richText.name == "labelRich" ? 9.0f : 5.0f) - richText.BottomRight.y;
                                if (btn.height != h)
                                {
                                    heightChanged = true;
                                    btn.SetSize(btn.width, h);
                                }
                            }
                        }
                    }

                    // Fix up any heights we may have changed
                    if (heightChanged)
                    {
                        list.RepositionItems();
                    }
                }
            }

            // Engineer's report gets messed up (ends up too tall) if we resize the contracts window before it is displayed
            if (engineerFrame != null)
            {
                // Do a little hackery by using maxHeight to store "state"
                if (engineerFrame.maxHeight == 476)
                {
                    // Set the "state" as handled
                    engineerFrame.maxHeight = 477;

                    // Need to reset the height to the proper one on first invokation
                    engineerFrame.minHeight = 176;
                    engineerFrame.gfxBg.height = 176;
                    engineerFrame.UpdateDraggingBounds(engineerFrame.minHeight, -engineerFrame.minHeight);
                    engineerFrame.Reposition();
                }
            }
        }
    }
}
