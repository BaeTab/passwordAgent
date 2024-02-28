using System;
using System.IO;
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
            loadUrlsToListBox();
        }

        private void initControl()
        {
            button1.Click += Button1_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            button6.Click += Button6_Click;
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        }

        public string filePath = "pwAgent.xml";


        private void Button1_Click(object sender, EventArgs e) // 데이터 저장을 위한 버튼
        {
            string id = textBox1.Text;
            string pw = textBox2.Text;
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
                loadUrlsToListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message, "오류");
            }
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
                doc.Load("pwAgent.xml");

                XmlNodeList pwAgentlNodes = doc.SelectNodes($"//pwAgent[url='{selectedUrl}']");

                if (pwAgentlNodes.Count > 0)
                {
                    XmlNode selectedNode = pwAgentlNodes[0];

                    string id = selectedNode.SelectSingleNode("id").InnerText;
                    string password = selectedNode.SelectSingleNode("password").InnerText;
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
                MessageBox.Show("아직 표시할 목록이 없습니다. \n관리하실 비밀번호를 등록해 주세요","알림",MessageBoxButtons.OK , MessageBoxIcon.Information);
            }
        }
    }
}
