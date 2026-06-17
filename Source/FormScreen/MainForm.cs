namespace FacialRecognitionSystem.FormScreen;

using FacialRecognitionSystem.FacialRecognitionFunction;
using FacialRecognitionSystem.UtilityFunction;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
//using DirectShowLib;
public partial class MainForm : Form
{
    #region 変数定義

    /// <summary>カメラのカウント</summary>
    const int CAMERA_COUNT = 30;

    /// <summary>タイマーコンポーネント</summary>
    Timer timer = new();

    /// <summary>configファイル</summary>
    private AppConfig _config;


    private DateTime lastDateTime1 = DateTime.Now;
    private DateTime lastDateTime2 = DateTime.Now;

    #endregion

    #region フォームのイベント

    public MainForm()
    {
        InitializeComponent();

        Utility.DeleteTempFolder();
        _config = AppConfig.Load();

        this.initData();
        this.initBindData();

        //サイズの変更
        btnAdvanced_Click(new object(), EventArgs.Empty);
        chkDevelopMode_CheckedChanged(new object(), EventArgs.Empty);

        timer.Enabled = false;
        timer.Tick += Timer_Tick;   //初期はで　値セット時のtxtTimerInterval_TextChanged　で自動実行される

        //var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

        //for (int i = 0; i < devices.Length; i++)
        //{
        //    MessageBox.Show($"{i}: {devices[i].Name}");
        //}
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (_config.DevelopMode == false)
        {
            Utility.DeleteTempFolder();
        }
        _config.Save(); // 念のため保存
    }

    #endregion

    #region 各種コントロールのイベント

    /// <summary>
    /// チェック用Timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Timer_Tick(object? sender, EventArgs e)
    {
        // 既に実行中
        if (timer.Enabled == false)
        {
            return;
        }

        timer.Enabled = false;

        await Task.Run(() =>
        {
            //カメラ1台目
            if (true)
            {
                var videoCaptureID = _config.VideoCaptureID1;
                var pictureBox = this.pictureBox1;
                var lblPlayerId = this.lblPlayerID1;
                var lblPlayerName = this.lblPlayerName1;
                var lblLastPlayerID = this.lblLastPlayerID1;
                var playerFileName = _config.FileSaveFullPath1;

                facialRecognitionExecute(videoCaptureID, pictureBox, lblPlayerId, lblPlayerName, playerFileName, lblLastPlayerID, ref lastDateTime1);
            }

            //カメラ2台目
            if (true)
            {
                var videoCaptureID = _config.VideoCaptureID2;
                var pictureBox = this.pictureBox2;
                var lblPlayerId = this.lblPlayerID2;
                var lblPlayerName = this.lblPlayerName2;
                var lblLastPlayerID = this.lblLastPlayerID2;
                var playerFileName = _config.FileSaveFullPath2;

                facialRecognitionExecute(videoCaptureID, pictureBox, lblPlayerId, lblPlayerName, playerFileName, lblLastPlayerID, ref lastDateTime2);
            }

            //全参加プレーヤのファイルを作成
            if (!string.IsNullOrEmpty(_config.JoinPlayerFileFullPath))
            {

                var fileIdList = Utility.GetTempIdList();
                var playerList = new PlayerDataList(
                        PlayerDataStorage.PlayerDataDB.Where(
                        w => fileIdList.Contains(w.PlayerId.ToString()) && !string.IsNullOrEmpty(w.PlayerName)
                        )
                    )
                    .Select(w => w.PlayerName.ToString())
                    .Distinct()
                    .ToList();
                ;

                //区切り文字
                var splitChar = _config.JoinPlayerSplitChar;
                if (string.IsNullOrEmpty(splitChar))
                {
                    splitChar = Environment.NewLine;
                }

                var text = string.Join(char.Parse("¶"), playerList).Replace("¶", splitChar);
                Utility.WriteAllText(_config.JoinPlayerFileFullPath, text, true);

            }
        });

        timer.Enabled = true;
    }

    /// <summary>
    /// カメラのindex（1台目）
    /// </summary>
    private void cboVideoCapture1_SelectedIndexChanged(object sender, EventArgs e)
    {
        txtTimerInterval_TextChanged(new object(), EventArgs.Empty);
    }

    /// <summary>
    /// カメラのindex（2台目）
    /// </summary>
    private void cboVideoCapture2_SelectedIndexChanged(object sender, EventArgs e)
    {
        txtTimerInterval_TextChanged(new object(), EventArgs.Empty);
    }

    /// <summary>
    /// プレイヤー名保存ボタンクリック（1台目）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave1_Click(object sender, EventArgs e)
    {
        this.openSaveFileDialog(this.txtFileSavePath1, AppConfig.DEFAULT_FILE_SAVE_FULLPATH1);
    }

    /// <summary>
    /// プレイヤー名保存ボタンクリック（2台目）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave2_Click(object sender, EventArgs e)
    {
        this.openSaveFileDialog(this.txtFileSavePath2, AppConfig.DEFAULT_FILE_SAVE_FULLPATH2);
    }

