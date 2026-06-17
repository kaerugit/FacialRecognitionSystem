using FacialRecognitionSystem.UtilityFunction;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace FacialRecognitionSystem.FacialRecognitionFunction;


/// <summary>
/// 顔認証してない場合は登録
/// </summary>
public static class FacialRecognition
{

   
    //顔判定の定義
    private static readonly FaceRecognitionLogic logic = new();

    //顔判定モデルの定義
    private static FaceEmbeddingModel onnxModel;


    static FacialRecognition()
    {
        // MobileFaceNet ONNX モデル読み込み
        //https://github.com/deepinsight/insightface/tree/master/model_zoo
        onnxModel = new FaceEmbeddingModel(".\\Resource\\glint360k_r100.onnx");

        // 顔検出（OpenCV の HaarCascade）
        //var faceDetector = new CascadeClassifier(".\\Resource\\haarcascade_frontalface_default.xml");
        //faceDetector = new CascadeClassifier(".\\Resource\\haarcascade_frontalface_alt2.xml");
        //
    }

    //todo:var net = CvDnn.ReadNetFromONNX("retinaface.onnx");　サンプル

    /// <summary>
    /// 顔認証メイン処理
    /// </summary>
    /// <param name="config">設定ファイル</param>
    /// <param name="videoCaptureID">カメラID</param>
    /// <param name="frame">画面に表示する画像</param>
    /// <returns>該当するプレイヤーデータ/類似最大Score/newの場合true</returns>
    public static (PlayerData? playerData, double maxScore,  bool newFlag) Execute(AppConfig config, int videoCaptureID, ref Mat frame)
    {
        //デフォルトの戻り値
        (PlayerData?, double, bool) returnNull = (null, 0.0, false);

        if (videoCaptureID == -1)
        {
            return returnNull;
        }

        using var cam = new VideoCapture(videoCaptureID);

        //cam.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));

        if (config.FrameWidth > 0)
        {
            cam.Set(VideoCaptureProperties.FrameWidth, config.FrameWidth);
        }

        if (config.FrameHeight > 0)
        {
            cam.Set(VideoCaptureProperties.FrameHeight, config.FrameHeight);
        }

        //MessageBox.Show(cam.Get(VideoCaptureProperties.FrameWidth).ToString());
        //MessageBox.Show(cam.Get(VideoCaptureProperties.FrameHeight).ToString());
        //MessageBox.Show(cam.Get(VideoCaptureProperties.FourCC).ToString());

        if (frame.Rows == 0)        //テスト用（テストはjpgセットして読んでいる）
        {
            if (!cam.IsOpened())
            {
                //MessageBox.Show("カメラを開けませんでした。接続を確認してください。");
                return returnNull;
            }

            cam.Read(frame);

            // ノイズ除去
            Cv2.FastNlMeansDenoisingColored(frame, frame, 3, 3, 7, 21);

            // シャープ化
            Mat blurred = new Mat();
            Cv2.GaussianBlur(frame, blurred, new OpenCvSharp.Size(0, 0), 3);
            Cv2.AddWeighted(frame, 1.5, blurred, -0.5, 0, frame);
        }

        var faceDetector = FaceDetectorYN.Create(
               ".\\Resource\\face_detection_yunet_2023mar.onnx",
               "",
               new OpenCvSharp.Size(frame.Width, frame.Height), // 入力サイズ
               0.6f,               // スコアしきい値
               0.3f,               // NMSしきい値
               4,                  // TopK
               Backend.OPENCV, Target.CPU); // バックエンド設定


        if (!frame.Empty())
        {

            Mat faces = new Mat();
            //顔認証（結果がfacesに入ってくる）
            faceDetector.Detect(frame, faces);

            if (faces.Rows > 0)
            {
                //// 画像の大きさから こちらの値より小さい顔検出は除外する(ギャラリーなどは無視したい) デフォルト(0.1)10%
                var threshold = config.ExclusionRate;

                // 全体の大きさから割合を取得
                //var judgementArea = frame.Width * frame.Height * threshold;
                var judgementArea = frame.Width * frame.Height * threshold;

                Rect? faceRect = null;
                try
                {
                    for (var i = 0; i < faces.Rows; i++)
                    {

                        //[x, y, w, h, score,
                        //  left_eye_x, left_eye_y,
                        //  right_eye_x, right_eye_y,
                        //  nose_x, nose_y,
                        //  left_mouth_x, left_mouth_y,
                        //  right_mouth_x, right_mouth_y]
                        float[] row = new float[15];
                        //faces.Row(i).GetArray(out row);
                        
                        for (int c = 0; c < 15; c++)
                        {
                            row[c] = faces.At<float>(i, c);
                        }
                        //https://qiita.com/UnaNancyOwen/items/f3db189760037ec680f3

                        var x = (int)row[0];
                        var y = (int)row[1];
                        var w = (int)row[2];
                        var h = (int)row[3];

                        var leftEye = new Point2f(row[4], row[5]);
                        var rightEye = new Point2f(row[6], row[7]);
                        var nose = new Point2f(row[8], row[9]);

                        var score = row[14];

                        //デバッグモード時は写真とこちらの情報をtempフォルダに出力
                        if (config.DevelopMode)
                        {
                            var developFileName = DateTime.Now.ToString("MMddhhmmss");
                            var jpgFileName = Path.Combine(Utility.GetTempFolder(), developFileName + ".jpg") ?? "";
                            if (Path.Exists(jpgFileName) == false)
                            {
                                Cv2.ImWrite(jpgFileName, frame);
                            }
                            var jsonFileName = Path.Combine(Utility.GetTempFolder(), developFileName + ".json") ?? "";

                            var json = JsonSerializer.Serialize(row, new JsonSerializerOptions { WriteIndented = true });
                            Utility.WriteAllText(jsonFileName, json);

                        }

                        if (score < (float)config.FaceScore)    //顔認識としてのスコア
                        {
                            continue;
                        }

                        if ((decimal)(w * h) < judgementArea)    //小さいものは除く
                        {
                            continue;
                        }

                        //正面でない場合
                        if (isFrontFace(leftEye, rightEye, nose) == false)
                        {
                            continue;
                        }

                        faceRect = new Rect(
                            x,
                            y,
                            w, //width
                            h  //higth
                        );
                        break;
                                               

                    }
                }
                catch { }


                if (faceRect.HasValue)
                {
                    //顔の範囲を取得
                    Mat faceImg;

                    try
                    {
                        faceImg = new Mat(frame, faceRect.Value);
                    }
                    catch
                    {
                        return returnNull;
                    }


                    // 顔の範囲から数値化 (Embedding 生成　FaceEmbeddingModel.cs)
                    float[] embedding = onnxModel.GetEmbedding(faceImg);

                    // 該当するプレイヤーを検索
                    var (player, maxScore) = logic.FindPlayer(embedding,(double)config.JudgementScore);

                    var newFlag = false;
                    if (player == null)
                    {
                        player = logic.InsertPlayer(embedding, maxScore);
                        newFlag = true;
                    }
                    //else
                    //{
                    //}

                    //if (true)
                    //{
                    //赤枠つけて表示
                    Cv2.Rectangle(
                        frame,
                        faceRect.Value,
                        new Scalar(0, 0, 255), 2          // 赤色・線の太さ2
                    );

                    //デバッグ用　検出した顔を表示
                    //    Cv2.ImShow("Frame", frame);
                    //    Cv2.WaitKey(1);
                    //}

                    return (player, maxScore, newFlag);

                }
            }
        }

        return returnNull;

    }

