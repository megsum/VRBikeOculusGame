using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace MedRoad.Utils
{
    /// <summary>
    /// A class to replace Unity's new GUI Text components with the old system. Attach this
    /// component to an object with a Text component. This script will instead render the text
    /// using OnGUI with much improved performance, especially for text that is updated often.
    /// </summary>
    public class DrawTextOnGUI : MonoBehaviour
    {
        // Constants to access corners returned by RectTransform.GetWorldCorners.
        private const int BOTTOM_LEFT = 0;
        private const int TOP_LEFT = 1;
        private const int TOP_RIGHT = 2;
        private const int BOTTOM_RIGHT = 3;

        private Canvas rootCanvas;

        private bool ready = false;
        private Text targetText;
        private Rect targetTextPosition;
        private GUIStyle targetTextStyle;

        void Start()
        {
            GameObject rootCanvasGO = FindRootCanvas(gameObject);
            if (rootCanvasGO != null)
                this.rootCanvas = rootCanvasGO.GetComponent<Canvas>();

            StartCoroutine(this.DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            // Wait for the root canvas to become enabled if it isn't already.
            while (this.rootCanvas != null && !this.rootCanvas.enabled)
                yield return new WaitForEndOfFrame();

            // Give Unity a few frames to position the UI elements before we get its position and
            // dimensions.
            for (int framesToWait = 3; framesToWait > 0; framesToWait--)
                yield return new WaitForEndOfFrame();

            this.ResetTargetText();
        }

        /// <summary>
        /// Find the target Text component on this object and recalculate its position, dimensions,
        /// and style. This method is performed automatically on Start but in some cases might need
        /// to be called manually, or if the layout changes.
        /// </summary>
        public void ResetTargetText()
        {
            targetText = gameObject.GetComponent<Text>();

            // From the target text get a Rect which we'll need for GUI.Label.
            Vector3[] corners = new Vector3[4];
            gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector2 dimensions = corners[TOP_RIGHT] - corners[BOTTOM_LEFT];
            targetTextPosition = new Rect(corners[TOP_LEFT].x, Screen.height - corners[TOP_LEFT].y - 1f, dimensions.x, dimensions.y);

            // From the target text get a GUIStyle which we'll need for GUI.Label. Not all
            // properties have a direct mapping.
            targetTextStyle = new GUIStyle();
            targetTextStyle.font = targetText.font;
            targetTextStyle.fontStyle = targetText.fontStyle;
            targetTextStyle.fontSize = targetText.fontSize;
            targetTextStyle.fixedHeight = (targetText.lineSpacing == 1f) ? 0 : targetText.lineSpacing * targetTextStyle.lineHeight;
            targetTextStyle.richText = targetText.supportRichText;
            targetTextStyle.alignment = targetText.alignment;
            targetTextStyle.normal.textColor = targetText.color;

            targetText.enabled = false;
            this.ready = true;
        }

        void OnGUI()
        {
            if (this.ready)
                GUI.Label(targetTextPosition, targetText.text, targetTextStyle);
        }

        /// <summary>
        /// Finds the root parent canvas of this GameObject. 
        /// </summary>
        /// <returns>The root canvas's GameObject. Returns null if no GameObject's with Canvas
        /// components are found or if the no Canvas is marked as the root canvas.</returns>
        private static GameObject FindRootCanvas(GameObject gameObject)
        {
            Canvas[] canvases = gameObject.GetComponentsInParent<Canvas>();

            if (canvases.Length == 0)
            {
                Debug.LogWarning("[DrawTextOnGUI] No Canvases currently loaded!");
                return null;
            }

            foreach (Canvas c in canvases)
                if (c.isRootCanvas)
                    return c.gameObject;

            Debug.LogWarningFormat("[DrawTextOnGUI] {0} Canvases, but no root Canvas found?", canvases.Length);
            return null;
        }

    }
}