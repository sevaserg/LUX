using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
namespace LUXCOM
{

    public partial class Form1 : Form
    {
        string exstates = "";
        SerialPort sp = new SerialPort();
        Button[] settingButtons = new Button[20];
        Button LoadBtn = new Button();
        Button SaveBtn = new Button();
        Button RecBtn = new Button();
        Button PlayPauseBtn = new Button();
        CheckBox[] checkEnabled = new CheckBox[20];
        int set_btn = -1;
        List<string> pressedBtns = new List<string>();
        string[] data = new string[20];
        string[] buttonsSettings = new string[20];
        int tickCount = 0;
        Timer t = new Timer();
        bool recording = false;
        bool playing = false;

        public Form1()
        {
            InitializeComponent();
            this.AutoScroll = true;
            textBox1.Enabled = false;
            textBox1.Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            tlp1.RowCount = 10;
            tlp1.ColumnCount = 4;
            tlp1.AutoSize = true;
            tlp1.BorderStyle += 2;
            tlp1.Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            for (int i = 0; i < 20; i++)
            {
                data[i] = "";
                checkEnabled[i] = new CheckBox();
                tlp1.Controls.Add(checkEnabled[i], i < 10 ? 0 : 2, i % 10);
                settingButtons[i] = new Button();
                settingButtons[i].Text = (i+1).ToString();
                tlp1.Controls.Add(settingButtons[i], i < 10 ? 1 : 3, i % 10);
                settingButtons[i].Click += new System.EventHandler(Btn_Click);
            }
            this.tlp1.ColumnStyles[0].Width = 20;
            this.tlp1.ColumnStyles[1].Width = 80;
            this.tlp1.ColumnStyles[2].Width = 20;
            this.tlp1.ColumnStyles[3].Width = 80;
            for (int i = 0; i < 10; i++)
            {
                this.tlp1.RowStyles[i].Height = 30;
                this.tlp1.RowStyles[i].SizeType = SizeType.Absolute;
            }
            for (int i = 0; i < 4; i++)
                this.tlp1.ColumnStyles[i].SizeType = SizeType.Absolute;
            this.tlp2.RowCount = 2;
            this.tlp2.ColumnCount = 2;
            for (int i = 0; i < 2; i++)
            {
                this.tlp2.RowStyles[i].Height = 30;
                this.tlp2.RowStyles[i].SizeType = SizeType.Absolute;
                this.tlp2.ColumnStyles[i].Width = 100;
                this.tlp2.ColumnStyles[i].SizeType = SizeType.Absolute;
            }
            this.tlp2.Controls.Add(SaveBtn, 0, 0);
            SaveBtn.Anchor = AnchorStyles.None;
            SaveBtn.Width = 70;
            SaveBtn.Text = "Save";
            this.tlp2.Controls.Add(LoadBtn, 1, 0);
            LoadBtn.Anchor = AnchorStyles.None;
            LoadBtn.Width = 70;
            LoadBtn.Text = "Load";
            this.tlp2.Controls.Add(PlayPauseBtn, 0, 1);
            PlayPauseBtn.Anchor = AnchorStyles.None;
            PlayPauseBtn.Width = 70;
            PlayPauseBtn.Text = "Play";
            this.tlp2.Controls.Add(RecBtn, 1, 1);
            RecBtn.Anchor = AnchorStyles.None;
            RecBtn.Width = 70;
            RecBtn.Text = "Record";

            this.KeyPreview = true;
            this.KeyUp += Form1_KeyUp;
            this.KeyDown += Form1_KeyDown;
            t.Interval = 10;
            t.Tick += new System.EventHandler(tick_event);
            RecBtn.Click += new System.EventHandler(record);
            PlayPauseBtn.Click += new System.EventHandler(playPause);
            SaveBtn.Click += new System.EventHandler(save);
            LoadBtn.Click += new System.EventHandler(load);

            connectBtn.Click += new System.EventHandler(serConnect);
            string[] portsList = SerialPort.GetPortNames();
            portsCombo.Items.Add("---");
            portsCombo.Items.AddRange(portsList);
            portsCombo.SelectedIndex = 0;
            t.Start();
        }
        
