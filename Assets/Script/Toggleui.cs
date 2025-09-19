using UnityEngine;

public class ToggleSetting : MonoBehaviour
{
    public GameObject setting; // ลาก Setting มาวางใน Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (setting != null)
            {
                setting.SetActive(!setting.activeSelf);
                Debug.Log("Toggle Setting: " + setting.activeSelf);
            }
        }
    }
}
