// using UnityEngine;

// public class Billboard : MonoBehaviour
// {
//     private Camera mainCamera;

//     private void Start()
//     {
//         mainCamera = Camera.main;
//         if (mainCamera == null) Debug.LogError("Billboard: Main Camera not found!");
//     }

//     private void LateUpdate()
//     {
//         if (mainCamera != null)
//         {
//             Vector3 direction = mainCamera.transform.position - transform.position;
//             direction.x = 0;
//             if (direction != Vector3.zero)
//             {
//                 transform.rotation = Quaternion.LookRotation(-direction);
//                 Debug.Log($"Billboard: Rotating towards camera at {mainCamera.transform.position}");
//             }
//         }
//     }
// }