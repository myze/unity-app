using System.Collections;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Assets.Scripts.StartupScreens.MenuScreen
{
    public class TransitionalMove : MonoBehaviour
    {
        [SerializeField]
        CurveControlledBob headBob = new CurveControlledBob();
        [SerializeField]
        [Range(0.1f, 1f)]
        float moveSpeed = 0.1f;
        [SerializeField]
        [Range(1f, 5f)]
        float rotateSpeed = 1f;
        [SerializeField]
        [Range(0f, 1f)]
        float cornerDelay = 1f;
        [SerializeField]
        [Range(0f, 1f)]
        float lightIncrement = 0.1f;
        [SerializeField]
        float stepInterval = 5;
        [SerializeField]
        float headBobSpeed = 5;

        Transform cardboardMain;
        Camera _camera;
        AfterExitDelegate after;

        // Action performed after exit
        public delegate void AfterExitDelegate();

        // Use this for initialization
        void Start()
        {
            cardboardMain = GameObject.Find("CardboardMain").transform;
            _camera = Camera.main;
            headBob.Setup(_camera, stepInterval);
        }

        // For non-MonoBehaviour class usage
        public void towardsExit()
        {
            StartCoroutine(
                towardsExitCoroutine(after)
                );
        }

        // Set after exit action
        public void setAfterExitEventHandler(AfterExitDelegate afterExit)
        {
            after = afterExit;
        }

        IEnumerator rotateToExit()
        {
            for (float i = 0; i <= 90; i += rotateSpeed)
            {
                cardboardMain.rotation = Quaternion.Euler(0, i, 0);
                yield return null;
            }
        }

        IEnumerator towardsExitCoroutine(AfterExitDelegate after)
        {
            // Binding
            Transform lighting = GameObject.Find("Lighting").transform;

            // Forward
            for (float i = 0; i <= 11.5f; i += moveSpeed)
            {
                cardboardMain.position = new Vector3(
                    cardboardMain.position.x,
                    cardboardMain.position.y,
                    i
                    );

                _camera.transform.localPosition = headBob.DoHeadBob(headBobSpeed);

                float a = Mathf.Round(i * 100) / 100f, b = Mathf.Round(11.5f / moveSpeed / 1.9f) * moveSpeed;

                // Rotate
                if (a == b)
                    StartCoroutine(rotateToExit());

                yield return null;
            }

            // Delay for second(s)
            yield return new WaitForSeconds(cornerDelay);

            // Leftwards
            for (float i = 0; i <= 11.5f; i += moveSpeed)
            {
                cardboardMain.position = new Vector3(
                    i,
                    cardboardMain.position.y,
                    cardboardMain.position.z
                    );

                _camera.transform.localPosition = headBob.DoHeadBob(headBobSpeed);

                if (i >= 3f)
                    // Increase light's intensity
                    for (int j = 0; j < lighting.childCount; j++)
                        lighting.GetChild(j).GetComponent<Light>().intensity += lightIncrement;

                yield return null;
            }

            if (after != null)
                // Action after exit
                after();
        }
    }
}
