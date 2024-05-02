using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using WindowsInput;
using System.IO.Ports;
using Newtonsoft.Json;
using System.IO;

namespace macro_testen
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public string Poort { get; set; }
        bool geenComPoort = true;

        private readonly InputSimulator simulator;
        private SavedData data;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSerialPort();
            simulator = new InputSimulator();
            data = JsonManager.LoadData();
            LaadData();
        }
        private void InitializeSerialPort()
        {
            while (geenComPoort == true) // blijft zoeken achter COM-poort indien geen gevonden
            {
                // zoek alle mogelijke poorten
                String[] Ports = SerialPort.GetPortNames();

                foreach (string poort in Ports) // overloopt alle gevonden COM-poorts
                {
                    SerialPort port = new SerialPort(poort, 9600);
                    try // opent poort en zoekt achter data
                    {
                        port.Open();
                        String indata = port.ReadExisting();
                        RecivedData.Content = indata;

                        port.DtrEnable = true;
                        port.DataReceived += DatareceivedHandler;

                        if (indata == "z") // als er een 10 wordt ontvangen stopt de controleloop (van de microcontroller)
                        {
                            port.DiscardInBuffer();

                            try
                            {
                                System.Windows.MessageBox.Show("Toetsenbord gevonden, druk knop 1 in.");
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show($"Error writing to serial port: {ex.Message}");
                            }
                            geenComPoort = false;
                        }
                        if (indata.Trim() != "z") // indien er niets wordt ontvangen, sluit de poort opnieuw
                        {
                            port.Close();
                        }
                    }
                    catch (Exception ex) // meld probleem bij een poort
                    {
                        //System.Windows.MessageBox.Show($"Error accessing port {poort}: {ex.Message}");
                    }
                }
            }
        }
        private void DatareceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort portdata = (SerialPort)sender;
            String indata = portdata.ReadExisting();
            try
            {
                if (indata == "1" || indata == "2" || indata == "3" || indata == "4" || indata == "5" || indata == "6" || indata == "7" || indata == "8" || indata == "9")
                {
                    SimulateWinRKeyPress();
                    if (data.site[int.Parse(indata)] == true)
                    {
                        System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
                        int textBoxIndex = int.Parse(textBox.Name.Substring(4));
                        string siteName = "site" + indata;
                        UIElement TBtekst = FindName(siteName) as UIElement;
                        SendKeys.SendWait(((System.Windows.Controls.TextBox)TBtekst).Text.ToString());
                    }
                    else
                    {
                        SendKeys.SendWait(data.VarPrograms[int.Parse(indata)]);
                    }
                    simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show($"Error writing to serial port: {ex.Message}");
            }
        }
        private void SimulateWinRKeyPress()
        {
            simulator.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.LWIN, WindowsInput.Native.VirtualKeyCode.VK_R);
        }
        public class SavedData
        {
            public string[] Sites { get; set; } = new string[9];
            public string[] Programs { get; set; } = new string[9];
            public string[] VarPrograms { get; set; } = new string[9];
            public bool[] site { get; set; } = new bool[9]; // site of program
        }
        public class JsonManager
        {
            private const string FilePath = "SavedData.json";

            public static SavedData LoadData()
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonConvert.DeserializeObject<SavedData>(json);
                }
                return new SavedData();
            }

            public static void SaveData(SavedData data)
            {
                string json = JsonConvert.SerializeObject(data);
                File.WriteAllText(FilePath, json);
            }
        }
        private void LaadData()
        {
            string[] items = { "Excel", "Google", "Notepad", "Outlook", "Site", "Word" };
            System.Windows.Controls.ComboBox[] comboBoxes = { CB1, CB2, CB3, CB4, CB5, CB6, CB7, CB8, CB9};

            foreach (System.Windows.Controls.ComboBox cb in comboBoxes)
            {
                foreach (string item in items)
                {
                    cb.Items.Add(item);
                }
            }
            CB1.SelectedItem = data.Programs[1];
            site1.Text = data.Sites[1];

            for (int i = 1; i <= 9; i++)
            {
                string siteName = "site" + i;
                UIElement TBtekst = FindName(siteName) as UIElement;
                string CBName = "CB" + i;
                UIElement CBElement = FindName(CBName) as UIElement;
                ((System.Windows.Controls.ComboBox)CBElement).SelectedItem = data.Programs[i];
                ((System.Windows.Controls.TextBox)TBtekst).Text = data.Sites[i];
            }
        }
        private void CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = sender as System.Windows.Controls.ComboBox;
            int comboBoxIndex = int.Parse(comboBox.Name.Substring(2));
            string selectedItem = comboBox.SelectedItem.ToString();
            data.Programs[comboBoxIndex] = selectedItem;
            string siteName = "site" + comboBoxIndex;
            UIElement siteElement = FindName(siteName) as UIElement;
            for (int i = 1; i <= 9; i++)
            {
                data.site[i] = false;
            }
            siteElement.Visibility = Visibility.Hidden;

            switch (selectedItem)
            {
                case "Site":
                    data.site[comboBoxIndex] = true;
                    siteElement.Visibility = Visibility.Visible;
                    break;
                case "Notepad":
                    data.VarPrograms[comboBoxIndex] = "notepad";
                    break;  
                case "Word":
                    data.VarPrograms[comboBoxIndex] = "winword"; 
                    break;
                case "Excel":
                    data.VarPrograms[comboBoxIndex] = "excel";
                    break;
                case "Outlook":
                    data.VarPrograms[comboBoxIndex] = "outlook";
                    break;
                case "Google":
                    data.VarPrograms[comboBoxIndex] = "https://www.google.com/";
                    break;
            }

            JsonManager.SaveData(data);
        }
        private void site_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            int textBoxIndex = int.Parse(textBox.Name.Substring(4));
            string siteName = "site" + textBoxIndex;
            UIElement TBtekst = FindName(siteName) as UIElement;
            data.Sites[textBoxIndex] = ((System.Windows.Controls.TextBox)TBtekst).Text.ToString();
            JsonManager.SaveData(data);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            const int moveAmount = -25; // Adjust this value as needed

            // Get current cursor position
            GetCursorPos(out POINT currentPos);

            // Calculate new cursor position
            int newX = currentPos.X;
            int newY = currentPos.Y + moveAmount;

            // Set new cursor position
            SetCursorPos(newX, newY);
        }
    }
}