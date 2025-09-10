using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class ImageRawPublisher : UnityPublisher<MessageTypes.Sensor.Image>
    {
        public Camera ImageCamera;
        public string FrameId = "camera_link";
        public int resolutionWidth = 640;
        public int resolutionHeight = 480;

        private MessageTypes.Sensor.Image message;
        private Texture2D texture2D;
        private Rect rect;

        protected override void Start()
        {
            base.Start();
            InitializeGameObject();
            InitializeMessage();
            Camera.onPostRender += UpdateImage;
        }

        private void UpdateImage(Camera _camera)
        {
            if (texture2D != null && _camera == this.ImageCamera)
                UpdateMessage();
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
            ImageCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.Image();
            message.header.frame_id = FrameId;
            message.encoding = "rgb8"; // 修改为 encoding
            message.height = (uint)resolutionHeight;
            message.width = (uint)resolutionWidth;
            message.is_bigendian = 0;
            message.step = (uint)resolutionWidth * 3; // RGB8 每像素 3 字节
            message.data = new byte[resolutionWidth * resolutionHeight * 3];
        }

        private void UpdateMessage()
        {
            message.header.Update();
            RenderTexture.active = ImageCamera.targetTexture;
            texture2D.ReadPixels(rect, 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;

            // 获取原始像素数据
            message.data = texture2D.GetRawTextureData();

            // Color32[] pixels = texture2D.GetPixels32();
            // Color32[] flippedPixels = FlipTextureVertically(pixels, resolutionWidth, resolutionHeight);
            // message.data = ConvertPixelsToByteArray(flippedPixels);

            Publish(message);
        }

        private Color32[] FlipTextureVertically(Color32[] originalPixels, int width, int height)
        {
            Color32[] flipped = new Color32[originalPixels.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flipped[y * width + x] = originalPixels[(height - y - 1) * width + x];
                }
            }
            return flipped;
        }

        private byte[] ConvertPixelsToByteArray(Color32[] pixels)
        {
            byte[] byteArray = new byte[pixels.Length * 3]; // RGB 每像素 3 字节
            for (int i = 0; i < pixels.Length; i++)
            {
                byteArray[i * 3] = pixels[i].r;
                byteArray[i * 3 + 1] = pixels[i].g;
                byteArray[i * 3 + 2] = pixels[i].b;
            }
            return byteArray;
        }

        // private Time GetCurrentRosTime()
        // {
        //     double currentTime = Time.time;
        //     uint sec = (uint)Mathf.Floor((float)currentTime);
        //     uint nanosec = (uint)((currentTime - sec) * 1e9);
        //     return new Time { sec = sec, nanosec = nanosec };
        // }
    }
}
