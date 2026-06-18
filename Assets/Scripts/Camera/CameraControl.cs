using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private bool isFirstPerson = true; 

    [Header("Camera Positions")]
    // Tọa độ tương đối của Camera so với CamPivot
    [SerializeField] private Vector3 fpsLocalPos = new Vector3(0f, 0f, 0f);     // Ngay tại điểm pivot (mắt)
    [SerializeField] private Vector3 tpsLocalPos = new Vector3(0f, 0.4f, -3.5f); // Lùi ra sau 3.5m, hơi cao lên một chút

    void Start()
    {
        // Mặc định ban đầu ở góc nhìn thứ nhất
        transform.localPosition = fpsLocalPos;
    }

    public void change_camera_mode()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
        {
            transform.localPosition = fpsLocalPos;
        }
        else
        {
            transform.localPosition = tpsLocalPos;
        }
    }
}