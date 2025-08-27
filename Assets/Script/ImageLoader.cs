using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public string folderPath = "C:\\Users\\artro\\KrathongScanner\\Assets\\Scanned";
    public float checkInterval = 2f; // เช็คทุกๆ 2 วินาที

    private HashSet<string> loadedFiles = new HashSet<string>();

    void Start()
    {
        StartCoroutine(CheckFolderLoop());
    }

    IEnumerator CheckFolderLoop()
    {
        while (true)
        {
            string[] files = Directory.GetFiles(folderPath, "*.png");

            foreach (string filePath in files)
            {
                if (!loadedFiles.Contains(filePath))
                {
                    Debug.Log("พบรูปใหม่: " + filePath);
                    loadedFiles.Add(filePath);

                    Texture2D tex = LoadTexture(filePath);
                    if (tex != null)
                    {
                        Sprite sprite = ConvertToSprite(tex);
                        GameObject obj = CreatePrefabFromSprite(sprite);

                        // ตั้งตำแหน่งสุ่มนิดหน่อยไม่ให้ซ้อนกัน
                        obj.transform.position = new Vector3(
                            UnityEngine.Random.Range(-3f, 3f),
                            UnityEngine.Random.Range(-2f, 2f),
                            0
                        );

                        // เริ่ม Animation ลอย
                        StartCoroutine(FloatAnimation(obj));
                    }
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            return tex;
        }
        return null;
    }

    Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    GameObject CreatePrefabFromSprite(Sprite sprite)
    {
        GameObject obj = new GameObject("Kratong");

        // SpriteRenderer
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // ✅ ตั้งค่า Order in Layer (ค่ามากจะอยู่ข้างหน้า)
        sr.sortingOrder = 5;

        // Collider2D
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;

        // Animator
        Animator animator = obj.AddComponent<Animator>();

        // ✅ ฟิก Scale ตายตัว
        obj.transform.localScale = new Vector3(16f, 16f, 8f);

        return obj;
    }


    IEnumerator FloatAnimation(GameObject obj)
    {
        if (obj == null) yield break;

        // เช็คว่ามี Animator ไหม ถ้าไม่มีก็เพิ่ม
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null)
        {
            animator = obj.AddComponent<Animator>();
        }

        // โหลด Animator Controller ที่คุณทำไว้
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("KratongController");
        // หมายเหตุ: ต้องเอา KratongController.controller ไปวางในโฟลเดอร์ Resources
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning("หา Animator Controller ไม่เจอใน Resources!");
        }

        yield break; // จบ Coroutine ไปเลย เพราะแค่เซ็ต Animator ก็พอ
    }

}
