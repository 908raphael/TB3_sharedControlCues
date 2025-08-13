using System;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine;

public class ONNXModelInference : MonoBehaviour
{
    private InferenceSession session;
    public Texture2D inputImage; // 输入图像
    public RectTransform canvas; // 用于可视化检测框的画布

    private Texture2D resizedImage;

    void Start()
    {
        // 加载 ONNX 模型
        string modelPath = Application.dataPath + "/yolov8s.onnx";
        session = new InferenceSession(modelPath);

        resizedImage = ResizeImage(inputImage, 640, 640);
        float[] inputData = PreprocessImage(resizedImage);

        // 加载并预处理输入图片
        //float[] inputData = PreprocessImage(inputImage, 640, 640);

        // 创建 ONNX 张量
        var inputTensor = new DenseTensor<float>(inputData, new[] { 1, 3, 640, 640 });

        //var input = new[] { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

        // 推理
        var input = new NamedOnnxValue[]
        {
            NamedOnnxValue.CreateFromTensor("images", inputTensor) // 替换 "images" 为实际输入名
        };

        using (var results = session.Run(input))
        {
            // 获取输出张量
            var output = results.First().AsTensor<float>().ToArray();

            Debug.Log($"Output shape: {output.Length}");

            // 解析输出并可视化
            int numBoxes = 8400;
            int numClasses = 80;
            var detections = ParseOutput(output, numBoxes, numClasses, 0.5f);

            VisualizeDetections(detections, 5);
        }
    }

    void OnDestroy()
    {
        session?.Dispose();
    }

    private float[] PreprocessImage(Texture2D image)
    {
        Color32[] pixels = image.GetPixels32();
        float[] inputData = new float[1 * 3 * image.width * image.height];

        for (int i = 0; i < pixels.Length; i++)
        {
            inputData[i] = pixels[i].r / 255.0f;
            inputData[i + image.width * image.height] = pixels[i].g / 255.0f;
            inputData[i + 2 * image.width * image.height] = pixels[i].b / 255.0f;
        }

        return inputData;
    }

    // 预处理图像为模型输入格式
    // private float[] PreprocessImage(Texture2D image, int width, int height)
    // {
    //     Texture2D resizedImage = new Texture2D(width, height);
    //     Graphics.ConvertTexture(image, resizedImage);

    //     float[] inputData = new float[1 * 3 * width * height]; // 3 通道
    //     Color[] pixels = resizedImage.GetPixels(); // 获取像素值

    //     for (int i = 0; i < pixels.Length; i++)
    //     {
    //         inputData[i] = pixels[i].r / 255.0f; // 归一化
    //         inputData[i + width * height] = pixels[i].g / 255.0f;
    //         inputData[i + 2 * width * height] = pixels[i].b / 255.0f;
    //     }

    //     return inputData;
    // }

    private Texture2D ResizeImage(Texture2D source, int width, int height)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, tmp);
        
        Texture2D result = new Texture2D(width, height);
        RenderTexture.active = tmp;
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.ReleaseTemporary(tmp);
        return result;
    }

    // 解析模型输出
    private (float x, float y, float w, float h, float confidence, int classId)[] ParseOutput(float[] output, int numBoxes, int numClasses, float threshold)
    {
        int stride = 84; // 每个检测框的长度 (4 + 80)
        var detections = new System.Collections.Generic.List<(float, float, float, float, float, int)>();

        for (int i = 0; i < numBoxes; i++)
        {
            // 解析检测框
            float x = output[i * stride + 0];
            float y = output[i * stride + 1];
            float w = output[i * stride + 2];
            float h = output[i * stride + 3];

            // 获取类别分数并找到最大值
            float[] classScores = new float[numClasses]; //初始化类别分数数组，长度为80，值为0
            Array.Copy(output, i * stride + 4, classScores, 0, numClasses); // 类别分数从第 5 个位置开始
            int bestClass = Array.IndexOf(classScores, classScores.Max());
            float confidence = classScores[bestClass]; // 置信度为最大类别分数


            if (confidence > threshold)
            {
                detections.Add((x, y, w, h, confidence, bestClass));
            }
        }
        Debug.Log($"Detections: {detections.Count}");

        // 按置信度排序
        // 
        return detections.OrderByDescending(d => d.Item5).ToArray();
    }

    // 可视化检测框
    private void VisualizeDetections((float x, float y, float w, float h, float confidence, int classId)[] detections, int maxDetections)
    {
        for (int i = 0; i < Mathf.Min(detections.Length, maxDetections); i++)
        {
            // 解构元组
            var (x, y, w, h, confidence, classId) = detections[i];
            Debug.Log($"Class: {classId}, Confidence: {confidence:F2}");

            // 转换为屏幕坐标
            float screenX = x * canvas.rect.width;
            float screenY = y * canvas.rect.height;
            float screenW = w * canvas.rect.width;
            float screenH = h * canvas.rect.height;


            // 创建 UI 元素表示框
            GameObject box = new GameObject("DetectionBox");
            box.transform.SetParent(canvas);
            var rectTransform = box.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(screenX, screenY);
            rectTransform.sizeDelta = new Vector2(screenW, screenH);

            var image = box.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(1, 0, 0, 0.5f); // 半透明红色

            // 显示类别和置信度
            GameObject label = new GameObject("Label");
            label.transform.SetParent(box.transform);
            var text = label.AddComponent<UnityEngine.UI.Text>();
            text.text = $"Class: {classId}, Conf: {confidence:F2}";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
        }
    }
}