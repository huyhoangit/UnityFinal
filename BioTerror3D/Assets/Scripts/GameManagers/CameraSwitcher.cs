using UnityEngine;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    public List<Camera> cameras;
    private int currentCameraIndex = 0;

    void Start()
    {
        if (cameras != null && cameras.Count > 0)
        {
            SwitchCamera(currentCameraIndex);
        }
        else
        {
            Debug.LogWarning("CameraSwitcher: No cameras assigned to the cameras list.");
        }
    }

    void Update()
    {
        if (cameras != null && cameras.Count > 0 && Input.GetKeyDown(KeyCode.C))
        {
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
            SwitchCamera(currentCameraIndex);
        }
    }

    void SwitchCamera(int index)
    {
        int count = 0;
        Debug.Log("cameras length: " + cameras.Count);
        foreach (Camera cam in cameras)
        {
            Debug.Log("count: " + count++);
            cam.enabled = false;
            SetAudioListener(cam, false);
        }

        Camera selectedCamera = cameras[index];
        selectedCamera.enabled = true;
        SetAudioListener(selectedCamera, true);
    }

    void SetAudioListener(Camera camera, bool isEnabled)
    {
        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = isEnabled;
        }
    }
}
