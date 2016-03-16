using System;

using UnityEngine;

namespace MedRoad.Utils
{
    /// <summary>
    /// Allows fade-to-black type transitions.
    /// </summary>
    public static class ScreenFader
    {
        // Inspired by https://unity3d.com/learn/tutorials/projects/stealth/screen-fader

        /// <summary>
        /// Fades the screen to black, performs the given action, and then fades the scene back in.
        /// </summary>
        /// <param name="fadeSpeed">The lerp rate when fading.</param>
        /// <param name="onBlack">An action to perform when the screen is black, or <c>null</c> to
        /// perform no action.</param>
        public static void PerformScreenFadeInOut(float fadeSpeed, Action onBlack)
        {
            GameObject screenFader = new GameObject("ScreenFader");

            GUITexture guiTexture = screenFader.AddComponent<GUITexture>();
            guiTexture.color = Color.clear;
            guiTexture.texture = Texture2D.whiteTexture;
            guiTexture.pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);

            ScreenFaderScript script = screenFader.AddComponent<ScreenFaderScript>();

            Action onScriptClear = delegate () {
                GameObject.Destroy(screenFader);
            };

            Action onScriptBlack = delegate () {
                if (onBlack != null)
                    onBlack();

                script.FadeToClear(fadeSpeed, onScriptClear);
            };

            script.FadeToBlack(fadeSpeed, onScriptBlack);
        }


        private class ScreenFaderScript : MonoBehaviour
        {
            private enum Mode
            {
                IDLE,
                FADING_TO_BLACK,
                FADING_TO_CLEAR
            }

            private Mode mode = Mode.IDLE;
            private float fadeSpeed;
            private Action completedCallback = null;
            private new GUITexture guiTexture;

            private void Awake()
            {
                guiTexture = this.GetComponent<GUITexture>();
            }

            private void Update()
            {
                if (this.mode == Mode.FADING_TO_BLACK)
                {
                    // Lerp the colour of the texture between itself and black.
                    guiTexture.color = Color.Lerp(guiTexture.color, Color.black, fadeSpeed * Time.deltaTime);

                    // If the screen is almost black...
                    if (guiTexture.color.a >= 0.95f)
                    {
                        guiTexture.enabled = false;
                        this.mode = Mode.IDLE;
                        if (completedCallback != null)
                            completedCallback();
                    }
                }

                if (this.mode == Mode.FADING_TO_CLEAR)
                {
                    // Lerp the colour of the texture between itself and transparent.
                    guiTexture.color = Color.Lerp(guiTexture.color, Color.clear, fadeSpeed * Time.deltaTime);

                    // If the screen is almost black...
                    if (guiTexture.color.a <= 0.025f)
                    {
                        guiTexture.enabled = false;
                        this.mode = Mode.IDLE;
                        if (completedCallback != null)
                            completedCallback();
                    }
                }
            }

            public void FadeToClear(float fadeSpeed, Action completedCallback)
            {
                this.fadeSpeed = fadeSpeed;
                this.completedCallback = completedCallback;
                guiTexture.enabled = true;
                this.mode = Mode.FADING_TO_CLEAR;
            }

            public void FadeToBlack(float fadeSpeed, Action completedCallback)
            {
                this.fadeSpeed = fadeSpeed;
                this.completedCallback = completedCallback;
                guiTexture.enabled = true;
                this.mode = Mode.FADING_TO_BLACK;
            }
        }

    }
}
