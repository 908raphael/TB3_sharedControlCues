using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapLoader : MonoBehaviour
{
    public string yamlFilePath = "Assets/Maps/mymap.yaml"; // YAML 文件路径
    public Image mapImage;  // UI Image 组件
    private string imagePath = "Assets/Maps/mymap.pgm";
    private float resolution;
    private Vector3 origin;

    private int desiredHeight;

    private int desiredWidth;

    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        if (!File.Exists(yamlFilePath))
        {
            Debug.LogError("YAML file not found at: " + yamlFilePath);
            return;
        }

        // 读取 YAML 文件内容
        string[] lines = File.ReadAllLines(yamlFilePath);
        Dictionary<string, string> yamlData = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            // 跳过空行和注释行
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            string[] keyValue = line.Split(':');
            if (keyValue.Length >= 2)
            {
                string key = keyValue[0].Trim();
                string value = string.Join(":", keyValue, 1, keyValue.Length - 1).Trim();
                yamlData[key] = value;
            }
        }

        // 获取 YAML 数据
        if (yamlData.ContainsKey("image"))
        {
            imagePath = Path.Combine(Path.GetDirectoryName(yamlFilePath), yamlData["image"].Replace("\"", ""));
        }
        if (yamlData.ContainsKey("resolution"))
        {
            resolution = float.Parse(yamlData["resolution"]);
        }
        if (yamlData.ContainsKey("origin"))
        {
            string[] originValues = yamlData["origin"].Replace("[", "").Replace("]", "").Split(',');
            origin = new Vector3(float.Parse(originValues[0]), float.Parse(originValues[1]), float.Parse(originValues[2]));
        }

        Debug.Log($"Loaded Map: {imagePath}, Resolution: {resolution}, Origin: {origin}");

        // 加载地图图像
        LoadImageToPanel(imagePath);
    }

    void LoadImageToPanel(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError("Image file not found at: " + imagePath);
            return;
        }

        Texture2D texture = LoadPGMImage(imagePath);
        if (texture == null)
        {
            Debug.LogError("Failed to load PGM image.");
            return;
        }

        // 将 Texture 转换为 Sprite
        Sprite mapSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        // 将 Sprite 设置到 UI Image 上
        mapImage.sprite = mapSprite;

        // 调整 Image 的大小以匹配地图尺寸
        //RectTransform rectTransform = mapImage.GetComponent<RectTransform>();

        //rectTransform.sizeDelta = new Vector2(texture.width, texture.height);

    }

    Texture2D LoadPGMImage(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        int index = 0;

        // 读取魔数（P5）
        string magicNumber = ReadNextToken(fileData, ref index);
        if (magicNumber != "P5")
        {
            Debug.LogError("Not a valid PGM file");
            return null;
        }

        // 读取宽度和高度
        int width = 0;
        int height = 0;
        int maxGrayValue = 0;

        while (true)
        {
            string token = ReadNextToken(fileData, ref index);
            if (token.StartsWith("#"))
            {
                // 跳过注释行
                SkipLine(fileData, ref index);
                continue;
            }

            if (width == 0)
            {
                width = int.Parse(token);
            }
            else if (height == 0)
            {
                height = int.Parse(token);
            }
            else
            {
                maxGrayValue = int.Parse(token);
                break;
            }
        }

        // 检查最大灰度值
        if (maxGrayValue > 255)
        {
            Debug.LogError("Unsupported max gray value: " + maxGrayValue);
            return null;
        }

        // 计算图像数据的开始位置
        int dataStartIndex = index;

        // 检查数据长度是否足够
        int expectedDataLength = width * height;
        if (fileData.Length - dataStartIndex < expectedDataLength)
        {
            Debug.LogError("Insufficient image data in PGM file.");
            return null;
        }

        // 提取图像数据
        byte[] imageData = new byte[expectedDataLength];
        System.Array.Copy(fileData, dataStartIndex, imageData, 0, expectedDataLength);

        // 创建纹理
        Texture2D texture = new Texture2D(width, height, TextureFormat.Alpha8, false);
        texture.LoadRawTextureData(imageData);
        texture.Apply();

        if (desiredWidth <= 0 || desiredHeight <= 0)
        {
            return texture;
        }

        // 调整纹理大小
        Texture2D resizedTexture = new Texture2D(desiredWidth, desiredHeight, TextureFormat.Alpha8, false);
        Graphics.ConvertTexture(texture, resizedTexture);

        return resizedTexture;
    }

    string ReadNextToken(byte[] data, ref int index)
    {
        SkipWhitespace(data, ref index);

        if (index >= data.Length)
            return null;

        // 检查是否为注释行
        if (data[index] == '#')
        {
            return "#";
        }

        List<byte> tokenBytes = new List<byte>();
        while (index < data.Length && !IsWhitespace(data[index]))
        {
            tokenBytes.Add(data[index]);
            index++;
        }

        return System.Text.Encoding.ASCII.GetString(tokenBytes.ToArray());
    }

    void SkipWhitespace(byte[] data, ref int index)
    {
        while (index < data.Length && IsWhitespace(data[index]))
        {
            index++;
        }
    }

    void SkipLine(byte[] data, ref int index)
    {
        while (index < data.Length && data[index] != '\n')
        {
            index++;
        }
        if (index < data.Length && data[index] == '\n')
        {
            index++;
        }
    }

    bool IsWhitespace(byte b)
    {
        return b == ' ' || b == '\t' || b == '\r' || b == '\n';
    }
}
