using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Text;
using Windows.Storage.Streams;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App3
{

    public class DeviceInformationListItem
    {
        public DeviceInformation deviceInformation;

        public DeviceInformationListItem(DeviceInformation deviceInformation)
        {
            this.deviceInformation = deviceInformation;

        }

        public override string ToString()
        {
            return $"Name: {deviceInformation.Name} Id:{deviceInformation.Id}";
        }

    }

    public class GattDeviceServiceListItem
    {
        public GattDeviceService service;

        public GattDeviceServiceListItem(GattDeviceService service)
        {
            this.service = service;

        }

        public override string ToString()
        {
            return $"Service UUID: {service.Uuid}";
        }

    }


    public class GattCharacteristicListItem
    {
        public GattCharacteristic characteristic;

        public GattCharacteristicListItem(GattCharacteristic characteristic)
        {
            this.characteristic = characteristic;

        }

        public override string ToString()
        {
            return $"Characteristic UUID: {characteristic.Uuid}";
        }

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DeviceWatcher deviceWatcher;
        DeviceInformation deviceInformation;
        BluetoothLEDevice bluetoothLeDevice;
        GattDeviceService service;
        GattCharacteristic characteristic;

        object selectedListItem;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            //var deviceWatcher= new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };

            deviceWatcher =
                         DeviceInformation.CreateWatcher(
                                 BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                 null,
                                 DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            //deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            //deviceWatcher.Stopped += DeviceWatcher_Stopped;

            //deviceWatcher.Received += DeviceWatcher_Received;

            // Start the watcher.
            deviceWatcher.Start();
        }
        /*
                private async void DeviceWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
                {
                    args.Advertisement.
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Add(new BluetoothLEAdvertisementReceivedEventArgs(args)));
                }
                    throw new NotImplementedException();
                }

                */

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Add(new DeviceInformationListItem(args)));
        }



        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
           selectedListItem = e.ClickedItem;
            Debug.WriteLine(selectedListItem.ToString());
        }



        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            deviceWatcher.Stop();
            // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            deviceInformation = ((DeviceInformationListItem)selectedListItem).deviceInformation;
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInformation.Id);
            Debug.WriteLine($"Device connected: {bluetoothLeDevice.Name}");
        }

        private async void ListServicesButton_Click(object sender, RoutedEventArgs e)
        {

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Clear());
            var services = await bluetoothLeDevice.GetGattServicesAsync();

            foreach (GattDeviceService service in services.Services)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Add(new GattDeviceServiceListItem(service)));
                Debug.WriteLine($"Service UUID: {service.Uuid}");
               
            }
        }

        private async void ListCharactericsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Clear());

            service = ((GattDeviceServiceListItem)selectedListItem).service;
            var characteristics = await service.GetCharacteristicsAsync();

            foreach (GattCharacteristic characteristic in characteristics.Characteristics)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Add(new GattCharacteristicListItem(characteristic)));
                Debug.WriteLine($"Characteristic UUID: {characteristic.Uuid}");

            }



        }

        private async void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listView.Items.Clear());

            characteristic = ((GattCharacteristicListItem)selectedListItem).characteristic;

            await characteristic.WriteValueAsync((Encoding.ASCII.GetBytes("123456789ABCDEFGHIJ")).AsBuffer());

        }

        private async void RegisterNotifyButton_Click(object sender, RoutedEventArgs e)
        {
            characteristic = ((GattCharacteristicListItem)selectedListItem).characteristic;
            characteristic.ValueChanged += Characteristic_ValueChanged;
            await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                     GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        private async void Characteristic_ValueChanged(
    GattCharacteristic sender,
    GattValueChangedEventArgs args)
        {
            //var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            //byte[] bytesRead = new byte[args.CharacteristicValue.Length];
            //dataReader.ReadBytes(bytesRead);

            var data = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);
            //Debug.WriteLine(Encoding.ASCII.GetString(data));

            var dialog = new MessageDialog("Received :"+ Encoding.ASCII.GetString(data));
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await dialog.ShowAsync());

        }
    }
}
