using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/**
 * Script for controlling the camera
 */
public class CameraScript : MonoBehaviour
{

    public Camera thisCamera;
    public float targetZoom;
    public float zoomFactor = 8f;
    public float zoomSpeed = 30f;
    public int panSpeed;

    private int size;
    /**
     * Sets up the camera to look at correct area
     * @param size The size of the field to look at
     */
    public void setup(int size)
    {
        this.size = size;
        int x = size / 2;
        int y = size / 2;
        int cameraSize = (size / 2) + 1;
        thisCamera.enabled = true;
        thisCamera.orthographic = true;
        thisCamera.orthographicSize = cameraSize;
        thisCamera.GetComponent<Transform>().position = new Vector3(x, y, -10);
        targetZoom = cameraSize;
    }

    private void Update()
    {
        float scrollData;
        scrollData = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollData * zoomFactor;
        targetZoom = Mathf.Clamp(targetZoom, size / 16, size / 2);
        thisCamera.orthographicSize = Mathf.Lerp(thisCamera.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);

        float horz = transform.position.x + Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
        float vert = transform.position.y + Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;
        horz = Mathf.Clamp(horz, (size / 2) - (size / 2 - targetZoom), (size / 2) + (size / 2 - targetZoom));
        vert = Mathf.Clamp(vert, (size / 2) - (size / 2 - targetZoom), (size / 2) + (size / 2 - targetZoom));
        transform.position = new Vector3(horz, vert, -10);
        


    }

}