    private static bool isFrontFace(Point2f leftEye, Point2f rightEye, Point2f nose)
    {
        // 目の高さ差
        float eyeDiff = Math.Abs(leftEye.Y - rightEye.Y);
        if (eyeDiff > 15) return false;

        // 鼻の左右位置（Yaw）
        float yaw = ((leftEye.X + rightEye.X) / 2f) - nose.X;
        if (Math.Abs(yaw) > 20) return false;

        return true;
    }

    #region 【参考】faceDetector 使用時のソース
    //顔検出の定義
    // private static CascadeClassifier faceDetector;

    //faceDetector = new CascadeClassifier(".\\Resource\\haarcascade_frontalface_alt2.xml");

    //public static (PlayerData?, double , bool ) Execute(AppConfig config, int videoCaptureID, ref Mat frame)
    //{
    //    //デフォルトの戻り値
    //    ( PlayerData?, double, bool ) returnNull = ( null,0.0, false);

    //    if (videoCaptureID == -1)
    //    {
    //        return returnNull;
    //    }

    //    using var cam = new VideoCapture(videoCaptureID);
    //    //using var cam = new VideoCapture(videoCaptureID,VideoCaptureAPIs.DSHOW);

    //    //cam.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));

    //    //cam.Set(VideoCaptureProperties.FrameWidth, 1280);
    //    //cam.Set(VideoCaptureProperties.FrameHeight, 720);

