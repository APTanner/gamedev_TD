using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed for keyboard rotation
    public float verticalSpeed;
    public Vector3 minBorder, maxBorder; // Bounds for camera movement
    public float zoomSpeed = 5f; // Speed for zooming
    public float normalSpeed = 5f;
    public float fastestSpeed = 15f;
    public float panSmoothSpeed = 10f; // Speed for smoothing the panning movement
    public float mouseMovementThreshold = 0.01f; // Threshold for detecting significant mouse movement
    public float maxPanDistance = 10f; // Maximum distance for a single pan update

    public float horizontalSensitivity = 0.5f; // Sensitivity for horizontal mouse movement
    public float verticalSensitivity = 0.5f; // Sensitivity for vertical mouse movement

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
        CamMovement();
    }

    private void CamMovement()
    {
        // Use targetPos for all movement calculations
        Vector3 pos = targetPos;

        // Camera horizontal rotation with right mouse button
        if (Input.GetMouseButton(1)) // Right mouse button held down
        {
            float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

            // Rotate around the y-axis (horizontal rotation on main camera)
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + mouseX * rotationSpeed, 0f);

            // Tilt up and down (vertical rotation only on child camera)
            float newTilt = Mathf.Clamp(childCam.localRotation.eulerAngles.x - mouseY * verticalSpeed, 10f, 80f);
            childCam.localRotation = Quaternion.Euler(newTilt, 0f, 0f);
        }

        // Rotate with Q and E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y - rotationSpeed * Time.deltaTime, 0f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + rotationSpeed * Time.deltaTime, 0f);
        }

        // Simplified zoom functionality: move in the direction of childCam.forward
        float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 zoomDirection = childCam.forward * scrollInput * zoomSpeed;
            pos += zoomDirection;
        }

        // Keyboard movement
        if (Input.GetKey(KeyCode.LeftShift)) pos += verticalSpeed * Time.deltaTime * transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) pos -= verticalSpeed * Time.deltaTime * transform.up;
        if (Input.GetKey(KeyCode.W)) pos += speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.S)) pos -= speed * Time.deltaTime * transform.forward;
        if (Input.GetKey(KeyCode.A)) pos -= speed * Time.deltaTime * transform.right;
        if (Input.GetKey(KeyCode.D)) pos += speed * Time.deltaTime * transform.right;

        // Apply bounds to keep the camera within the designated area
        pos.x = Mathf.Clamp(pos.x, minBorder.x, maxBorder.x);
        pos.y = Mathf.Clamp(pos.y, minBorder.y, maxBorder.y);
        pos.z = Mathf.Clamp(pos.z, minBorder.z, maxBorder.z);

        // Update targetPos with the new position
        targetPos = pos;

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

        // Smoothly move the camera to the target position
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * panSmoothSpeed);
    }
}
