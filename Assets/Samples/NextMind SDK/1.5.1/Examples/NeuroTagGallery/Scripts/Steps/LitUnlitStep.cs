using UnityEngine;

namespace NextMind.Examples.NeuroTagGallery
{
    /// <summary>
    /// Implementation of a <see cref="NeuroTagGalleryStep"/>.
    /// During this step, one light of the environment is shut down and a point light is going left and right to show the difference between a lit and an unlit material.
    /// </summary>
    public class LitUnlitStep : NeuroTagGalleryStep
    {
        [Header("Light")]

        /// <summary>
        /// The light that will move left to right.
        /// </summary>
        [SerializeField]
        private Light rotatingLight;

        /// <summary>
        /// The speed of the light movement.
        /// </summary>
        [SerializeField]
        private float speed;

        /// <summary>
        /// The max rotation possible to the left for the moving light.
        /// </summary>
        private Quaternion rotationLeft;
        /// <summary>
        /// The max rotation possible to the right for the moving light.
        /// </summary>
        private Quaternion rotationRight;

        private float timer = 0f;

        #region AbstractStep implementation
        public override void OnEnterStep()
        {
            HubManager.Instance.ActivateLight(false);
        }

        public override void OnExitStep()
        {
            var hubManager = HubManager.Instance;
            if (hubManager)
            {
                hubManager.ActivateLight(true);
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            // Set the maximum rotation values.
            rotationLeft = Quaternion.Euler(0f, -45f, 0f);
            rotationRight = Quaternion.Euler(0f, 45f, 0f);
        }

        protected override void UpdateAnimation()
        {
            base.UpdateAnimation();

            timer += Time.deltaTime;
            float progress = (Mathf.Sin(timer * speed) + 1) / 2;
            rotatingLight.transform.rotation = Quaternion.Lerp(rotationLeft, rotationRight, progress);
        }
    }
}

