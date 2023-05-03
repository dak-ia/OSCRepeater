using Rug.Osc;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace OSCRepeater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class SettingValue
    {
        [XmlElement("Window_Position")]
        public string settingWindowPositionVar = "";
        [XmlElement("Receiver_PortNumber")]
        public string settingReceiverPortNumberVar = "";
        [XmlElement("Target_IPAddress")]
        public string settingTargetIPAddressVar = "";
        [XmlElement("Target_PortNumber")]
        public string settingTargetPortNumberVar = "";
    }

    public partial class MainWindow : Window
    {
        private bool executeButtonState = false;
        private bool receiverPortNumberError = true;
        private bool targetIPAddressError = true;
        private bool targetPortNumberError = true;
        private bool executeState = false;
        private string windowPositionVar = "";
        private int receiverPortNumberVar;
        private IPAddress targetIPAddressVar;
        private int targetPortNumberVar;
        private IPAddress receivedIPAddressVar;
        private OscReceiver oscReceiver;
        private Task oscTask = null;
        private SettingValue settingValue = new SettingValue();
        private string path = Directory.GetCurrentDirectory();
        private string settingFileName = @"Setting.xml";

        public string inputPortNumber { get; set; }
        public string outputIPAddress { get; set; }
        public string outputPortNumber { get; set; }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            IPAddress[] ipList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress x in ipList)
            {
                if (x.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    receivedIPAddress.Text = x.ToString();
                    receivedIPAddressVar = x;
                }
            }
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler((object sender, EventArgs e) =>
            {
                try
                {
                    IPAddress[] newIPList = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress x in newIPList)
                    {
                        if (x.AddressFamily.Equals(AddressFamily.InterNetwork))
                        {
                            Debug.Print(x.ToString());
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                receivedIPAddress.Text = x.ToString();
                            }));
                            receivedIPAddressVar = x;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            });
            Directory.SetCurrentDirectory(path);
            if (File.Exists(settingFileName))
            {
                try
                {
                    StreamReader streamReader = new StreamReader(settingFileName, new UTF8Encoding(false));
                    XmlSerializer serializer = new XmlSerializer(typeof(SettingValue));
                    settingValue = (SettingValue)serializer.Deserialize(streamReader);
                    streamReader.Close();
                    windowPositionVar = settingValue.settingWindowPositionVar.ToString();
                    Debug.Print(windowPositionVar);
                    if (windowPositionVar != null && windowPositionVar.Length != 0)
                    {
                        try
                        {
                            string[] positions = windowPositionVar.Split(",");
                            double topPosition;
                            double leftPosition;
                            double positionWidth;
                            double positionHeight;
                            double.TryParse(positions[0], out topPosition);
                            double.TryParse(positions[1], out leftPosition);
                            double.TryParse(positions[2], out positionWidth);
                            double.TryParse(positions[3], out positionHeight);
                            if (topPosition < SystemParameters.VirtualScreenHeight && leftPosition < SystemParameters.VirtualScreenWidth)
                            {
                                WindowStartupLocation = WindowStartupLocation.Manual;
                                Top = topPosition;
                                Left = leftPosition;
                                Width = positionWidth;
                                Height = positionHeight;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.ToString());
                            WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        }
                    }
                    Loaded += (s, e) =>
                    {
                        if (settingValue.settingReceiverPortNumberVar != null && settingValue.settingReceiverPortNumberVar.ToString().Length != 0 && settingValue.settingTargetIPAddressVar != null && settingValue.settingTargetIPAddressVar.ToString().Length != 0 && settingValue.settingTargetPortNumberVar != null && settingValue.settingTargetPortNumberVar.ToString().Length != 0)
                        {

                            receiverPortNumber.Text = settingValue.settingReceiverPortNumberVar.ToString();
                            targetIPAddress.Text = settingValue.settingTargetIPAddressVar.ToString();
                            targetPortNumber.Text = settingValue.settingTargetPortNumberVar.ToString();
                        }
                    };
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            executeButton.IsEnabled = false;
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (receiverPortNumber.Text.Length != 0)
            {
                try
                {
                    int receiverPortNum = int.Parse(receiverPortNumber.Text);
                    if (0 <= receiverPortNum && receiverPortNum <= 65535)
                    {
                        receiverPortNumberError = false;
                    }
                    else
                    {
                        receiverPortNumberError = true;
                    }
                }
                catch (Exception ex)
                {
                    receiverPortNumberError = true;
                    Debug.Print(ex.ToString());
                }
            }
            else
            {
                receiverPortNumberError = true;
            }
            if (targetIPAddress.Text.Length != 0)
            {
                if (Regex.IsMatch(targetIPAddress.Text, "^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])$"))
                {
                    targetIPAddressError = false;
                }
                else
                {
                    targetIPAddressError = true;
                }
            }
            else
            {
                targetIPAddressError = true;
            }
            if (targetPortNumber.Text.Length != 0)
            {
                try
                {
                    int targetPortNum = int.Parse(targetPortNumber.Text);
                    if (0 <= targetPortNum && targetPortNum <= 65535)
                    {
                        targetPortNumberError = false;
                    }
                    else
                    {
                        targetPortNumberError = true;
                    }
                }
                catch (Exception ex)
                {
                    targetPortNumberError = true;
                    Debug.Print(ex.ToString());
                }
            }
            else
            {
                targetPortNumberError = true;
            }
            if (!receiverPortNumberError && !targetIPAddressError && !targetPortNumberError)
            {
                int.TryParse(receiverPortNumber.Text, out receiverPortNumberVar);
                IPAddress.TryParse(targetIPAddress.Text, out targetIPAddressVar);
                int.TryParse(targetPortNumber.Text, out targetPortNumberVar);
                executeButton.IsEnabled = true;
                Debug.Print("------Apply------");
                Debug.Print(receiverPortNumberVar.ToString());
                Debug.Print(targetIPAddressVar.ToString());
                Debug.Print(targetPortNumberVar.ToString());
                Debug.Print("-----------------");
            }
            else
            {
                executeButton.IsEnabled = false;
            }
        }

        private void executeButton_Click(object sender, RoutedEventArgs e)
        {
            executeButtonState = !executeButtonState;
            if (executeButtonState)
            {
                executeButton.Content = "Stop";
                applyButton.IsEnabled = false;
                receiverPortNumber.IsReadOnly = true;
                targetIPAddress.IsReadOnly = true;
                targetPortNumber.IsReadOnly = true;
                executeState = true;
                oscReceiver = new OscReceiver(receiverPortNumberVar);
                oscReceiver.Connect();
                oscTask = new Task(oscProcess);
                oscTask.Start();
            }
            else
            {
                executeButton.Content = "Start";
                applyButton.IsEnabled = true;
                receiverPortNumber.IsReadOnly = false;
                targetIPAddress.IsReadOnly = false;
                targetPortNumber.IsReadOnly = false;
                executeState = false;
                oscReceiver.Close();
            }
        }

        private void oscProcess()
        {
            while (true)
            {
                try
                {
                    OscPacket packet = oscReceiver.Receive();
                    using (OscSender oscSender = new OscSender(targetIPAddressVar, 0, targetPortNumberVar))
                    {
                        try { oscSender.Connect(); } catch (Exception) { Debug.Print("ConnectError"); }
                        try { oscSender.Send(packet); } catch (Exception) { Debug.Print("SendError"); }
                        try { oscSender.Close(); } catch (Exception) { Debug.Print("CloseError"); }


                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("oscProcessError");
                    Debug.Print(ex.ToString());
                }
                if (!executeState)
                {
                    Debug.Print("oscProcessEnd");
                    break;
                }
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settingValue.settingWindowPositionVar = Top.ToString() + "," + Left.ToString() + "," + Width.ToString() + "," + Height.ToString();
            if (!receiverPortNumberError && !targetIPAddressError && !targetPortNumberError)
            {
                settingValue.settingReceiverPortNumberVar = receiverPortNumberVar.ToString();
                settingValue.settingTargetIPAddressVar = targetIPAddressVar.ToString();
                settingValue.settingTargetPortNumberVar = targetPortNumberVar.ToString();
            }
            try
            {
                Directory.SetCurrentDirectory(path);
                XmlSerializer serializer = new XmlSerializer(typeof(SettingValue));
                StreamWriter streamWriter = new StreamWriter(settingFileName, false, new UTF8Encoding(false));//Non BOM
                serializer.Serialize(streamWriter, settingValue);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textbox = sender as TextBox;
                if (textbox.Name == "receiverPortNumber")
                {
                    targetIPAddress.Focus();
                    targetIPAddress.Select(targetIPAddress.Text.Length, 0);
                }
                if (textbox.Name == "targetIPAddress")
                {
                    targetPortNumber.Focus();
                    targetPortNumber.Select(targetPortNumber.Text.Length, 0);
                }
                if (textbox.Name == "targetPortNumber")
                {
                    receiverPortNumber.Focus();
                    receiverPortNumber.Select(receiverPortNumber.Text.Length, 0);
                }
            }
        }

        private void TextBoxDoubleClickEvent(object sender, EventArgs e)
        {
            var textbox=sender as TextBox;
            textbox.SelectAll();
        }
    }

    public class PortNumberValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                int num = int.Parse((string)value);
                if (0 <= num && num <= 65535)
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return new ValidationResult(false, null);
            }
        }
    }

    public class IPAddressValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Regex.IsMatch(((string)value), "^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])$"))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, null);
            }
        }
    }
}