        private void sendState(string states)
        {
            if (sp.IsOpen && states != exstates)
            {
                exstates = states;
                sp.WriteLine(states);
            }
        }
        private void serConnect(object sender, EventArgs e)
        {
            if (sp.IsOpen)
                sp.Close();
            if (portsCombo.SelectedItem.ToString() != "---" )
            {
                print(portsCombo.SelectedItem.ToString());
                
                sp = new SerialPort(portsCombo.SelectedItem.ToString(),
                                    115200, Parity.None, 8, StopBits.One);
                try
                {
                    sp.Open();
                    sp.WriteLine("Hello!");
                }
                catch
                {
                    print("Unable to open port!");
                }
            }
            else
            {
                print("None selected!");
            }
        }
        private void record(object sender, EventArgs e)
        {
            
            if(recording)
            {
                recording = false;
                for(int i = 0; i < 20; i++)
                {
                    settingButtons[i].Enabled = true;
                    checkEnabled[i].Enabled = true;
                }
                LoadBtn.Enabled = true;
                SaveBtn.Enabled = true;
                PlayPauseBtn.Enabled = true;

                for (int i = 0; i < 20; i++)
                {
                    while(data[i].Length < tickCount)
                    {
                        data[i] += '0';
                    }
                }
                tickCount = 0;
            }
            else
            {
                tickCount = 0;
                for (int i = 0; i < 20; i++)
                {
                    if (checkEnabled[i].Checked)
                        data[i] = "";
                    settingButtons[i].Enabled = false;
                    checkEnabled[i].Enabled = false;
                }
                LoadBtn.Enabled = false;
                SaveBtn.Enabled = false;
                PlayPauseBtn.Enabled = false;
                recording = true;
            }
        }
        private void playPause(object sender, EventArgs e)
        {
            if (playing)
            {
                tickCount = 0;
                playing = false;
                for (int i = 0; i < 20; i++)
                {
                    settingButtons[i].Enabled = true;
                    checkEnabled[i].Enabled = true;
                }
                LoadBtn.Enabled = true;
                SaveBtn.Enabled = true;
                RecBtn.Enabled = true;
                PlayPauseBtn.Text = "Play";
            }
            else
            {
                if (data.Length > 0)
                { 
                    tickCount = 0;
                    for (int i = 0; i < 20; i++)
                    {
                        settingButtons[i].Enabled = false;
                        checkEnabled[i].Enabled = false;
                    }
                    LoadBtn.Enabled = false;
                    SaveBtn.Enabled = false;
                    RecBtn.Enabled = false;
                    PlayPauseBtn.Text = "Stop";
                    playing = true;
                }
            }
        }
        private void print(string s)
        {
            textBox1.Text = s;
        }
        private void tick_event(object sender, EventArgs e)
        {
            if (recording)
            {
                string st = "";
                for (int i = 0; i < 20; i++)
                {
                    if (checkEnabled[i].Checked)
                    { 
                        if (pressedBtns.Contains(buttonsSettings[i]))
                            data[i]+="1";
                        else
                            data[i]+="0";
                        st += data[i].ElementAt(tickCount);
                    }
                    else
                    {
                        if (data[i].Length <= tickCount)
                            st += "0";
                        else
                            st += data[i].ElementAt(tickCount);
                    }
                }
                print(st);
                sendState(st);
                tickCount++;
                if (tickCount == 0xFFFFFF) record(null, null);
            }
            if (playing)
            {
                string st = "";
                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine(i.ToString() + " " + tickCount+"\n");
                    Console.WriteLine(data.Length + " " + data[i].Length + "\n");
                    st += data[i][tickCount];
                }
                print(st);
                sendState(st);
                tickCount++;
                if (tickCount == 0xFFFFFF || tickCount == data[0].Length-1)  playPause(null, null);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(pressedBtns.Contains(e.KeyCode.ToString())))
            {
                if (set_btn == -1)
                {
                    pressedBtns.Add(e.KeyCode.ToString());
                    //textBox1.Text = ($"KeyDown code: {e.KeyCode}, value: {e.KeyValue}, modifiers: {e.Modifiers}");
                    
                }
                else
                {
                    pressedBtns.Add(e.KeyCode.ToString());
                    textBox1.Text = ( "LED" + (set_btn+1).ToString()+" set to button "+ e.KeyCode.ToString());
                    buttonsSettings[set_btn] = e.KeyCode.ToString();
                    set_btn = -1;
                }
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (pressedBtns.Contains(e.KeyCode.ToString()))
            {
                pressedBtns.RemoveAt(pressedBtns.IndexOf(e.KeyCode.ToString()));
                //textBox1.Text = ($"KeyUp code: {e.KeyCode}, value: {e.KeyValue}, modifiers: {e.Modifiers}");
            }
        }
        private void Btn_Click(object sender, EventArgs e)
        {

            string T = ((Button)sender).Text;
            set_btn = int.Parse(T) - 1;
            textBox1.Text = ("Waiting for input...");
        }
        private void save(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDialog.ShowDialog();
            string filepath = saveDialog.FileName.ToString();
            
            File.WriteAllText(filepath, "");
            int amt = 0;
            string s1 = "", s2 = "";
            for (int i = 0; i < data[0].Length-1; i++)
            {
                s1 = "";
                s2 = "";
                for (int j = 0; j < 20; j++)
                {
                    s1 += data[j][i];
                    s2 += data[j][i+1];
                }
                if (s1 == s2)
                    amt++;
                else
                {
                    File.AppendAllText(filepath, s1+" "+(amt+1).ToString()+'\n');
                    amt = 0;
                }
            }
            File.AppendAllText(filepath, s2 + " " + (amt + 1).ToString() + '\n');
            print("Saved to "+filepath);
        }
        private void load(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openDialog.ShowDialog();
            string filepath = openDialog.FileName.ToString();
            string s = File.ReadAllText(filepath);
            string[]s_array = s.Split('\n');
            for (int i = 0; i < 20; i++)
                data[i] = "";
            for (int i = 0; i < s_array.Length - 1; i++)
            {
                string values = s_array[i].Split(' ')[0];
                int amount = int.Parse(s_array[i].Split(' ')[1]);
                for (int j = 0; j < 20; j++)
                    for (int a = 0; a < amount; a++)
                        data[j] += values[j];
            }
            print("Loaded from " + filepath);
        }
    }
}
