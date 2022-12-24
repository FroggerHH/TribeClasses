using UnityEngine;

namespace TribeClasses
{
    internal class ClassAltar : MonoBehaviour
    {
        public PulsatingGlow pulsatingGlow;

        private void Reset()
        {
            pulsatingGlow = transform.Find("model").GetComponent<PulsatingGlow>();
        }
    }
}
