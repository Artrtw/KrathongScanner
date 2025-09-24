using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using TMPro; // ต้องใช้ TextMeshPro

public class KrathongReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    [Tooltip("IP address of the Scanner PC")]
    public string scannerIP = "192.168.1.100";

    [Header("UI")]
    public TMP_InputField ipInputField; // ช่องกรอก IP

    [Tooltip("Port the scanner API runs on")]
    public int scannerPort = 5000;
    
    [Tooltip("How often to check for new scans (seconds)")]
    public float pollInterval = 2f;
    
    [Header("Image Loader Integration")]
    [Tooltip("Reference to the ImageLoader component that monitors the folder")]
    public ImageLoader imageLoader;
    
    // Internal variables
    private string lastScanTimestamp = "";
    
    [System.Serializable]
    private class ScanData
    {
        public string filename;
        public string timestamp;
        public string template_used;
        public string status;
    }
    
    void Start()
    {
        // Validate setup
        if (imageLoader == null)
        {
            Debug.LogError("KrathongReceiver: No ImageLoader component assigned! Please assign it in the Inspector.");
            return;
        }
        
        if (string.IsNullOrEmpty(imageLoader.folderPath))
        {
            Debug.LogError("KrathongReceiver: ImageLoader folderPath is not set!");
            return;
        }
        if (ipInputField != null)
        {
            // ตั้งค่า default ลงในกล่องกรอก
            ipInputField.text = scannerIP;

            // ฟัง event เวลา user พิมพ์จบ
            ipInputField.onEndEdit.AddListener(OnIPChanged);
        }
        // Ensure the folder exists
        if (!Directory.Exists(imageLoader.folderPath))
        {
            Directory.CreateDirectory(imageLoader.folderPath);
            Debug.Log($"KrathongReceiver: Created folder {imageLoader.folderPath}");
        }
        
        Debug.Log($"KrathongReceiver: Starting to poll scanner at {scannerIP}:{scannerPort}");
        Debug.Log($"KrathongReceiver: Will save images to {imageLoader.folderPath}");
        
        StartCoroutine(PollForNewScans());
    }

    void OnIPChanged(string newIP)
    {
        scannerIP = newIP;
        Debug.Log("Scanner IP updated to: " + scannerIP);
    }

    IEnumerator PollForNewScans()
    {
        while (true)
        {
            yield return new WaitForSeconds(pollInterval);
            
            // Check for latest scan metadata
            UnityWebRequest request = UnityWebRequest.Get($"http://{scannerIP}:{scannerPort}/api/latest_scan");
            
            // FIX: Allow insecure HTTP connections
            request.certificateHandler = new AcceptAllCertificatesHandler();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ScanData scanData = JsonUtility.FromJson<ScanData>(request.downloadHandler.text);
                    
                    // Check if this is a new scan
                    if (scanData.timestamp != lastScanTimestamp && scanData.status == "ready")
                    {
                        lastScanTimestamp = scanData.timestamp;
                        Debug.Log($"KrathongReceiver: New scan detected - {scanData.filename}");
                        
                        // Download and save to ImageLoader's folder
                        StartCoroutine(DownloadAndSaveKrathong(scanData.filename));
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"KrathongReceiver: Failed to parse scan data: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"KrathongReceiver: Failed to check for new scans: {request.error}");
            }
        }
    }
    
    IEnumerator DownloadAndSaveKrathong(string filename)
    {
        string downloadUrl = $"http://{scannerIP}:{scannerPort}/api/download/{filename}";
        Debug.Log($"KrathongReceiver: Downloading krathong from {downloadUrl}");
        
        UnityWebRequest download = UnityWebRequestTexture.GetTexture(downloadUrl);
        
        // FIX: Allow insecure HTTP connections
        download.certificateHandler = new AcceptAllCertificatesHandler();
        
        yield return download.SendWebRequest();
        
        if (download.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)download.downloadHandler).texture;
            
            // Generate unique filename to avoid conflicts
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string uniqueFilename = $"network_krathong_{timestamp}.png";
            string savePath = Path.Combine(imageLoader.folderPath, uniqueFilename);
            
            // Convert texture to PNG bytes and save
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(savePath, pngData);
            
            Debug.Log($"KrathongReceiver: Saved krathong to {savePath}");
            Debug.Log("KrathongReceiver: ImageLoader should automatically detect and spawn the krathong");
        }
        else
        {
            Debug.LogError($"KrathongReceiver: Failed to download krathong: {download.error}");
        }
    }
    
    // Public method to manually trigger a scan check (useful for debugging)
    public void CheckForNewScans()
    {
        StopCoroutine(PollForNewScans());
        StartCoroutine(PollForNewScans());
    }
    
    // Public method to set scanner IP at runtime
    public void SetScannerIP(string newIP)
    {
        scannerIP = newIP;
        Debug.Log($"KrathongReceiver: Scanner IP updated to {newIP}");
    }
}

// Custom certificate handler to accept all certificates (for HTTP connections)
public class AcceptAllCertificatesHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Accept all certificates (for development/testing only)
        return true;
    }
}