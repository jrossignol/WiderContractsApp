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
        private static GenericAppFrame appFrame = null;

        const float RESIZE_FACTOR = 1.6f;
        void Start()
        {
            if (ScreenSafeUI.referenceCam != null &&
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
            if (appFrame == null || !appFrame.gameObject.activeSelf)
            {
                appFrame = UnityEngine.Object.FindObjectOfType<GenericAppFrame>();
            }

            if (appFrame != null && appFrame.header.text == "Contracts")
            {
                // Set the background images to their new width
                if (appFrame.gfxBg.width < 200)
                {
                    // Set the widths of graphics/buttons
                    appFrame.gfxBg.width *= RESIZE_FACTOR;
                    appFrame.gfxHeader.width *= RESIZE_FACTOR;
                    appFrame.gfxFooter.width *= RESIZE_FACTOR;
                    appFrame.hoverComponent.width *= RESIZE_FACTOR;

                    // Don't limit max height
                    appFrame.maxHeight = Screen.height;

                    // Set the default size to something reasonable
                    int oldMin = appFrame.minHeight;
                    appFrame.minHeight = (int)(appFrame.minHeight * 2.5);
                    appFrame.UpdateDraggingBounds(appFrame.minHeight, -appFrame.minHeight);
                    appFrame.minHeight = oldMin;

                    // Apply changes
                    appFrame.Reposition();
                    appFrame.gfxHeader.SetSize(appFrame.gfxHeader.width, appFrame.gfxHeader.height);
                    appFrame.gfxBg.SetSize(appFrame.gfxBg.width, appFrame.gfxBg.height);
                    appFrame.gfxFooter.SetSize(appFrame.gfxFooter.width, appFrame.gfxFooter.height);
                }

                // Deal with the list of contracts
                UIScrollList list = appFrame.scrollList;
                if (list != null)
                {
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
                        BTButton[] btns = listObject.GetComponentsInChildren<BTButton>();
                        foreach (BTButton btn in btns)
                        {
                            if (btn != null)
                            {
                                // Widen the button
                                if (btn.width < 200)
                                {
                                    btn.SetSize(btn.width * RESIZE_FACTOR, btn.height);
                                }

                                // Find the associated text
                                SpriteTextRich richText = listObject.GetComponentInChildren<SpriteTextRich>();
                                if (richText != null)
                                {
                                    if (richText.maxWidth < 200)
                                    {
                                        richText.maxWidth *= RESIZE_FACTOR;
                                        richText.UpdateMesh();
                                    }

                                    // Resize in the y dimension to match text
                                    btn.SetSize(btn.width, (richText.name == "labelRich" ? 9.0f : 5.0f) - richText.BottomRight.y);
                                }
                            }
                        }
                    }

                    // Fix up any heights we may have changed
                    list.RepositionItems();
                }
            }
            // Engineer's report gets messed up (ends up too tall) if we resize the contracts window before it is displayed
            else if (appFrame != null && appFrame.header.text == "Engineer's Report")
            {
                // Do a little hackery by using maxHeight to store "state"
                if (appFrame.maxHeight == 476)
                {
                    // Set the "state" as handled
                    appFrame.maxHeight = 477;

                    // Need to reset the height to the proper one on first invokation
                    appFrame.minHeight = 176;
                    appFrame.gfxBg.height = 176;
                    appFrame.UpdateDraggingBounds(appFrame.minHeight, -appFrame.minHeight);
                    appFrame.Reposition();
                }
            }
        }
    }
}
