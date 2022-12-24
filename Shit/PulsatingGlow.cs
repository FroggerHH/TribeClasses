using UnityEngine;

namespace TribeClasses
{
    public class PulsatingGlow : MonoBehaviour
    {
        internal Renderer renderer;
        internal bool active = true;
        internal float MAXINTENSITY = 4.7f;
        internal float MININTENSITY = 2f;
        internal float LERPSPEED = 1.2f;
        internal float currentIntensity = 0f;
        public Color color = Color.white;
        internal bool onMax = false;

        private void Awake()
        {
            if (!renderer)
            {
                if (TryGetComponent(out renderer))
                {
                    return;
                }
            }
            if (!renderer)
            {
                return;
            }

            renderer.material.EnableKeyword("_Emission");
        }

        private void LateUpdate()
        {
            if (renderer && Time.timeScale > 0)
            {
                if (active)
                {
                    renderer.material.EnableKeyword("_Emission");
                    if (!onMax)
                    {
                        currentIntensity = Mathf.Lerp(currentIntensity, MAXINTENSITY + 0.2f, LERPSPEED * Time.deltaTime);
                        renderer.material.SetColor("_EmissionColor", color * currentIntensity);
                        if (currentIntensity >= MAXINTENSITY)
                        {
                            onMax = true;
                        }
                    }
                    else if (onMax)
                    {
                        currentIntensity = Mathf.Lerp(currentIntensity, MININTENSITY - 0.2f, LERPSPEED * Time.deltaTime);
                        renderer.material.SetColor("_EmissionColor", color * currentIntensity);
                        if (currentIntensity <= MININTENSITY)
                        {
                            onMax = false;
                        }
                    }
                }
                else
                {
                    renderer.material.SetColor("_EmissionColor", Color.white * 0);
                }
            }
        }
    }
}