    /// <summary>
    /// プレイヤー名リストボタンクリック
    /// </summary>
    private void btnJoinPlayerFileFullPath_Click(object sender, EventArgs e)
    {
        this.openSaveFileDialog(this.txtJoinPlayerFileFullPath, "join.txt");
    }


    private void txtTimerInterval_TextChanged(object sender, EventArgs e)
    {
        int value;
        if (!int.TryParse(this.txtTimerInterval.Text, out value))
        {
            value = 10;
        }

        timer.Stop();
        this.timer.Interval = 1000 * value;
        timer.Start();
        Timer_Tick(new object(), EventArgs.Empty);
    }

    /// <summary>
    /// プレイヤー名登録ボタンクリック（1台目）
    /// </summary>
    private void btnPlayerSave1_Click(object sender, EventArgs e)
    {
        openInputPlayer(this.lblPlayerID1.Text);
    }

    /// <summary>
    /// プレイヤー名登録ボタンクリック（2台目）
    /// </summary>

    private void btnPlayerSave2_Click(object sender, EventArgs e)
    {
        openInputPlayer(this.lblPlayerID2.Text);
    }


    private bool isExpanded = true;

    private void btnAdvanced_Click(object sender, EventArgs e)
    {
        var defaultHeight = this.btnAdvanced.Top + this.btnAdvanced.Height + 45;

        if (!isExpanded)
        {
            this.Height = defaultHeight + this.pnlAdvanced.Height + 10;          // フォームを拡張
            this.btnAdvanced.Text = "<<閉じる";
            isExpanded = true;
        }
        else
        {
            this.Height = defaultHeight;                                   // 元のサイズに戻す
            this.btnAdvanced.Text = "高度な設定>>";
            isExpanded = false;
        }
    }

    private void chkDevelopMode_CheckedChanged(object sender, EventArgs e)
    {
        this.lblPlayerID1.Visible = this.chkDevelopMode.Checked;
        this.lblPlayerID2.Visible = this.chkDevelopMode.Checked;

        this.lblLastPlayerID1.Visible = this.chkDevelopMode.Checked;
        this.lblLastPlayerID2.Visible = this.chkDevelopMode.Checked;

    }
    #endregion

    #region private関数

    private void initData()
    {
        var list = new List<ComboBoxItem>()
        {
            new ComboBoxItem { Value = -1, Display = "" }
        };

        for (var i = 0; i < CAMERA_COUNT; i++)
        {
            list.Add(new ComboBoxItem { Value = (i), Display = (i).ToString() });
        }

        this.cboVideoCapture1.DataSource = Utility.DeepCopy(list);
        this.cboVideoCapture1.DisplayMember = nameof(ComboBoxItem.Display);   // 表示
        this.cboVideoCapture1.ValueMember = nameof(ComboBoxItem.Value);

        this.cboVideoCapture2.DataSource = Utility.DeepCopy(list);
        this.cboVideoCapture2.DisplayMember = nameof(ComboBoxItem.Display);   // 表示
        this.cboVideoCapture2.ValueMember = nameof(ComboBoxItem.Value);
    }

    private void initBindData()
    {

        //カメラ1台目
        this.cboVideoCapture1.DataBindings.Add("SelectedValue", _config, nameof(AppConfig.VideoCaptureID1),
                            false, DataSourceUpdateMode.OnPropertyChanged);


        this.txtFileSavePath1.DataBindings.Add("Text", _config, nameof(AppConfig.FileSaveFullPath1),
                             false, DataSourceUpdateMode.OnPropertyChanged);

        //カメラ2台目
        this.txtFileSavePath2.DataBindings.Add("Text", _config, nameof(AppConfig.FileSaveFullPath2),
                             false, DataSourceUpdateMode.OnPropertyChanged);

        this.cboVideoCapture2.DataBindings.Add("SelectedValue", _config, nameof(AppConfig.VideoCaptureID2),
                            false, DataSourceUpdateMode.OnPropertyChanged);


        //高度な設定
        this.txtTimerInterval.DataBindings.Add("Text", _config, nameof(AppConfig.TimerInterval),
                             false, DataSourceUpdateMode.OnPropertyChanged);

        this.txtFaceScore.DataBindings.Add("Text", _config, nameof(AppConfig.FaceScore),
            true, DataSourceUpdateMode.OnPropertyChanged, null, "0.0");  //入力終わってからバインド


        this.txtJudgementScore.DataBindings.Add("Text", _config, nameof(AppConfig.JudgementScore),
             true, DataSourceUpdateMode.OnPropertyChanged, null, "0.0");  //入力終わってからバインド

        this.txtExclusionRate.DataBindings.Add("Text", _config, nameof(AppConfig.ExclusionRate),
                     true, DataSourceUpdateMode.OnPropertyChanged, null, "0.0");    //入力終わってからバインド

        this.txtNewPlayerName.DataBindings.Add("Text", _config, nameof(AppConfig.NewPlayerName),
             false, DataSourceUpdateMode.OnPropertyChanged);


        this.txtJoinPlayerFileFullPath.DataBindings.Add("Text", _config, nameof(AppConfig.JoinPlayerFileFullPath),
                false, DataSourceUpdateMode.OnPropertyChanged);


        this.txtJoinPlayerSplitChar.DataBindings.Add("Text", _config, nameof(AppConfig.JoinPlayerSplitChar),
            false, DataSourceUpdateMode.OnPropertyChanged);

        this.txtFrameWidth.DataBindings.Add("Text", _config, nameof(AppConfig.FrameWidth),
                     false, DataSourceUpdateMode.OnPropertyChanged);

        this.txtFrameHeight.DataBindings.Add("Text", _config, nameof(AppConfig.FrameHeight),
                             false, DataSourceUpdateMode.OnPropertyChanged);


        this.chkDevelopMode.DataBindings.Add("Checked", _config, nameof(AppConfig.DevelopMode),
            false, DataSourceUpdateMode.OnPropertyChanged);
    }

