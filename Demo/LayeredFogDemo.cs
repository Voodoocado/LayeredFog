
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Voodoocado
{
    public class LayeredFogDemo : MonoBehaviour
    {
        public GameObject cameraRig;
        public Camera mainCamera;
        public PostProcessVolume postProcessVolume;
        public LayeredFog layeredFog;

		//public Moments.Recorder recorder;

		public void Start()
		{
            postProcessVolume.profile.TryGetSettings<LayeredFog>(out layeredFog);
        }

		public void Update()
        {
            cameraRig.transform.Rotate(Vector3.up, Time.deltaTime * 10f);

            layeredFog.fogHeightMax.value = 5f + 5f * Mathf.Sin(Time.time);

            // Record a GIF using Moments Recorder
            /*
            if (Input.GetKeyDown(KeyCode.F4))
            {
                print("KeyCode.F4");

                if (recorder.State != Moments.RecorderState.Recording)
                {
                    recorder.Record();
                    print("Now recording...");
                }
                else
                {
                    recorder.Pause();
                    print("Now pausing...");
                }
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                recorder.Save();
            }
            */
        }
    }
}