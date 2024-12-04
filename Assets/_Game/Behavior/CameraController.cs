using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed for keyboard rotation
    public float verticalSpeed = 100f;

    public Vector3 minBorder, maxBorder; // Bounds for camera movement
    public float zoomSpeed = 5f; // Speed for zooming
    public float normalSpeed = 5f;
    public float fastestSpeed = 15f;
    public float panSmoothSpeed = 10f; // Speed for smoothing the panning movement
    public float mouseMovementThreshold = 0.01f; // Threshold for detecting significant mouse movement
    public float maxPanDistance = 10f; // Maximum distance for a single pan update

    public float arrowRotationSpeed = 100f;

    public float horizontalSensitivity = 1f; // Sensitivity for horizontal mouse movement
    public float verticalSensitivity = 1f; // Sensitivity for vertical mouse movement

    private float speed;
    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector3 targetPos; // Target position for panning and movement
    private Camera cam;
    private Transform childCam;
    private Plane plane;

    private void Awake()
    {
        cam = Camera.main;
        speed = normalSpeed;
        childCam = transform.GetChild(0).transform;
        plane = new Plane(Vector3.up, Vector3.zero);
        targetPos = transform.position; // Initialize target position
    }

    private void Update()
    {
        CameraRotate();
        CameraMove();
    }

    private void CameraMove()
    {
        // The reason the scroll sensitivity is so high is that my mousewheel only registers scrolling
        // for a few frames, which means it must travel very fast in those few frames. This may break on
        // your machine so change it if that's the case
        float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 zoomDirection = scrollInput * zoomSpeed * childCam.forward;
            targetPos += zoomDirection * Time.deltaTime;
        }

        // Keyboard movement
        if (Input.GetKey(KeyCode.LeftShift)) targetPos += verticalSpeed * Time.deltaTime * transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) targetPos -= verticalSpeed * Time.deltaTime * transform.up;
        if (Input.GetKey(KeyCode.W)) targetPos += speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.S)) targetPos -= speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.A)) targetPos -= speed * Time.deltaTime * transform.right;
        if (Input.GetKey(KeyCode.D)) targetPos += speed * Time.deltaTime * transform.right;


        // Movement with slide (middle mouse panning)
        if (Input.GetMouseButtonDown(2))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float entry))
            {
                startPos = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(2))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float entry))
            {
                currentPos = ray.GetPoint(entry);
                Vector3 movementDelta = startPos - currentPos;

                // Clamp the movement delta to the maximum allowed distance
                movementDelta = Vector3.ClampMagnitude(movementDelta, maxPanDistance);

                // Only update targetPos if the movement is significant
                if (movementDelta.magnitude > mouseMovementThreshold)
                {
                    targetPos += movementDelta;
                    startPos = currentPos; // Update startPos to currentPos to reset for next movement
                }
            }
        }

        // Apply bounds to keep the camera within the designated area
        targetPos.x = Mathf.Clamp(targetPos.x, minBorder.x, maxBorder.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBorder.y, maxBorder.y);
        targetPos.z = Mathf.Clamp(targetPos.z, minBorder.z, maxBorder.z);

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * panSmoothSpeed);
    }

    private void CameraRotate()
    {
        // Camera horizontal rotation with right mouse button
        if (Input.GetMouseButton(1)) // Right mouse button held down
        {
            float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;
            RotateX(mouseY * rotationSpeed);
            RotateY(mouseX * rotationSpeed);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            RotateX(arrowRotationSpeed);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            RotateX(-arrowRotationSpeed);
        }

        // Rotate with Q and E keys
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow))
        {
            RotateY(-arrowRotationSpeed);
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightArrow))
        {
            RotateY(arrowRotationSpeed);
        }
    }

    private void RotateX(float angle)
    {
        Vector3 rot = childCam.localEulerAngles;
        rot.x = Mathf.Clamp(rot.x - angle * Time.deltaTime, 10f, 80f);
        childCam.localEulerAngles = rot;
    }

    private void RotateY(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, angle * Time.deltaTime, 0f) * transform.rotation;
    }
}
