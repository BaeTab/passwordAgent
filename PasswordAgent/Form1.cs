using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;


namespace PasswordAgent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initControl();
            listBox1.Items.Clear();
            loadUrlsToListBox();
        }

        private void initControl()
        {
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            button6.Click += Button6_Click;

            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        }

        public string filePath = "pwAgent.xml";


        private void Button1_Click(object sender, EventArgs e) // 데이터 저장을 위한 버튼
        {
            string id = textBox1.Text;
            string pw = EncryptionHelper.EncryptString(textBox2.Text);
            string url = textBox3.Text;

            try
            {
                if (string.Empty == filePath || string.Empty == id || string.Empty == pw || string.Empty == url)
                {
                    MessageBox.Show("빈 값이 있습니다", "오류");
                    return;
                }
                else
                {
                    if (!System.IO.File.Exists(filePath))
                    {
                        SavePwAgentToXml(id, pw, url, filePath);
                        MessageBox.Show("저장 되었습니다", "알림");
                    }
                    else
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(filePath);

                        // 중복 체크
                        XmlNodeList existingpwAgent = doc.SelectNodes($"//pwAgent[url='{url}']");
                        if (existingpwAgent.Count > 0)
                        {
                            MessageBox.Show("해당 URL 정보는 저장되어 있습니다", "알림");
                        }
                        else
                        {
                            SavePwAgentToXml(id, pw, url, filePath);
                            MessageBox.Show("저장 되었습니다", "알림");
                        }
                    }
                }
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();

                listBox1.Items.Clear();
                loadUrlsToListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message, "오류");
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string selectedUrl = listBox1.SelectedItem?.ToString();

            try
            {
                if (!string.IsNullOrEmpty(selectedUrl))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    XmlNodeList nodesToRemove = doc.SelectNodes($"//pwAgent[url='{selectedUrl}']");
                    if (null == nodesToRemove) return;

                    foreach (XmlNode node in nodesToRemove)
                    {
                        node.ParentNode.RemoveChild(node);
                    }

                    if (DialogResult.Yes == MessageBox.Show("정말 삭제 하시겠습니까?", "정보", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    {
                        doc.Save(filePath);

                        textBox4.Clear();
                        textBox5.Clear();
                        textBox6.Clear();

                        listBox1.Items.Clear();
                        loadUrlsToListBox();
                    }
                }
                else
                {
                    MessageBox.Show("먼저 리스트에서 삭제할 아이템을 선택해 주세요", "알림",MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류" + ex.Message, "알림");
            }
            return;
        }

        private void Button3_Click(object sender, EventArgs e) // 아이디 복사버튼
        {
            if (!string.IsNullOrEmpty(textBox5.Text))
            {
                Clipboard.SetText(textBox5.Text);
            }
            else
            {
                label7.Visible = true;
                label7.Text = "먼저 리스트에서 확인할 아이템을 선택해 주세요";
            }
        }

        private void Button4_Click(object sender, EventArgs e) // 비밀번호 복사버튼
        {
            if (!string.IsNullOrEmpty(textBox6.Text))
            {
                Clipboard.SetText(textBox6.Text);
            }
            else
            {
                label7.Visible = true;
                label7.Text = "먼저 리스트에서 확인할 아이템을 선택해 주세요";
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            string currentFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            System.Diagnostics.Process.Start(currentFolderPath);
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedUrl = listBox1.SelectedItem.ToString();

                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNodeList pwAgentlNodes = doc.SelectNodes($"//pwAgent[url='{selectedUrl}']");

                if (pwAgentlNodes.Count > 0)
                {
                    XmlNode selectedNode = pwAgentlNodes[0];

                    string id = selectedNode.SelectSingleNode("id").InnerText;
                    string password = EncryptionHelper.DecryptString(selectedNode.SelectSingleNode("password").InnerText);
                    string url = selectedNode.SelectSingleNode("url").InnerText;

                    textBox4.Text = url;
                    textBox5.Text = id;
                    textBox6.Text = password;
                }
                else
                {
                    textBox5.Text = "";
                    textBox6.Text = "";
                }
            }
        }

        private void SavePwAgentToXml(string id, string pw, string url, string filePath)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                if (File.Exists(filePath))
                {
                    doc.Load(filePath);
                }
                else
                {
                    XmlElement root = doc.CreateElement("pwAgents");
                    doc.AppendChild(root);
                }

                XmlElement pwAgent = doc.CreateElement("pwAgent");

                XmlElement idElement = doc.CreateElement("id");
                idElement.InnerText = id;
                pwAgent.AppendChild(idElement);

                XmlElement passwordElement = doc.CreateElement("password");
                passwordElement.InnerText = pw;
                pwAgent.AppendChild(passwordElement);

                XmlElement urlElement = doc.CreateElement("url");
                urlElement.InnerText = url;
                pwAgent.AppendChild(urlElement);

                doc.DocumentElement?.AppendChild(pwAgent);

                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류" + ex.Message, "오류");
            }
        }

        private void searchURLFromXml(string url, string filePath)
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(filePath))
            {
                doc.Load(filePath);

                // URL에 대한 자격 증명을 찾습니다.
                XmlNodeList pwAgents = doc.SelectNodes($"//pwAgent[url='{url}']");
                if (pwAgents.Count == 1)
                {
                    if (InvokeRequired == true)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            foreach (XmlNode pwAgentNode in pwAgents)
                            {
                                string id = pwAgentNode.SelectSingleNode("id").InnerText;
                                string password = pwAgentNode.SelectSingleNode("password").InnerText;

                                textBox5.Text = id;
                                textBox6.Text = password;
                            }
                        }));
                    }
                    else
                    {
                        foreach (XmlNode pwAgentNode in pwAgents)
                        {
                            string id = pwAgentNode.SelectSingleNode("id").InnerText;
                            string password = pwAgentNode.SelectSingleNode("password").InnerText;

                            textBox5.Text = id;
                            textBox6.Text = password;
                        }
                    }
                }
                else
                {
                    label7.Visible = true;
                    label7.Text = "입력한 URL 을 찾을수 없습니다";
                }
            }
            else
            {
                label7.Visible = true;
                label7.Text = "파일을 찾을 수 없습니다.";
            }
        }

        private void loadUrlsToListBox()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNodeList pwAgentlNodes = doc.SelectNodes("//pwAgent/url");

                foreach (XmlNode node in pwAgentlNodes)
                {
                    listBox1.Items.Add(node.InnerText);
                }
            }
            catch
            {
                MessageBox.Show("아직 표시할 목록이 없습니다. \n관리하실 께정 정보를 등록해 주세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public class EncryptionHelper
        {
            private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF"); // 16바이트 (AES-128)
            private static readonly byte[] IV = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };

            public static string EncryptString(string plainText)
            {
                if (string.IsNullOrEmpty(plainText))
                    throw new ArgumentException("입력된 문자열이 null 또는 비어 있습니다.");

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // AES 알고리즘을 사용하여 암호화 객체를 생성합니다.
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // 스트림을 생성하여 암호화 데이터를 쓰고 읽습니다.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // 암호화된 데이터를 쓰기합니다.
                                swEncrypt.Write(plainText);
                            }
                        }

                        // 암호화된 데이터를 바이트 배열로 변환합니다.
                        byte[] encryptedBytes = msEncrypt.ToArray();
                        // 바이트 배열을 Base64 문자열로 변환하여 반환합니다.
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }

            public static string DecryptString(string encryptedText)
            {
                byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // AES 알고리즘을 사용하여 복호화 객체를 생성합니다.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // 스트림을 생성하여 복호화 데이터를 쓰고 읽습니다.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // 복호화된 데이터를 읽어옵니다.
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
