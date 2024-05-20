using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Diagnostics;
using AudioSwitcher.AudioApi.CoreAudio;
namespace macro_testen
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y); // event dat de muis op de nieuwe positie plaats

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint); // event dat de huidige positie van de muis op haalt

        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo); // event dat zorgt dat de muis klikt
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        int newX;
        int newY;

        public string Poort { get; set; }
        SerialPort port;
        bool geenComPoort = true;

        private SavedData data;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSerialPort();
            data = JsonManager.LoadData();
            LaadData();
        }
        private void InitializeSerialPort()
        {
            Stopwatch stopwatch = new Stopwatch(); // maak stopwatch aan
            stopwatch.Start();                     // start stopwatch

            // blijft zoeken achter COM-poort indien geen gevonden, maar na 10 seconden niet verbonden stopt de lus
            while (geenComPoort == true && stopwatch.Elapsed.TotalSeconds < 10)
            {
                // zoek alle mogelijke poorten
                String[] Ports = SerialPort.GetPortNames();

                foreach (string poort in Ports) // overloopt alle gevonden COM-poorts
                {
                    port = new SerialPort(poort, 9600); // opent poort en zoekt achter data
                    try
                    {
                        port.Open();
                        String indata = port.ReadExisting();

                        if (indata == "z") // als er een z wordt ontvangen stopt de controleloop (van de microcontroller)
                        {

                            port.DataReceived += DatareceivedHandler;                   // linkt de data dat toekomt aan een eventhandler
                            MessageBox.Show("Toetsenbord gevonden, druk knop 1 in.");
                            geenComPoort = false;                                       // maakt dat de lus stopt
                           return;
                        }
                        else // indien er geen z wordt ontvangen, sluit de poort opnieuw
                        {
                            port.Close();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            Environment.Exit(0);    // wanneer er geen COM-poort verbonden is na 10sec sluit het programma
        }
        private void DatareceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort portdata = (SerialPort)sender;
            String indata = portdata.ReadExisting();    // leest inkomende data
            try
                {   // wanneer indata een cijfer is betekent dit dat een knop is ingedrukt
                    if (indata == "1" || indata == "2" || indata == "3" || indata == "4" || indata == "5" || indata == "6" || indata == "7" || indata == "8" || indata == "9")
                    {
                        // als de knop een site is, word deze if uitgevoerd
                        if (data.Site[int.Parse(indata)] == true)
                        {
                            string siteName = "site" + indata;
                            Dispatcher.Invoke(() =>             // zorgt ervoor dat de site in de UI thread geopend wordt
                            {
                                UIElement TBtekst = FindName(siteName) as UIElement;                                // zoekt de textbox dat bij de knop hoort en lees welke site erin staat
                                Process.Start(((TextBox)TBtekst).Text.ToString(), null);    // opent de site
                            });
                        }
                        // als de knop links klik is, word deze if uitgevoerd
                        else if (data.Links[int.Parse(indata)] == true)
                        {
                            // druk linker muisknop in
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

                            Thread.Sleep(100);

                            // laat linker muisknop los
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        }
                        // als de knop links klik is, word deze if uitgevoerd
                        else if (data.Rechts[int.Parse(indata)] == true)
                        {
                            // druk rechter muisknop in
                            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);

                            Thread.Sleep(100);

                            // laat rechter muisknop los
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        }
                        
                        else // als de knop een programma is, word deze else uitgevoerd
                        {   
                            // haalt uit de json file welk programma er moet geopend worden en start dit programma
                            Process.Start(data.VarPrograms[int.Parse(indata)], null);
                        }
                    }
                    // als de knop een letter is, word deze if uitgevoerd (dit is de joystick voor de muis)
                    // U = omhoog(up), R = rechts, L = links, O = omlaag
                    else if (indata == "U" || indata == "R" || indata == "L" || indata == "O")
                    {
                        const int moveAmount = 15;             // Hoeveel de muis verplaatsen
                        GetCursorPos(out POINT currentPos);     // huidige positie van de muis

                        switch (indata)
                        {
                            case "U":                               // verplaat muis omhoog
                                newX = currentPos.X;
                                newY = currentPos.Y - moveAmount;
                                break;
                            case "R":                               // verplaat muis naar rechts
                                newX = currentPos.X + moveAmount;
                                newY = currentPos.Y;
                                break;
                            case "L":                               // verplaat muis naar links
                                newX = currentPos.X - moveAmount;
                                newY = currentPos.Y;
                                break;
                            case "O":                               // verplaat muis omlaag
                                newX = currentPos.X;
                                newY = currentPos.Y + moveAmount;
                                break;
                        }
                        SetCursorPos(newX, newY);               // zet de muis op de nieuwe positie
                    }
                    // de letters A tot K zijn een simpeler manier om de waarde van de slider weer te geven, dit dient voor de volume van de computer
                    else if (indata == "A" || indata == "B" || indata == "C" || indata == "D" || indata == "E" || indata == "F" || indata == "G" || indata == "H" || indata == "I" || indata == "J" || indata == "K")
                    {
                        // maak nieuwe instantie van coreaudiocontroller
                        CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                        switch (indata)
                        {   // switch case voor de verschillende letters die overeen komen met een bepaaalde volume
                            case "A":
                                defaultPlaybackDevice.Volume = 0;
                                break;
                            case "B":
                                defaultPlaybackDevice.Volume = 10;
                                break;
                            case "C":
                                defaultPlaybackDevice.Volume = 20;
                                break;
                            case "D":
                                defaultPlaybackDevice.Volume = 30;
                                break;
                            case "E":
                                defaultPlaybackDevice.Volume = 40;
                                break;
                            case "F":
                                defaultPlaybackDevice.Volume = 50;
                                break;
                            case "G":
                                defaultPlaybackDevice.Volume = 60;
                                break;
                            case "H":
                                defaultPlaybackDevice.Volume = 70;
                                break;
                            case "I":
                                defaultPlaybackDevice.Volume = 80;
                                break;
                            case "J":
                                defaultPlaybackDevice.Volume = 90;
                                break;
                            case "K":
                                defaultPlaybackDevice.Volume = 100;
                                break;
                        }
                    }
                }
            catch { }
        }
        public class SavedData
        {   // alle data die opgeslagen moet worden
            public string[] Sites { get; set; } = new string[10];       // site dat gebruiker ingeeft in tekstvak 
            public string[] Programs { get; set; } = new string[10];    // programma dat geopend moet worden
            public string[] VarPrograms { get; set; } = new string[10]; // exe van programma dat geopend moet worden
            public bool[] Site { get; set; } = new bool[10];            // duid aan dat een knop op site staat
            public bool[] Links { get; set; } = new bool[10];           // duid aan dat een knop op links klik staat
            public bool[] Rechts { get; set; } = new bool[10];          // duid aan dat een knop op rechts klik staat
        }
        public class JsonManager
        {
            // pad waar de data opgeslagen wordt
            private const string FilePath = "SavedData.json";

            // leest op begin van programma de data in van de json file
            public static SavedData LoadData()
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonConvert.DeserializeObject<SavedData>(json);
                }
                return new SavedData();
            }

            // slaat de data op in de json file
            public static void SaveData(SavedData data)
            {
                string json = JsonConvert.SerializeObject(data);
                File.WriteAllText(FilePath, json);
            }
        }
        private void LaadData()
        {
            // mogelijkheden die in de combobox kunnen gekozen worden
            string[] items = { "Excel", "Google", "Notepad", "Outlook", "Site", "Word", "Bestanden", "links klik", "rechts klik" };
            // alle comboboxen in een array steken zodat deze makkelijk kunnen overlopen worden
            ComboBox[] comboBoxes = { CB1, CB2, CB3, CB4, CB5, CB6, CB7, CB8, CB9};

            // elke combobox krijgt de mogelijkheden
            foreach (ComboBox cb in comboBoxes)
            {
                foreach (string item in items)
                {
                    cb.Items.Add(item);
                }
            }

            // overloopt alle comboboxen en zet de laatste waarde dat de gebuiker heeft gekozen in de combobox en de site in de textbox
            for (int i = 1; i <= 9; i++)
            {
                string siteName = "site" + i;
                UIElement TBtekst = FindName(siteName) as UIElement;   // selecteert juiste textbox
                string CBName = "CB" + i;
                UIElement CBElement = FindName(CBName) as UIElement;   // selecteert juiste combobox
                ((ComboBox)CBElement).SelectedItem = data.Programs[i]; // zet de laatste waarde dat de gebruiker gekozen heeft
                ((TextBox)TBtekst).Text = data.Sites[i];               // zet de laatste site dat de gebruiker heeft ingegeven
            }
        }
        private void CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;                         // selecteert de combobox waar de gebruiker iets heeft gekozen
            int comboBoxIndex = int.Parse(comboBox.Name.Substring(2));      // haalt het nummer van de combobox
            string selectedItem = comboBox.SelectedItem.ToString();         // haalt het selected item dat de gebruiker heeft gekozen
            data.Programs[comboBoxIndex] = selectedItem;                    // slaat op in de json file welk programma de gebruiker heeft gekozen
            string siteName = "site" + comboBoxIndex;                       // selecteert de textbox dat bij de combobox hoort
            UIElement siteElement = FindName(siteName) as UIElement;

            data.Site[comboBoxIndex] = false;                               // maakt dat programma weet dat site niet gekozen is
            data.Links[comboBoxIndex] = false;                              // maakt dat programma weet dat links klik niet gekozen is
            data.Rechts[comboBoxIndex] = false;                             // maakt dat programma weet dat rechts klik niet gekozen is

            siteElement.Visibility = Visibility.Hidden;                     // maakt dat de textbox niet zichtbaar is wanneer site niet gekozen is

            switch (selectedItem)
            {
                case "Site":
                    data.Site[comboBoxIndex] = true;
                    siteElement.Visibility = Visibility.Visible;            // waanneer site gekozen is, maakt dat de textbox zichtbaar is
                    break;
                case "Notepad":
                    data.VarPrograms[comboBoxIndex] = "notepad";            // slaat op in de json file welk exe van het programma dat de gebruiker heeft gekozen
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
                case "Bestanden":
                    data.VarPrograms[comboBoxIndex] = "Explorer";
                    break;
                case "Google":
                    data.VarPrograms[comboBoxIndex] = "https://www.google.com/";
                    break;
                case "links klik":
                    data.Links[comboBoxIndex] = true;                       // maakt dat programma weet dat de knop links klik gekozen is
                    break;
                case "rechts klik":
                    data.Rechts[comboBoxIndex] = true;                      // maakt dat programma weet dat de knop rechts klik gekozen is
                    break;
            }

            JsonManager.SaveData(data);                                     // slaat de data op in de json file
        }
        private void Site_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int textBoxIndex = int.Parse(textBox.Name.Substring(4));
            string siteName = "site" + textBoxIndex;                        
            UIElement TBtekst = FindName(siteName) as UIElement;            // selecteert de juiste textbox
            data.Sites[textBoxIndex] = ((TextBox)TBtekst).Text.ToString();  // slaat de site op in de json file
            JsonManager.SaveData(data);                                     // slaat de data op in de json file
        }
    }
}