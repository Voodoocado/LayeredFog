
using UnityEngine;

namespace Voodoocado
{
    public class Demo : MonoBehaviour
    {
        public GameObject cameraRig;
        public Camera mainCamera;
        //public Moments.Recorder recorder;

        public void Update()
        {
            cameraRig.transform.Rotate(Vector3.up, Time.deltaTime * 10f);

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