    //    //MessageBox.Show(cam.Get(VideoCaptureProperties.FrameWidth).ToString());
    //    //MessageBox.Show(cam.Get(VideoCaptureProperties.FrameHeight).ToString());
    //    //MessageBox.Show(cam.Get(VideoCaptureProperties.FourCC).ToString());

    //    if (frame.Rows == 0)        //テスト用（テストはjpgセットして読んでいる）
    //    {
    //        if (!cam.IsOpened())
    //        {
    //            //MessageBox.Show("カメラを開けませんでした。接続を確認してください。");
    //            return returnNull;
    //        }

    //        cam.Read(frame);
    //    }

    //    if (!frame.Empty())
    //    {

    //        // 顔検出  1.1 → 10% ずつ縮小しながら検出
    //        var faces = faceDetector.DetectMultiScale(frame, 1.05, 3); //1.1, 4);

    //        if (faces.Length == 0)
    //        {
    //            return returnNull;
    //        }

    //        // 画像の大きさから こちらの値より小さい顔検出は除外する(ギャラリーなどは無視したい) デフォルト(0.1)10%
    //        var threshold = config.ExclusionRate;

    //        // 全体の大きさから割合を取得
    //        var judgementArea = frame.Width * frame.Height * threshold;

    //        var filteredFaces = faces
    //            .Where(f => f.Width * f.Height >= judgementArea)
    //            .ToArray();

    //        if (filteredFaces.Length == 0)
    //        {
    //            return returnNull;
    //        }

    //        // 最初の顔を使用
    //        var faceRect = filteredFaces[0];
    //        Mat faceImg = new Mat(frame, faceRect);

    //        // Embedding 生成　FaceEmbeddingModel.cs
    //        float[] embedding = onnxModel.GetEmbedding(faceImg);

    //        // 該当するプレイヤーを検索
    //        var (player, maxScore) = logic.FindPlayer(embedding);

    //        var newFlag = false;
    //        if (player == null)
    //        {
    //            player = logic.InsertPlayer(embedding);
    //            newFlag = true;
    //            //lblStatus.Text = $"新規登録: ID={newId}, Name={name}";
    //        }
    //        else
    //        {
    //            //lblStatus.Text = $"認証成功: ID={player.PersonId}, Name={player.Name}";
    //        }

    //        //if (true)
    //        //{
    //        //赤枠つけて表示
    //        Cv2.Rectangle(
    //            frame,
    //            faceRect,
    //            new Scalar(0, 0, 255), 2          // 赤色・線の太さ2
    //        );

    //        //デバッグ用　検出した顔を表示
    //        //    Cv2.ImShow("Frame", frame);
    //        //    Cv2.WaitKey(1);
    //        //}

    //        return ( player, maxScore , newFlag);

    //    }

    //    return returnNull;

    //}
    #endregion
}
