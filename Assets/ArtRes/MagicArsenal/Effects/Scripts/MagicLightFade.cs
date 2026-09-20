using System.Collections;
using UnityEngine;


namespace MagicArsenal
{
    public class MagicLightFade : MonoBehaviour
    {
        [Header("Seconds to dim the light")]
        public float life = 0.2f;
        public bool killAfterLife = true;


        private Light li;
        private float initIntensity;

        // Use this for initialization
        private void Awake()
        {
            li = GetComponent<Light>();

            if (li != null)
                initIntensity = li.intensity;
        }

        private void OnEnable()
        {
            // 每次从对象池激活，都恢复灯光亮度。
            if (li != null)
                li.intensity = initIntensity;
        }

        // Update is called once per frame
        private void Update()
        {
            if (li == null)
                return;

            if (life <= 0f)
            {
                li.intensity = 0f;
            }
            else
            {
                li.intensity = Mathf.Max(
                    0f,
                    li.intensity - initIntensity * Time.deltaTime / life
                );
            }

            if (killAfterLife && li.intensity <= 0f)
                Destroy(gameObject);
        }
    }
}