using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class ONNXtest : MonoBehaviour
{
    private InferenceSession session;
    //public TextAsset modelFile; // 将 ONNX 模型添加到 Unity 项目中，并在 Inspector 中关联
    public Texture2D inputImage; // 在 Inspector 中关联输入图像

    void Start()
    {
        // 加载 ONNX 模型
        // string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "yolov8s.onnx");
        string modelPath = Application.dataPath + "/yolov8s.onnx";
        session = new InferenceSession(modelPath);

        // 图像预处理
        float[] inputTensorData = PreprocessImage(inputImage, 640, 640);

        // 创建输入张量
        var inputTensor = new DenseTensor<float>(inputTensorData, new[] { 1, 3, 640, 640 });

        // 运行模型
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };
        var results = session.Run(inputs);

        // 解析输出
        var output = results.First().AsTensor<float>().ToArray();
        var boxes = ParseOutput(output, inputImage.width, inputImage.height);

        // 输出检测结果
        foreach (var box in boxes)
        {
            Debug.Log($"Class: {box.Label}, Prob: {box.Prob}, Box: ({box.X1}, {box.Y1}, {box.X2}, {box.Y2})");
        }

        // 可视化检测框
        VisualizeBoxes(boxes);
    }

    private float[] PreprocessImage(Texture2D img, int targetWidth, int targetHeight)
    {
        Texture2D resizedImage = new Texture2D(targetWidth, targetHeight);
        Graphics.ConvertTexture(img, resizedImage);

        Color[] pixels = resizedImage.GetPixels();
        float[] inputData = new float[3 * targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int index = y * targetWidth + x;
                Color pixel = pixels[index];
                inputData[index] = pixel.r / 255.0f; // R通道
                inputData[targetWidth * targetHeight + index] = pixel.g / 255.0f; // G通道
                inputData[2 * targetWidth * targetHeight + index] = pixel.b / 255.0f; // B通道
            }
        }

        return inputData;
    }

    private List<(float X1, float Y1, float X2, float Y2, string Label, float Prob)> ParseOutput(float[] output, int imgWidth, int imgHeight)
    {
        int numBoxes = output.Length / 84; // 每个框包含84个值
        List<(float, float, float, float, string, float)> boxes = new List<(float, float, float, float, string, float)>();

        for (int i = 0; i < numBoxes; i++)
        {
            int offset = i * 84;
            float xc = output[offset + 0];
            float yc = output[offset + 1];
            float w = output[offset + 2];
            float h = output[offset + 3];

            float x1 = (xc - w / 2) * imgWidth / 640;
            float y1 = (yc - h / 2) * imgHeight / 640;
            float x2 = (xc + w / 2) * imgWidth / 640;
            float y2 = (yc + h / 2) * imgHeight / 640;

            float[] classProbs = output.Skip(offset + 4).Take(80).ToArray();
            int bestClass = Array.IndexOf(classProbs, classProbs.Max());
            float confidence = classProbs[bestClass];

            if (confidence > 0.5)
            {
                string label = GetClassLabel(bestClass);
                boxes.Add((x1, y1, x2, y2, label, confidence));
            }
        }

        return boxes;
    }

    private string GetClassLabel(int classId)
    {
        string[] yoloClasses = {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
            "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
            "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
            "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
            "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard",
            "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush"
        };
        return yoloClasses[classId];
    }

    private void VisualizeBoxes(List<(float X1, float Y1, float X2, float Y2, string Label, float Prob)> boxes)
    {
        foreach (var box in boxes)
        {
            Debug.DrawLine(new Vector3(box.X1, box.Y1, 0), new Vector3(box.X2, box.Y1, 0), Color.red, 5);
            Debug.DrawLine(new Vector3(box.X2, box.Y1, 0), new Vector3(box.X2, box.Y2, 0), Color.red, 5);
            Debug.DrawLine(new Vector3(box.X2, box.Y2, 0), new Vector3(box.X1, box.Y2, 0), Color.red, 5);
            Debug.DrawLine(new Vector3(box.X1, box.Y2, 0), new Vector3(box.X1, box.Y1, 0), Color.red, 5);

            Debug.Log($"{box.Label} ({box.Prob:F2}) at ({box.X1}, {box.Y1})");
        }
    }

    void OnDestroy()
    {
        session?.Dispose();
    }
}
