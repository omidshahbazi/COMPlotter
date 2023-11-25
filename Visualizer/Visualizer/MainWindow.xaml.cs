using System;
using System.IO.Ports;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace Visualizer
{
	public partial class MainWindow : Window
	{
		public class Packet
		{
			[JsonPropertyName("I")]
			public uint ID { get; set; }

			//[JsonPropertyName("T")]
			//public float Time { get; set; }

			[JsonPropertyName("C")]
			public int Color { get; set; }

			[JsonPropertyName("V")]
			public double Value { get; set; }
		}

		private SerialPort m_Port = null;
		private Dictionary<uint, double[]> m_DataMap;

		public MainWindow()
		{
			InitializeComponent();

			OpenCloseB.Click += OpenCloseB_Click;

			PortsCB.ItemsSource = SerialPort.GetPortNames();

			BaudRatesCB.SelectedIndex = 0;
			BaudRatesCB.SelectionChanged += BaudRatesCB_SelectionChanged;

			GraphsP.Plot.AxisAutoX(margin: 0);
			GraphsP.Plot.SetAxisLimits(yMin: -1, yMax: 1);
			GraphsP.Plot.BottomAxis.Hide();
			GraphsP.Refresh();

			m_DataMap = new Dictionary<uint, double[]>();

			Update();

			PortsCB.SelectedItem = "COM3";
			BaudRatesCB.SelectedItem = "115200";
			OpenCloseB_Click(null, null);
		}

		private void HandlePackets(Packet[] Packets)
		{
			for (int i = 0; i < Packets.Length; i++)
			{
				Packet packet = Packets[i];

				double[] data = null;
				if (!m_DataMap.TryGetValue(packet.ID, out data))
				{
					data = new double[100];

					m_DataMap[packet.ID] = data;

					GraphsP.Plot.AddSignal(data, 1, Color.FromArgb((255 << 24) | packet.Color));
				}

				Array.Copy(data, 1, data, 0, data.Length -1);
				data[data.Length - 1] = packet.Value;
			}
		}

		private void HandleReceivedData(string Value)
		{
			Packet[] packets = null;

			try
			{
				packets = JsonSerializer.Deserialize<Packet[]>(Value);
			}
			catch
			{
			}

			if (packets == null)
			{
				//Application.Current.Dispatcher.Invoke(() =>
				//{
					ConsoleLB.Items.Add(Value);
				//});

				return;
			}

			HandlePackets(packets);

			//Application.Current.Dispatcher.Invoke(() =>
			//{
				GraphsP.Refresh();
			//});
		}

		private void DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				if (m_Port == null)
					return;

				string value = m_Port.ReadLine();

				if (Application.Current == null)
					return;

				//Task.Run(() =>
				Application.Current.Dispatcher.Invoke(() =>
				{
					HandleReceivedData(value);
				});
			}
			catch
			{
				return;
			}
		}

		private void OpenCloseB_Click(object sender, RoutedEventArgs e)
		{
			if (m_Port == null)
			{
				m_DataMap.Clear();
				GraphsP.Plot.Clear();
				GraphsP.Plot.ResetLayout();
				GraphsP.Refresh();

				if (PortsCB.SelectedItem != null)
				{
					m_Port = new SerialPort(PortsCB.SelectedItem.ToString());
					m_Port.BaudRate = Convert.ToInt32(BaudRatesCB.SelectedItem.ToString());
					m_Port.ReadBufferSize = 1024 * 1024 * 2;
					m_Port.DataReceived += DataReceived;

					try
					{
						m_Port.Open();
					}
					catch (Exception ex)
					{
						m_Port = null;

						MessageBox.Show(this, $"An error occured during port openning, {ex}", "Openning Port");
					}
				}
			}
			else
			{
				SerialPort port = m_Port;
				Task.Run(() =>
				{
					port.DataReceived -= DataReceived;
					port.BaseStream.Close();
					port.Close();
				});

				m_Port = null;
			}

			Update();
		}

		private void BaudRatesCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (m_Port == null)
				return;

			m_Port.BaudRate = Convert.ToInt32(BaudRatesCB.SelectedItem.ToString());
		}

		private void Update()
		{
			if (m_Port != null && m_Port.IsOpen)
			{
				OpenCloseB.Content = "Close";
				PortsCB.IsEnabled = false;
			}
			else
			{
				OpenCloseB.Content = "Open";
				PortsCB.IsEnabled = true;
			}
		}
	}
}
