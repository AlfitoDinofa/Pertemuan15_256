using ExcelDataReader;
using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();
        DAL dbLogic = new DAL();

        public Form1()
        {
            InitializeComponent();
        }

        private void SimpanLog(string message)
        {
            dbLogic.InsertLog(message);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.Items.Clear();
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellClick += dataGridView1_CellClick;

            // Default KodeProdi
            txtKodeProdi.Text = "TI01";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = dbLogic.GetMhs();

                // Reset DataGridView
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
                dataGridView1.Refresh();

                HitungTotal();
                dataGridView1.Enabled = true;

                // ========== OTOMATIS PILIH BARIS PERTAMA ==========
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[0].Selected = true;

                    // Isi TextBox secara manual (agar data langsung muncul)
                    DataRow row = dt.Rows[0];
                    txtNIM.Text = row["NIM"].ToString();
                    txtNama.Text = row["Nama"].ToString();
                    cmbJK.Text = row["JenisKelamin"].ToString();
                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);
                    txtAlamat.Text = row["Alamat"].ToString();
                    txtKodeProdi.Text = row["NamaProdi"].ToString();

                    if (row["Foto"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row["Foto"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
                // ====================================================
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("ERROR LoadData: " + ex.Message);  // ← TAMPILKAN ERROR
            }
        }

        private void HitungTotal()
        {
            try
            {
                object result = dbLogic.CountMhs();
                int total = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                lblTotal.Text = "Total Mahasiswa : " + total;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Text = "TI01";
            dtpTanggalLahir.Value = DateTime.Now;
            pictureBox1.Image = null;
            txtNIM.Focus();
        }

        private byte[] ConvertImageToBytes(PictureBox pb)
        {
            if (pb.Image == null)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // ========== SIMPAN DALAM FORMAT PNG (LEBIH AMAN) ==========
                    pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                SimpanLog("Gagal konversi gambar: " + ex.Message);
                return null;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbLogic.GetConnectionString()))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil");
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text))
                {
                    MessageBox.Show("NIM tidak boleh kosong!");
                    txtNIM.Focus();
                    return;
                }

                // ========== CEK GAMBAR DULU ==========
                byte[] imgBytes = null;
                if (pictureBox1.Image != null)
                {
                    try
                    {
                        imgBytes = ConvertImageToBytes(pictureBox1);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memproses gambar: " + ex.Message);
                        return;
                    }
                }
                // =====================================

                dbLogic.InsertMhs(
                    txtNIM.Text,
                    txtNama.Text,
                    txtAlamat.Text,
                    cmbJK.Text,
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text,
                    imgBytes
                );

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text))
                {
                    MessageBox.Show("Pilih data yang akan diupdate!");
                    return;
                }

                // ========== CEK APAKAH ADA GAMBAR BARU ==========
                byte[] imgBytes = null;
                bool hasNewImage = pictureBox1.Image != null;

                if (hasNewImage)
                {
                    try
                    {
                        imgBytes = ConvertImageToBytes(pictureBox1);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memproses gambar: " + ex.Message);
                        return;
                    }
                }
                // ================================================

                // ========== KIRIM NULL JIKA TIDAK ADA GAMBAR BARU ==========
                // TAPI di DAL.cs, jika foto == null, JANGAN update kolom Foto!
                // ============================================================

                dbLogic.UpdateMhs(
                    txtNIM.Text,
                    txtNama.Text,
                    txtAlamat.Text,
                    cmbJK.Text,
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text,
                    imgBytes,       // ← NULL jika tidak ada gambar baru
                    hasNewImage     // ← TAMBAHKAN PARAMETER INI!
                );

                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text))
                {
                    MessageBox.Show("Pilih data yang akan dihapus!");
                    return;
                }

                DialogResult dg = MessageBox.Show(
                    "Yakin ingin menghapus data?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNIM.Text))
                {
                    MessageBox.Show("Masukkan NIM terlebih dahulu!");
                    return;
                }

                dbLogic.testInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed");
                }
                else
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    txtNIM.Text = row.Cells["NIM"].Value?.ToString() ?? "";
                    txtNama.Text = row.Cells["Nama"].Value?.ToString() ?? "";
                    cmbJK.Text = row.Cells["JenisKelamin"].Value?.ToString() ?? "";

                    if (row.Cells["TanggalLahir"].Value != null && row.Cells["TanggalLahir"].Value != DBNull.Value)
                    {
                        dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
                    }

                    txtAlamat.Text = row.Cells["Alamat"].Value?.ToString() ?? "";
                    txtKodeProdi.Text = row.Cells["NamaProdi"].Value?.ToString() ?? "";

                    if (row.Cells["Foto"].Value != null && row.Cells["Foto"].Value != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])row.Cells["Foto"].Value;
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Error saat memilih data: " + ex.Message);
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(ofd.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat gambar: " + ex.Message);
                }
            }
        }

        private void btnRekap_Click(object sender, EventArgs e)
        {
            Form2 fm3 = new Form2();
            fm3.Show();
            this.Hide();
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Workbook|*.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                    {
                                        UseHeaderRow = true
                                    }
                                });
                                DataTable dt = result.Tables[0];
                                dataGridView1.DataSource = dt;
                                dataGridView1.Enabled = false;

                                btnDatabase.Enabled = true;
                                button3.Enabled = false;
                                button4.Enabled = false;
                                button5.Enabled = false;
                                btnCari.Enabled = false;
                                btnLoad.Enabled = false;
                                btnResetData.Enabled = false;
                                btnTestInjection.Enabled = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal import Excel: " + ex.Message);
                    }
                }
            }
        }

        private void btnDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                int gagal = 0;

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string nim = row["NIM"].ToString().Trim();
                        string nama = row["Nama"].ToString().Trim();
                        string jk = row["JenisKelamin"].ToString().Trim();
                        string alamat = row["Alamat"].ToString().Trim();
                        string kodeProdi = row["NamaProdi"].ToString().Trim();
                        string fotoPath = row.Table.Columns.Contains("FotoPath")
                            ? row["FotoPath"].ToString().Trim()
                            : string.Empty;

                        if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                        {
                            gagal++;
                            continue;
                        }

                        DateTime tglLahir;
                        if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                        {
                            gagal++;
                            continue;
                        }

                        byte[] fotoBytes = null;
                        if (!string.IsNullOrWhiteSpace(fotoPath) && File.Exists(fotoPath))
                        {
                            fotoBytes = File.ReadAllBytes(fotoPath);
                        }

                        dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                        sukses++;
                    }
                    catch
                    {
                        gagal++;
                    }
                }

                MessageBox.Show($"Import selesai!\nSukses: {sukses} data\nGagal: {gagal} data");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("General Error : " + ex.Message);
                MessageBox.Show("General Error : " + ex.Message);
            }
        }
    }
}