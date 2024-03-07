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

namespace macro_testen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool open;          // Maakt dat de poort niet twee keer open kan.
        bool sluit = true;         // Maakt dat de poort niet twee keer gesloten kan.
        bool keuze;         // Maakt dat de niet open kan voordat een com poort gekozen is.
        public string Poort { get; set; }
        private SerialPort serialPort;
        byte open_dicht = 1;
        byte aangesloten = 0;

        private readonly InputSimulator simulator;

        public MainWindow()
        {
            InitializeComponent();
            simulator = new InputSimulator();
        }
        private void keypress_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CheckPorts_Click(object sender, RoutedEventArgs e)
        {
            String[] Ports = SerialPort.GetPortNames();    // checkt welke poorten verbonden zijn.
            COMpoortenlijst.Items.Clear();                 // Verwijderd de compoorten die al in de lijst staan.
            foreach (string poort in Ports)                // Elke verbonden poort word in een string geplaatst.
            {
                COMpoortenlijst.Items.Add(poort);          // Zet de poorten in de list box.
            }
            if (Ports.Length > 0)
            {
                COMpoortenlijst.SelectedIndex = 0;         // Maakt zodat er zeker een poort word gekozen.
                OpenPoort.IsEnabled = true;                // Activeer de openButton als er poorten zijn gevonden
  
            }                                              // Toont aan dat er een poort gekozen is.
            else
            {
                OpenPoort.IsEnabled = false;
                System.Windows.MessageBox.Show("Geen COM-poorten gevonden.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private void OpenPoort_Click(object sender, RoutedEventArgs e)
        {
            if(serialPort != null && serialPort.IsOpen)
            {
                System.Windows.MessageBox.Show("COM-poort is al geopend.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string selectedPort = COMpoortenlijst.SelectedItem as string;

            if(selectedPort != null)
            {
                serialPort = new SerialPort(selectedPort);
                serialPort.DataReceived += DatareceivedHandler;
                try
                {
                    serialPort.Open();
                    System.Windows.MessageBox.Show("COM-poort is geopend.", "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Fout bij het openen an COM-poort: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Selecteer een COM-poort om te openen", "ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void SluitPoort_Click(object sender, RoutedEventArgs e)
        {
            if (sluit == false)
            {
                serialPort.Close();           // Sluit de com poort.
                aangesloten = 0;
                open = false;               // Maakt dat de com poort weer open kan.
                sluit = true;               // Maakt dat de com poort niet opnieuw kan gesloten worden.
            }
            else
            {
                System.Windows.MessageBox.Show("COM poort is al gesloten.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Exclamation); // Maakt dat gebruiker weet dat com poort al gesloten is.
            }
        }
        private void DatareceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort data = (SerialPort)sender;
            String indata = data.ReadExisting();
            switch (Convert.ToInt32(indata))
            {
                case 1:
                    Button1();
                    break;
                case 2:
                    Button2();
                    break;
                case 3:
                    Button3();
                    break;
                case 4:
                    Button4();
                    break;
                case 5:
                    Button5();
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
            }
        }

        private void SimulateWinRKeyPress()
        {
            simulator.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.LWIN, WindowsInput.Native.VirtualKeyCode.VK_R);
        }

        private void Button1()
        {
            SimulateWinRKeyPress();
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("https://www.facebook.com/");
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
        }

        private void Button2()
        {
            SimulateWinRKeyPress();
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("Notepad");
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
        }

        private void Button3()
        {
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("Hello, World!");
        }
                
        private void Button4()
        {
            SimulateWinRKeyPress();
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("winword");
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
        }

        private void Button5()
        {
            SimulateWinRKeyPress();
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("Excel");
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
        }
        
        private void Outlook_Click(object sender, RoutedEventArgs e)
        {
            SimulateWinRKeyPress();
            System.Threading.Thread.Sleep(200);
            SendKeys.SendWait("Outlook");
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.EXECUTE);
        }
    }
}
