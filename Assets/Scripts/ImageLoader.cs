using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ImageLoader : MonoBehaviour
{
    public string folderPath = "C:/Images";
    public GameObject photoPrefab;
    public Transform photoContainer;
    public float sphereRadius = 8f;

    void Start()
    {
        LoadImages();
    }

    void LoadImages()
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder not found: " + folderPath);
            return;
        }

        string[] files = Directory.GetFiles(folderPath);
        List<string> imageFiles = new List<string>();

        foreach (string file in files)
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                imageFiles.Add(file);
        }

        int total = imageFiles.Count;

        for (int i = 0; i < total; i++)
        {
            string file = imageFiles[i];

            byte[] imageBytes = File.ReadAllBytes(file);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);

            GameObject photo = Instantiate(photoPrefab, photoContainer);

            Renderer renderer = photo.GetComponent<Renderer>();
            renderer.material.mainTexture = tex;

            // initial sphere spawn
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / total);
            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * i;

            float x = sphereRadius * Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = sphereRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
            float z = sphereRadius * Mathf.Cos(phi);

            photo.transform.localPosition = new Vector3(x, y, z);
        }
    }
}