namespace FacialRecognitionSystem.FacialRecognitionFunction;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Runtime.InteropServices;

/// <summary>
/// 顔情報の数値化
/// </summary>
public class FaceEmbeddingModel
{
    private readonly InferenceSession session;

    public FaceEmbeddingModel(string modelPath)
    {
        session = new InferenceSession(modelPath);
    }

    public float[] GetEmbedding(Mat face)
    {
        // 1. Resize（112×112）
        Mat resized = face.Resize(new Size(112, 112));

        // 2. BGR → RGB
        Cv2.CvtColor(resized, resized, ColorConversionCodes.BGR2RGB);

        resized.ConvertTo(resized, MatType.CV_32FC3, 1.0 / 128.0, -127.5 / 128.0);

        //// 3. float32 + 正規化 (x - 127.5) / 128
        //resized.ConvertTo(resized, MatType.CV_32FC3);
        //resized = (resized - 127.5f) / 128.0f;

        // 4. Mat → float[]（HWC）
        int size = resized.Rows * resized.Cols * resized.Channels();
        float[] hwc = new float[size];
        Marshal.Copy(resized.Data, hwc, 0, size);

        // 5. HWC → NCHW
        float[] nchw = new float[3 * 112 * 112];
        int idx = 0;

        for (int c = 0; c < 3; c++)
        {
            for (int y = 0; y < 112; y++)
            {
                for (int x = 0; x < 112; x++)
                {
                    int hwcIndex = (y * 112 + x) * 3 + c;
                    nchw[idx++] = hwc[hwcIndex];
                }
            }
        }

        // 6. Tensor 作成
        var tensor = new DenseTensor<float>(nchw, new[] { 1, 3, 112, 112 });

        //キーの確認用
        //foreach (var inp in session.InputMetadata)
        //{
        //    Console.WriteLine($"Input Name = {inp.Key}");
        //}


        var inputs = new List<NamedOnnxValue>
        {
            //NamedOnnxValue.CreateFromTensor("input", tensor)
            NamedOnnxValue.CreateFromTensor("input.1", tensor)
        };

        // 7. 推論
        using var results = session.Run(inputs);
        float[] embedding = results.First().AsEnumerable<float>().ToArray();

        // 8. L2 正規化
        NormalizeL2(embedding);

        return embedding;
    }

    private void NormalizeL2(float[] v)
    {
        float sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];

        float norm = MathF.Sqrt(sum);
        for (int i = 0; i < v.Length; i++)
            v[i] /= norm;
    }
}