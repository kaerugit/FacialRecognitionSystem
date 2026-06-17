using FacialRecognitionSystem.FacialRecognitionFunction;
using FacialRecognitionSystem.UtilityFunction;
using OpenCvSharp;
using static System.Net.WebRequestMethods;

namespace FacialRecognitionTest;


[TestClass]
public class FacialRecognitionTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        PlayerDataStorage.Delete();
    }
    /// <summary>
    /// jpegから類似テスト
    /// </summary>
    [TestMethod]
    public void TestJpgExecute()
    {
        execute("jpg", "jpg");
    }

    [TestMethod]
    public void TestPngExecute()
    {
        execute("png", "jpg");
    }

    [TestMethod]
    public void TestTempExecute()
    {
        execute("temp", "jpg");
    }

    private void execute(string folderName ,string ext)
    {
        // テストプロジェクトの jpg フォルダを指す
        string dir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", folderName);
        dir = Path.GetFullPath(dir);

        // テストファイル用 JPG ファイルを列挙
        var files = Directory.GetFiles(dir, "*." + ext)
                        .OrderBy(f => f)
                        .ToArray();

        var config = AppConfig.Load();

        foreach (var file in files)
        {
            var frame = Cv2.ImRead(file);
            var (p, bestScore, newFlag) = FacialRecognition.Execute(config, 0, ref frame);

            //判定OKの場合
            if (p != null)
            {
                Assert.IsFalse(file.Contains("なし"), file);

                if (newFlag == true)    //完全新規
                {
                    Assert.IsFalse(file.Contains("同じ"), file);
                }
                else
                {
                    Assert.IsTrue(file.Contains("同じ"), file);
                }

            }
            else //人物がいない場合
            {
                Assert.IsTrue(file.Contains("なし"), file);
                //Assert.Fail("登録できません");
            }

        }
    }
}