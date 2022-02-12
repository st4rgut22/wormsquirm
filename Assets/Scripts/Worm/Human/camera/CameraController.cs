using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    public void onTelemetry(Vector3 cameraPosition, Quaternion quaternion)
    {
        camera.transform.position = cameraPosition;
        camera.transform.rotation = quaternion * Quaternion.AngleAxis(90, Vector3.forward);
    }
}
