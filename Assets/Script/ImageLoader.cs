using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public string folderPath;
    public float Countloop = 3;
    public float checkInterval = 2f;         // เวลาตรวจสอบโฟลเดอร์
    public float spawnCooldown = 5f;         // CD ของระบบหลัก (ไฟล์ใหม่)
    public float queueSpawnCooldown = 8f;    // CD ของระบบรอง (วนคิว 3 อัน)
    public float krathongLifeTime = 15f;     // อายุของกระทง (หลัก + รอง)

    private HashSet<string> loadedFiles = new HashSet<string>();
    private Queue<string> spawnQueue = new Queue<string>();   // คิวระบบหลัก
    private List<string> lastFiles = new List<string>();      // เก็บ 3 อันล่าสุด
    private int queueIndex = 0;

    private bool isMainSpawning = false;     // บอกว่าระบบหลักกำลัง spawn อยู่

    void Start()
    {
        StartCoroutine(CheckFolderLoop());   // ตรวจสอบไฟล์ใหม่
        StartCoroutine(SpawnLoop());         // ระบบหลัก
        StartCoroutine(QueueSpawnLoop());    // ระบบรอง
    }
    public void SetFolderPath(string path)
    {
        folderPath = path;
        Debug.Log("ImageLoader ใช้โฟลเดอร์ใหม่: " + folderPath);
    }

    // 🔹 ตรวจสอบไฟล์ใหม่
    IEnumerator CheckFolderLoop()
    {
        while (true)
        {
            string[] files = Directory.GetFiles(folderPath, "*.png");

            // เรียงไฟล์ตามเวลาแก้ไข (ล่าสุดอยู่ท้าย)
            Array.Sort(files, (a, b) => File.GetLastWriteTime(a).CompareTo(File.GetLastWriteTime(b)));

            foreach (string filePath in files)
            {
                if (!loadedFiles.Contains(filePath))
                {
                    Debug.Log("พบรูปใหม่: " + filePath);
                    loadedFiles.Add(filePath);

                    // ✅ เข้าคิวระบบหลัก
                    spawnQueue.Enqueue(filePath);

                    // ✅ อัปเดตระบบรอง (เก็บ 3 ไฟล์ล่าสุด)
                    if (lastFiles.Count >= Countloop)
                        lastFiles.RemoveAt(0);
                    lastFiles.Add(filePath);
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // 🔹 Spawn ไฟล์ใหม่ (ระบบหลัก)
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnQueue.Count > 0)
            {
                isMainSpawning = true; // ✅ เริ่ม spawn หลัก

                string filePath = spawnQueue.Dequeue();
                SpawnKratong(filePath, "Kratong"); // ✅ ใช้ชื่อ Krathong

                yield return new WaitForSeconds(spawnCooldown);

                isMainSpawning = false; // ✅ จบการ spawn หลัก
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // 🔹 วน spawn จาก 3 อันล่าสุด (ระบบรอง)
    IEnumerator QueueSpawnLoop()
    {
        // ✅ ตอนเริ่มต้องรอก่อน 1 รอบ
        yield return new WaitForSeconds(queueSpawnCooldown);

        while (true)
        {
            // ✅ รอจนกว่าหลักจะว่างก่อนค่อย spawn
            while (isMainSpawning)
                yield return null;

            if (lastFiles.Count > 0)
            {
                string filePath = lastFiles[queueIndex % lastFiles.Count];
                queueIndex++;

                SpawnKratong(filePath, "KratongLoop"); // ✅ ใช้ชื่อ KrathongLoop
            }

            // ✅ รอ cooldown ทุกครั้งก่อน spawn รอบใหม่
            yield return new WaitForSeconds(queueSpawnCooldown);
        }
    }

    // ================= Helper =================
    void SpawnKratong(string filePath, string objectName)
    {
        Texture2D tex = LoadTexture(filePath);
        if (tex != null)
        {
            Sprite sprite = ConvertToSprite(tex);
            GameObject obj = CreatePrefabFromSprite(sprite, objectName);

            // ตำแหน่ง (สุ่มเล็กน้อย)
            obj.transform.position = new Vector3(
                UnityEngine.Random.Range(-3f, 3f),
                UnityEngine.Random.Range(-2f, 2f),
                0
            );

            // ✅ ทำลายอัตโนมัติหลังจากเวลาที่กำหนด
            Destroy(obj, krathongLifeTime);

            StartCoroutine(FloatAnimation(obj));
        }
    }

    Texture2D LoadTexture(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                return tex;
            }
        }
        catch (IOException)
        {
            Debug.LogWarning("ไฟล์ยังไม่พร้อม: " + filePath);
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

    GameObject CreatePrefabFromSprite(Sprite sprite, string objectName)
    {
        GameObject obj = new GameObject(objectName);

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
        Vector3 startPos = obj.transform.position;

        while (obj != null)
        {
            obj.transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time) * 0.2f, 0);
            yield return null;
        }
    }
}
