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
        const float RESIZE_FACTOR = 1.6f;
        void Start()
        {
            if (ScreenSafeUI.referenceCam.gameObject.GetComponent<WiderContractsApp>() == null)
            {
                ScreenSafeUI.referenceCam.gameObject.AddComponent<WiderContractsApp>();

                // Destroy this object - otherwise we'll have two
                Destroy(this);
            }
        }

        public void OnPreCull()
        {
            if (ContractsApp.Instance != null)
            {
                // Set the background images to their new width
                if (ContractsApp.Instance.gfxBg.width < 200)
                {
                    ContractsApp.Instance.gfxBg.width *= RESIZE_FACTOR;
                    ContractsApp.Instance.gfxHeader.width *= RESIZE_FACTOR;
                    ContractsApp.Instance.gfxFooter.width *= RESIZE_FACTOR;
                    ContractsApp.Instance.hoverComponent.width *= RESIZE_FACTOR;
                }

                // Deal with the list of contracts
                UIScrollList list = ContractsApp.Instance.cascadingList.cascadingList;
                if (list != null)
                {
                    // Set the minimum height
                    if (ContractsApp.Instance.minHeight < 200)
                    {
                        ContractsApp.Instance.minHeight = (int)(ContractsApp.Instance.minHeight * 2.5);
                        ContractsApp.Instance.maxHeight = Screen.height;
                    }

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
        }
    }
}
