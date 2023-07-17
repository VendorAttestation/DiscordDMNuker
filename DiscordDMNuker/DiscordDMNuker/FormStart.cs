using System;
using System.Windows.Forms;
namespace DiscordDMNuker
{
    public partial class FormStart : Form
    {
        public bool Start = false;
        public ulong MainId;
        public ulong ChannelId;
        public string Token;
        public string Message;
        public int Delay;
        public bool SavePicsNVids;
        public bool SaveMessages;
        public bool Delete;
        public bool Edit;
        public bool IsGroupChat;
        public bool IsChannel;
        public FormStart()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Start = true;
            MainId = Convert.ToUInt64(textBox1.Text.Trim());
            Token = textBox2.Text.Trim();
            SavePicsNVids = checkBox1.Checked;
            SaveMessages = checkBox2.Checked;
            Delete = checkBox3.Checked;
            IsGroupChat = checkBox4.Checked;
            Edit = checkBox5.Checked;
            Message = textBox3.Text.Trim();
            IsChannel = checkBox6.Checked;
            Properties.Settings.Default.Token = textBox2.Text;
            Properties.Settings.Default.Save();
            if (IsChannel)
            {
                ChannelId = Convert.ToUInt64(textBox4.Text.Trim());
            }

            Close();
        }

        private void FormStart_Load(object sender, EventArgs e)
        {
            try
            {
                textBox2.Text = Properties.Settings.Default.Token;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Failed to get token automatically are you using canary? If not please get your token manually if so report this bug in issues on github");
            }
        }
    }
}
