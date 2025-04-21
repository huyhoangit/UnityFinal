using UnityEngine;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    public List<Camera> cameras;
    private int currentCameraIndex = 0;

    void Start()
    {
        // Initialize by enabling the first camera and disabling all others
        SwitchCamera(currentCameraIndex);
    }

    void Update()
    {
        // Switch cameras with the "C" key
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Increase index to switch to the next camera, looping around to the first camera
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;

            SwitchCamera(currentCameraIndex);
        }
    }

    // Method to switch to a specific camera based on index
    void SwitchCamera(int index)
    {
        int count = 0;
        Debug.Log("cameras length: " + cameras.Count);
        // Disable all cameras
        foreach (Camera cam in cameras)
        {
            Debug.Log("count: " + count++);
            cam.enabled = false;
            SetAudioListener(cam, false);
        }

        // Enable the selected camera
        Camera selectedCamera = cameras[index];
        selectedCamera.enabled = true;
        SetAudioListener(selectedCamera, true);
    }

    // Method to enable/disable the Audio Listener
    void SetAudioListener(Camera camera, bool isEnabled)
    {
        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = isEnabled;
        }
    }
}
