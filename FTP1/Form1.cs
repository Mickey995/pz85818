using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace FTP1
{
    public partial class Form1 : Form
    {
        string ftpServerIP;
        string ftpUserID;
        string ftpPassword;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ftpServerIP = "127.0.0.1";
            ftpUserID = "administrator";
            ftpPassword = "xxxxx";
            txtServerIP.Text = ftpServerIP;
            txtUsername.Text = ftpUserID;
            txtPassword.Text = ftpPassword;
            this.Text += ftpServerIP;

            btnFTPSave.Enabled = false;
        }
        /// upload wybranego pliku do wybranego serwera FTP
        /// name="filename" - > pełna nazwa pliku
        private void Upload(string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
            FtpWebRequest reqFTP;

            // Tworzenie obiektu FtpWebRequest z podanego URI 
            //https://msdn.microsoft.com/pl-pl/library/system.uri(v=vs.110).aspx 
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + fileInf.Name));

            // login i hasło dla serwera
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

           
            reqFTP.KeepAlive = false;

            // określenie jaki request ma zostać wykonany
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // typ przesyłu pliku, w tym wypadku binarny.
            reqFTP.UseBinary = true;

            // informacja dla serwera o wielkości pliku
            reqFTP.ContentLength = fileInf.Length;

            // wielkość buforu
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // otwiera strumień do odczytu pliku
            FileStream fs = fileInf.OpenRead();

            try
            {
                // strumień do którego plik ma zostać zapisany
                Stream strm = reqFTP.GetRequestStream();

                // czytaj plik zgodnie z wielkością buforu
                contentLen = fs.Read(buff, 0, buffLength);

                // tak długo aż nie skończy się strumień
                while (contentLen != 0)
                {
                    // Zapisz dane z bufora do strumienia 
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // zamknij strumienie
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "błąd podczas udostępniania pliku");
            }
        }

        public void DeleteFTP(string fileName)
        {
            try
            {
                string uri = "ftp://" + ftpServerIP + "/" + fileName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + fileName));

                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string[] GetFilesDetailList()
        {
            string[] downloadFiles;
            try
            {
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/"));
                ftp.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }

                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
                //MessageBox.Show(result.ToString().Split('\n'));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                downloadFiles = null;
                return downloadFiles;
            }
        }

        public string[] GetFileList()
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //MessageBox.Show(reader.ReadToEnd());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                //MessageBox.Show(response.StatusDescription);
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                downloadFiles = null;
                return downloadFiles;
            }
        }
        private void Download(string filePath, string fileName)
        {
            FtpWebRequest reqFTP;
            try
            {
                //filePath = <<ścieżka w której plik ma zostać utworzony.>>, 
                //fileName = <<nazwa pliku który ma zostać utworzony.>>
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog opFilDlg = new OpenFileDialog();
            if (opFilDlg.ShowDialog() == DialogResult.OK)
            {
                new Thread(() =>
                {
                    Upload(opFilDlg.FileName);
                }).Start();
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fldDlg = new FolderBrowserDialog();
            if (txtUpload.Text.Trim().Length > 0)
            {
                
                if (fldDlg.ShowDialog() == DialogResult.OK)
                {
                    new Thread(() =>
                    {
                        Download(fldDlg.SelectedPath, txtUpload.Text.Trim());
                    }).Start();
                }
                
            }
            else
            {
                MessageBox.Show("Proszę o wprowadzenie nazwy pliku lub wybranie go z listy");
            }
        }

        private void btnLstFiles_Click(object sender, EventArgs e)
        {
            string[] filenames = GetFileList();
            lstFiles.Items.Clear();
            {
                foreach (string filename in filenames)
                {
                    lstFiles.Items.Add(filename);
                }
            } 


        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            OpenFileDialog fldDlg = new OpenFileDialog();
            if (txtUpload.Text.Trim().Length > 0)
            {
                DeleteFTP(txtUpload.Text.Trim());
            }
            else
            {
                MessageBox.Show("proszę o wprowadzenie nazwy pliku lub wybranie go z listy");
            }
        }

        private long GetFileSize(string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return fileSize;
        }

        private void Rename(string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MakeDir(string dirName)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName =nazwa nowego katalogu.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnFileSize_Click(object sender, EventArgs e)
        {
            long size = GetFileSize(txtUpload.Text.Trim());
            MessageBox.Show(size.ToString() + " bajtów");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rename(txtCurrentFilename.Text.Trim(), txtNewFilename.Text.Trim());
        }

        private void btnewDir_Click(object sender, EventArgs e)
        {
            MakeDir(txtNewDir.Text.Trim());
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            btnFTPSave.Enabled = true;
        }

        private void txtServerIP_TextChanged(object sender, EventArgs e)
        {
            btnFTPSave.Enabled = true;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            btnFTPSave.Enabled = true;
        }

        private void btnFTPSave_Click(object sender, EventArgs e)
        {
            ftpServerIP = txtServerIP.Text.Trim();
            ftpUserID = txtUsername.Text.Trim();
            ftpPassword = txtPassword.Text.Trim();
            btnFTPSave.Enabled = false;
        }

        private void btnFileDetailList_Click(object sender, EventArgs e)
        {
            string[] filenames = GetFilesDetailList();
            lstFiles.Items.Clear();
            foreach (string filename in filenames)
            {
                lstFiles.Items.Add(filename);
            }
        }

        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtUpload.Text = lstFiles.GetItemText(lstFiles.SelectedItem);
            txtCurrentFilename.Text = lstFiles.GetItemText(lstFiles.SelectedItem);
        }
            
    }
}