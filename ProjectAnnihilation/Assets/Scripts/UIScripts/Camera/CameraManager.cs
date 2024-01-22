using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Translation")]
    [SerializeField]
    private float maxSpeed = 200;
    [SerializeField]
    private float minSpeed = 0;
    [SerializeField]
    [Range(0, 100)]
    private int borderX = 20;
    [SerializeField]
    [Range(0, 100)]
    private int borderY = 15;

    [Header("Rotation")]
    [SerializeField]
    private float maxSpeedRotation = 1;
    [SerializeField]
    private float minSpeedRotation = 0;
    [SerializeField]
    [Range(0, 100)]
    private int borderXRotation = 20;
    [SerializeField]
    [Range(0, 100)]
    private int borderYRotation = 15;



    [SerializeField]
    private float scrollSpeed = 5;

    [Space]

    [SerializeField]
    private bool disableMouse;
    [SerializeField]
    private bool prioritizeKeys;
    [SerializeField]
    [Range(0f, 1f)]
    private float keyMultiplier = 0.75f;

    [Space]


    [Space]

    [SerializeField]
    [Range(0, 100)]
    private int easeFactor = 75;
    [SerializeField]
    [Range(0, 100)]
    private int easeFactorRotation = 75;

    private int screenWidth;
    private int screenHeight;

    private Vector3 destination;
    private Quaternion destinationRotation;

    private void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;

        destination = transform.position;
    }


    private void Update()
    {
        RotateCamera();
        ScrollCamera();
        GetDestination();
        MoveToDestination();
    }

    private void RotateCamera()
    {
        if (!Input.GetMouseButton(1))
            return;

        Vector2 mousePos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out RaycastHit hit))
            destinationRotation = Quaternion.LookRotation(hit.point - transform.position);
        

        //Debug.Log(hit.point);
        //Debug.DrawLine(hit.point, transform.position);

    }

    private void ScrollCamera()
    {
        if (destination.y + (Input.mouseScrollDelta.y * scrollSpeed * transform.forward).y <= 1f)
            return;

        destination += Input.mouseScrollDelta.y * scrollSpeed * transform.forward;
    }

    private void MoveToDestination()
    {
        transform.parent.position = Vector3.Lerp(transform.parent.position, destination, 500 * Time.deltaTime / easeFactor);
        transform.localRotation = Quaternion.Lerp(transform.rotation, destinationRotation, 100 * Time.deltaTime / easeFactorRotation);
    }

    private void GetDestination()
    {
        // Update the destination var according to the keys, then to the mouse (keys are prioritized)
        //UpdateParentRotation();

        Vector2 mousePos = Input.mousePosition;

        int marginX = (int)(screenWidth * borderX/100f);
        int marginY = (int)(screenHeight * borderY/100f);

        bool inputKeyboard = false;

        var (forwardAxeInPlane, rightAxeInPlane) = GetForwardAndRightAxeInPlane();

        if (Input.GetKey(KeyCode.D))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * rightAxeInPlane;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            destination -= keyMultiplier * maxSpeed * Time.deltaTime * rightAxeInPlane;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * forwardAxeInPlane;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            destination -= keyMultiplier * maxSpeed * Time.deltaTime * forwardAxeInPlane;
            inputKeyboard = true;
        }

        if( (!prioritizeKeys || !inputKeyboard) && !disableMouse)
        {
            if (mousePos.x <= marginX)
            {
                float speed = Mathf.Lerp(maxSpeed, minSpeed, mousePos.x / marginX);

                destination += speed * Time.deltaTime * Vector3.left;
            }
            else if (mousePos.x >= screenWidth - marginX)
            {
                float speed = Mathf.Lerp(minSpeed, maxSpeed, (mousePos.x - screenWidth + marginX) / marginX);

                destination += speed * Time.deltaTime * Vector3.right;
            }

            if (mousePos.y <= marginY)
            {
                float speed = Mathf.Lerp(maxSpeed, minSpeed, mousePos.y / marginY);

                destination += speed * Time.deltaTime * Vector3.back;
            }
            else if (mousePos.y >= screenHeight - marginY)
            {
                float speed = Mathf.Lerp(minSpeed, maxSpeed, (mousePos.y - screenHeight + marginY) / marginY);

                destination += speed * Time.deltaTime * Vector3.forward;
            }
        }
    }

    private (Vector3, Vector3) GetForwardAndRightAxeInPlane()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;

        Vector3 right = Vector3.Cross(Vector3.up, forward);

        return (forward.normalized, right.normalized);
    }
}
