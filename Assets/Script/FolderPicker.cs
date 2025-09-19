using UnityEngine;
using UnityEngine.UI;
using SFB; // namespace ของ StandaloneFileBrowser
using System;

public class FolderPicker : MonoBehaviour
{
    public Button selectFolderButton;
    public ImageLoader imageLoader; // อ้างถึง ImageLoader

    void Start()
    {
        selectFolderButton.onClick.AddListener(OpenFolderDialog);
    }

    void OpenFolderDialog()
    {
        // เปิด File Explorer ให้เลือกโฟลเดอร์
        var paths = StandaloneFileBrowser.OpenFolderPanel("เลือกโฟลเดอร์รูป", "", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedPath = paths[0];
            Debug.Log("เลือกโฟลเดอร์: " + selectedPath);

            // ส่งค่าไปให้ ImageLoader ใช้งาน
            imageLoader.SetFolderPath(selectedPath);
        }
    }
}
