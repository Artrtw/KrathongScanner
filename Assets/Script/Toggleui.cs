using UnityEngine;

public class ToggleSetting : MonoBehaviour
{
    public GameObject[] settings; // ✅ เก็บได้หลาย GameObject

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject setting in settings)
            {
                if (setting != null)
                {
                    setting.SetActive(!setting.activeSelf);
                    Debug.Log("Toggle Setting: " + setting.name + " = " + setting.activeSelf);
                }
            }
        }
    }
}
