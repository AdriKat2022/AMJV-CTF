using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 200;
    [SerializeField]
    private float minSpeed = 0;

    [Space]

    [SerializeField]
    private bool prioritizeKeys;
    [SerializeField]
    [Range(0f, 1f)]
    private float keyMultiplier = 0.75f;

    [Space]

    [SerializeField]
    [Range(0, 100)]
    private int borderX = 20;
    [SerializeField]
    [Range(0, 100)]
    private int borderY = 15;

    [Space]

    [SerializeField]
    [Range(0, 100)]
    private int easeFactor = 75;

    private int screenWidth;
    private int screenHeight;

    private Vector3 destination;

    private void Start()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;

        destination = transform.position;
    }


    private void Update()
    {
        GetDestination();
        MoveToDestination();

    }


    private void MoveToDestination()
    {
        transform.localPosition = Vector3.Lerp(transform.position, destination, 500 * Time.deltaTime / easeFactor);
    }

    private void GetDestination()
    {
        // Update the destination var according to the keys, then to the mouse (keys are prioritized)

        Vector2 mousePos = Input.mousePosition;

        int marginX = (int)(screenWidth * borderX/100f);
        int marginY = (int)(screenHeight * borderY/100f);

        bool inputKeyboard = false;

        if (Input.GetKey(KeyCode.D))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * Vector3.right;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * Vector3.left;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * Vector3.forward;
            inputKeyboard = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            destination += keyMultiplier * maxSpeed * Time.deltaTime * Vector3.back;
            inputKeyboard = true;
        }

        if(!prioritizeKeys || !inputKeyboard)
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
}
