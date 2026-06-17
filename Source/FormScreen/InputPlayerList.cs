namespace FacialRecognitionSystem.FormScreen;

using FacialRecognitionSystem.UtilityFunction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




public partial class InputPlayerList : Form
{
    /// <summary>configファイル</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    private AppConfig _config;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ArgPlayerID { get; set; } = "";

    public InputPlayerList()
    {

        _config = AppConfig.Load();

        //this.Visible = false;
        InitializeComponent();

        setupGrid();

    }

    private void setupGrid()
    {
        // DataGridView 設定
        grdPlayerList.AllowUserToAddRows = true;
        grdPlayerList.RowTemplate.Height = 80;

        // ID列
        DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
        idCol.HeaderText = "id";
        idCol.DataPropertyName = nameof(PlayerData.PlayerId);
        idCol.Width = 50;
        idCol.ReadOnly = true;
        grdPlayerList.Columns.Add(idCol);

        // 画像列
        DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
        imgCol.Name = "ImageColumn";
        imgCol.HeaderText = "Image";
        imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
        imgCol.DataPropertyName = nameof(PlayerData.PlayerId);
        imgCol.DefaultCellStyle.NullValue = null;
        imgCol.Width = 100;
        grdPlayerList.Columns.Add(imgCol);

        // 名前列
        DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
        nameCol.HeaderText = "PlayerName";
        nameCol.DataPropertyName = nameof(PlayerData.PlayerName);
        nameCol.Width = 200;
        grdPlayerList.Columns.Add(nameCol);

        DataGridViewTextBoxColumn idMaxScore = new DataGridViewTextBoxColumn();
        idMaxScore.HeaderText = "MaxScore ";
        idMaxScore.DataPropertyName = nameof(PlayerData.MaxScore);
        idMaxScore.Width = 50;
        idMaxScore.DefaultCellStyle.Format = "0.000";
        idMaxScore.ReadOnly = true;
        idMaxScore.Visible = _config.DevelopMode ;
        grdPlayerList.Columns.Add(idMaxScore);

        // --- ヘッダー編集を可能にする設定 ---
        grdPlayerList.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        grdPlayerList.ColumnHeadersHeight = 30;
        grdPlayerList.EditMode = DataGridViewEditMode.EditOnEnter;

        // --- イベント登録 ---
        grdPlayerList.CellFormatting += grdPlayerList_CellFormatting;
        grdPlayerList.CellClick += grdPlayerList_CellClick;
        //grdPlayerList.CellEndEdit += grdPlayerList_CellEndEdit; // 編集完了
        //grdPlayerList.LostFocus += grdPlayerList_LostFocus;
    }

    private void serchData()
    {
        //if (this.Visible == false)
        //{
        //    return;
        //}
        //this.dataGridView1.Rows.Clear();

        var searchList = PlayerDataStorage.PlayerDataDB;

        //最近追加(ファイル名があるものだけ)
        if (this.rdoRecently.Checked == true)
        {
            var fileIdList = Utility.GetTempIdList();
            searchList = new PlayerDataList(searchList.Where(w => fileIdList.Contains(w.PlayerId.ToString())));
        }

        if (!string.IsNullOrWhiteSpace(this.txtPlayerName.Text))
        {
            searchList = new PlayerDataList(searchList.Where(w => w.PlayerName.Contains(this.txtPlayerName.Text)));
        }

        //引き数に　PlayerID　がセットされている場合は先頭に表示
        int argPlayerID = int.MinValue;
        if (!string.IsNullOrEmpty(this.ArgPlayerID))
        {

            if (int.TryParse(this.ArgPlayerID, out argPlayerID))

            {
                argPlayerID = int.MinValue;
            }

        }

        searchList = new PlayerDataList(searchList.OrderBy(o =>
        {
            var orderValue = o.PlayerId == argPlayerID ? int.MaxValue : o.PlayerId;

            if (this.rdoRecently.Checked == true)   //最近追加は新しいものを最初に表示
            {
                orderValue *= -1;
            }

            return orderValue;
        }
        ));


        this.grdPlayerList.DataSource = searchList;
    }

    private void grdPlayerList_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (grdPlayerList.Columns[e.ColumnIndex].Name == "ImageColumn")
        {
            var imageFile = Path.Combine(Utility.GetTempFolder(), e.Value + ".jpg");

            if (string.IsNullOrEmpty(imageFile) || File.Exists(imageFile) == false)
            {
                imageFile = Path.Combine(AppContext.BaseDirectory, "Resource", "noimage.jpg");
            }

            try
            {
                // ファイルから画像を読み込む
                e.Value = Image.FromFile(imageFile);
            }
            catch
            {
                e.Value = null; // 読み込み失敗時
            }

            e.FormattingApplied = true;

        }
    }


    private void grdPlayerList_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        // 行ヘッダーや列ヘッダーは無視
        if (e.RowIndex < 0)
        {
            return;
        }

        //// 名前列なら編集開始
        //if (e.ColumnIndex == 2)
        //{
        //    this.grdPlayerList.CurrentCell = grdPlayerList[e.ColumnIndex, e.RowIndex];

        //    this.grdPlayerList.BeginEdit(true);
        //}
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (this.grdPlayerList.IsCurrentCellInEditMode)
        {
            this.grdPlayerList.EndEdit();
        }

        PlayerDataStorage.SavePlayerData();
    }

    private void rdoAll_CheckedChanged(object sender, EventArgs e)
    {
        if (this.isShown == true)
        {
            return;
        }
        serchData();
    }

    private void rdoRecently_CheckedChanged(object sender, EventArgs e)
    {
        if (this.isShown == true)
        {
            return;
        }
        serchData();
    }

    private void txtPlayerName_TextChanged(object sender, EventArgs e)
    {
        if (this.isShown == true)
        {
            return;
        }
        serchData();
    }

    private bool isShown = true;
    private void InputPlayerList_Shown(object sender, EventArgs e)
    {
        serchData();
        this.isShown = false;
    }

}
