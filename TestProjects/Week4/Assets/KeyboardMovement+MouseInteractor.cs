using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class MousePointerGrab3D_FreeMove : MonoBehaviour
{
    [Header("Pick Settings")]
    public LayerMask grabbableLayers = ~0;
    public float maxPickDistance = 200f;
    public float followStiffness = 1800f;   // SpringJoint.spring
    public float followDamping  = 60f;      // SpringJoint.damper
    public float moveLerp = 0.2f;           // softness of grabber motion (0..1)
    public float scrollSensitivity = 6f;    // push/pull along the ray
    public float minDistance = 0.5f;
    public float maxDistance = 20f;

    [Header("Hover Feedback (optional)")]
    public Color hoverColor = new Color(1f, 0.9f, 0.3f, 0.6f);
    public float outlineWidth = 0.015f;

    [Header("Camera Move + Look")]
    public float lookSensitivity = 2.0f;    // mouse look when holding RMB
    public float moveSpeed = 6.0f;          // WASD speed (always active)
    public float sprintMultiplier = 2.0f;   // hold Left Shift
    public float pitchClamp = 89f;          // stop flipping

    private Camera cam;

    // grab mechanism
    private Rigidbody grabberRB;
    private SpringJoint joint;
    private Rigidbody heldRB;
    private float currentDistance;

    // hover
    private MaterialPropertyBlock mpb;
    private Renderer hoverRenderer;
    private int colorID, widthID;
    private bool hoverApplied;

    // look state
    private bool freeLookActive;
    private float yaw, pitch;

    void Awake()
    {
        cam = GetComponent<Camera>();

        // Invisible kinematic follower that the joint pulls toward the mouse ray
        var grabber = new GameObject("MouseGrabber");
        grabber.hideFlags = HideFlags.HideInHierarchy;
        grabberRB = grabber.AddComponent<Rigidbody>();
        grabberRB.isKinematic = true;
        grabberRB.interpolation = RigidbodyInterpolation.Interpolate;

        joint = grabber.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.maxDistance = 0f;
        joint.minDistance = 0f;
        joint.spring = followStiffness;
        joint.damper = followDamping;
        joint.enablePreprocessing = false;

        // hover feedback setup
        mpb = new MaterialPropertyBlock();
        colorID = Shader.PropertyToID("_OutlineColor");
        widthID = Shader.PropertyToID("_OutlineWidth");

        // start with free cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // init yaw/pitch
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        HandleRightMouseFreeLook();     // rotates on RMB
        HandleWASDMove();               // ALWAYS ON movement

        // When looking around with RMB, we pause picking (keeps the “edit” feel clean)
        if (freeLookActive)
        {
            ClearHover();
            if (Input.GetMouseButtonUp(0)) EndGrab();
            return;
        }

        // Ignore UI clicks
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            if (Input.GetMouseButtonUp(0)) EndGrab();
            return;
        }

        // Hover highlight
        UpdateHover();

        // Begin / end grab
        if (Input.GetMouseButtonDown(0)) TryBeginGrab();
        if (Input.GetMouseButtonUp(0)) EndGrab();

        // Adjust grab distance while holding
        if (heldRB != null)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                currentDistance = Mathf.Clamp(currentDistance + scroll * scrollSensitivity * Time.deltaTime,
                                              minDistance, maxDistance);
            }
        }
    }

    void FixedUpdate()
    {
        // Move the grabber each physics step (even while moving the camera)
        Vector3 targetPos = GetMouseRayPoint(currentDistance);
        Vector3 newPos = Vector3.Lerp(grabberRB.position, targetPos,
                         1f - Mathf.Pow(1f - moveLerp, Time.fixedDeltaTime * 60f));
        grabberRB.MovePosition(newPos);
    }

    // ------------------------
    // Picking
    // ------------------------
    void TryBeginGrab()
    {
        if (heldRB != null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPickDistance, grabbableLayers, QueryTriggerInteraction.Ignore))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null && rb.isKinematic == false)
            {
                heldRB = rb;
                currentDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);

                // snap grabber to hit point to avoid a jerk on pickup
                grabberRB.position = hit.point;

                joint.connectedBody = heldRB;
                joint.connectedAnchor = heldRB.transform.InverseTransformPoint(hit.point);
            }
        }
    }

    void EndGrab()
    {
        if (heldRB != null)
        {
            joint.connectedBody = null;
            heldRB = null;
        }
    }

    Vector3 GetMouseRayPoint(float distance)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return ray.GetPoint(distance);
    }

    // ------------------------
    // Hover feedback
    // ------------------------
    void UpdateHover()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPickDistance, grabbableLayers, QueryTriggerInteraction.Ignore))
        {
            Renderer r = hit.collider.GetComponentInParent<Renderer>();
            if (r != null && r != hoverRenderer)
            {
                ApplyHover(r);
            }
        }
        else
        {
            ClearHover();
        }
    }

    void ApplyHover(Renderer r)
    {
        ClearHover();
        hoverRenderer = r;
        hoverApplied = true;

        // Works if your shader has _OutlineColor/_OutlineWidth; otherwise no-op.
        hoverRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorID, hoverColor);
        mpb.SetFloat(widthID, outlineWidth);
        hoverRenderer.SetPropertyBlock(mpb);
    }

    void ClearHover()
    {
        if (!hoverApplied || hoverRenderer == null) return;
        hoverRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(widthID, 0f);
        hoverRenderer.SetPropertyBlock(mpb);
        hoverRenderer = null;
        hoverApplied = false;
    }

    void OnDisable()
    {
        ClearHover();
    }

    // ------------------------
    // Camera look (RMB) + movement (always on)
    // ------------------------
    void HandleRightMouseFreeLook()
    {
        if (Input.GetMouseButtonDown(1))
        {
            freeLookActive = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            freeLookActive = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!freeLookActive) return;

        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;
        yaw += mx;
        pitch = Mathf.Clamp(pitch - my, -pitchClamp, pitchClamp);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleWASDMove()
    {
        float boost = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

        Vector3 move = new Vector3(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)
        );
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Always-on: move relative to camera facing, regardless of mouse state
        transform.position += transform.TransformDirection(move) * moveSpeed * boost * Time.deltaTime;
    }
}
