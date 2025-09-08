using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public string folderPath = "C:\\Users\\artro\\KrathongScanner\\Assets\\Scanned";
    public float checkInterval = 2f;     // เวลาตรวจสอบโฟลเดอร์
    public float spawnCooldown = 7f;     // เวลาหน่วงการปล่อยกระทงต่ออัน (วินาที)

    private HashSet<string> loadedFiles = new HashSet<string>();
    private Queue<string> spawnQueue = new Queue<string>(); // ✅ เก็บ path ไว้เป็นคิว

    void Start()
    {
        StartCoroutine(CheckFolderLoop());  // ตรวจสอบไฟล์ใหม่
        StartCoroutine(SpawnLoop());        // ปล่อยกระทงตามคิว
    }

    // 🔹 ตรวจสอบไฟล์ใหม่
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

                    // ✅ เก็บเข้าคิว ไม่ spawn ทันที
                    spawnQueue.Enqueue(filePath);
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // 🔹 Spawn กระทงตามคิว
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnQueue.Count > 0)
            {
                string filePath = spawnQueue.Dequeue();

                Texture2D tex = LoadTexture(filePath);
                if (tex != null)
                {
                    Sprite sprite = ConvertToSprite(tex);
                    GameObject obj = CreatePrefabFromSprite(sprite);

                    // ตั้งตำแหน่ง (สุ่มนิดหน่อย)
                    obj.transform.position = new Vector3(
                        UnityEngine.Random.Range(-3f, 3f),
                        UnityEngine.Random.Range(-2f, 2f),
                        0
                    );

                    // ใส่อนิเมชัน
                    StartCoroutine(FloatAnimation(obj));
                }

                // ✅ รอ Cooldown ก่อนปล่อยอันถัดไป
                yield return new WaitForSeconds(spawnCooldown);
            }
            else
            {
                // ไม่มีไฟล์ใหม่ → รอ 0.5 วิ ก่อนเช็คอีกที
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // ================= Helper =================
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
        sr.sortingOrder = 5;

        // Collider2D
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;

        // Animator
        Animator animator = obj.AddComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("KratongController");

        // Scale
        obj.transform.localScale = new Vector3(35f, 35f, 18f);

        return obj;
    }

    IEnumerator FloatAnimation(GameObject obj)
    {
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null)
        {
            animator = obj.AddComponent<Animator>();
        }

        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("KratongController");
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning("หา Animator Controller ไม่เจอใน Resources!");
        }

        yield break;
    }
}