    private void openSaveFileDialog(System.Windows.Forms.TextBox txtfileSavePath, string defaultFileName)
    {

        string fileFullName = txtfileSavePath.Text;

        string fileName = defaultFileName;
        string folderName = Utility.GetDataFolder();

        if (!string.IsNullOrEmpty(fileFullName))
        {
            fileName = Path.GetFileName(fileFullName);
            folderName = Path.GetDirectoryName(fileFullName) ?? "";
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "テキストファイル (*.txt)|*.txt",
            FileName = fileName,
            InitialDirectory = folderName,
            OverwritePrompt = true,
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtfileSavePath.Text = Utility.GetRelativePath(dialog.FileName);
        }
    }

    /// <summary>
    /// 顔認証データ登録・判定
    /// </summary>
    /// <param name="videoCaptureID"></param>
    /// <param name="pictureBox"></param>
    private void facialRecognitionExecute(
        int videoCaptureID, PictureBox pictureBox,
        System.Windows.Forms.Label lblPlayerID,
        System.Windows.Forms.Label lblPlayerName,
        string playerFileName,
        System.Windows.Forms.Label lblLastPlayerID, //今使っていない
        ref DateTime lastDateTime)
    {
        Mat frame = new();

        ////frame = Cv2.ImRead("Test\\jpg\\test04.jpg");
        //if (playerFileName.Contains("2"))
        //{
        //    frame = Cv2.ImRead("");
        //}
        //else
        //{
        //    frame = Cv2.ImRead("");
        //}

        //顔認証データ登録・判定
        var (p, maxScore, _) = FacialRecognition.Execute(_config, videoCaptureID, ref frame);

        var displayPlayerId = "";
        var displayPlayerName = "";

        if (p != null)
        {

            displayPlayerId = p.PlayerId.ToString();
            displayPlayerName = p.PlayerName;

            if (string.IsNullOrEmpty(displayPlayerName))
            {
                displayPlayerName = _config.NewPlayerName;
            }

            var jpgFileName = Path.Combine(Utility.GetTempFolder(), p.PlayerId + ".jpg") ?? "";
            if (Path.Exists(jpgFileName) == false)
            {
                Cv2.ImWrite(jpgFileName, frame);
            }

            lblLastPlayerID.Text = displayPlayerId;
        }
        //else if (frame.Rows != 0)
        //{
        //    var jpgFileName = Path.Combine(Utility.GetTempFolder(), DateTime.Now.ToString("MMddhhmmss") + ".jpg") ?? "";
        //    Cv2.ImWrite(jpgFileName, frame);
        //}

        try
        {
            this.Invoke(() =>
            {
                if (frame.Rows != 0)
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = frame.ToBitmap();
                }

                pictureBox.Invalidate();

                lblPlayerID.Text = displayPlayerId + ":" + maxScore.ToString("0.000");
                lblPlayerName.Text = p == null ? "【識別なし】" : displayPlayerName;

            });
        }
        catch { }

        var passMinutes = false;
        if (Math.Abs((lastDateTime - DateTime.Now).TotalMinutes) >= 5)
        {
            passMinutes = true;
            lblLastPlayerID.Text = "";
        }

        //プレイヤー用のファイルを作成 　passMinutesの判定は5分ぐらい判定なしが続いたらの設定
        if (!string.IsNullOrEmpty(playerFileName))
        {
            if (!string.IsNullOrEmpty(displayPlayerName) || passMinutes)
            {
                lastDateTime = DateTime.Now;
                try
                {
                    Utility.WriteAllText(playerFileName, displayPlayerName, true);
                }
                catch { }
            }

        }

    }

    /// <summary>
    /// プレイヤー名登録画面を開く
    /// </summary>
    /// <param name="playerID"></param>
    private void openInputPlayer(string playerID)
    {
        var frm = new InputPlayerList();
        frm.ArgPlayerID = playerID;
        frm.ShowDialog();
    }

    #endregion


 
}