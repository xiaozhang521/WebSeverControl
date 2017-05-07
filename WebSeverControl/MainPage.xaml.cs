using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.UI.Text;
using System.Text;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace WebSeverControl
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public StreamSocketListener listener = new StreamSocketListener();
        public StreamSocket socket=new StreamSocket();
        public MainPage()
        {
            this.InitializeComponent();
            start_listen();
        }
        async void start_listen()
        {
            try
            {
                await listener.BindServiceNameAsync("11211");
                listener.ConnectionReceived += OnConnection;
            }
            catch
            {
                
            }
        }
        private async void OnConnection(
           StreamSocketListener sender,
           StreamSocketListenerConnectionReceivedEventArgs args)
        {
            socket = args.Socket;
            DataReader reader = new DataReader(args.Socket.InputStream);
            while (true)
            {
                reader.InputStreamOptions = InputStreamOptions.Partial;  //采用异步方式  
                await reader.LoadAsync(1024);  //获取一定大小的数据流  
                string response = reader.ReadString(reader.UnconsumedBufferLength);
                if (response == "") continue;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    TextBlock tmp = new TextBlock();
                    tmp.Text = response;
                    Border border = new Border();
                    Brush brush = new SolidColorBrush(Colors.LightGray);
                    border.Background = brush;
                    border.Child = tmp;
                    border.CornerRadius = new CornerRadius(5);
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    border.Margin = new Thickness(0, 5, 0, 5);
                    border.Padding = new Thickness(2, 2, 2, 2);
                    show_message.Children.Add(border);
                }
                 );
            }
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string content = send_message.Text;
            send_message.Text = "";
            TextBlock tmp = new TextBlock();
            tmp.Text = content;
            Border border = new Border();
            Brush brush = new SolidColorBrush(Colors.LightGray);
            border.Background = brush;
            border.Child = tmp;
            border.CornerRadius = new CornerRadius(5);
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.Margin = new Thickness(0, 5, 0, 5);
            border.Padding = new Thickness(2, 2, 2, 2);
            show_message.Children.Add(border);
            DataWriter writer = new DataWriter(socket.OutputStream);
            byte[] data = Encoding.UTF8.GetBytes(content);  //将字符串转换为字节类型，完全可以不用转换  
            writer.WriteBytes(data);  //写入字节流，当然可以使用WriteString直接写入字符串  
            await writer.StoreAsync();  //异步发送数据  
            writer.DetachStream();  //分离  
            writer.Dispose();  //结束writer
        }
    }
}
