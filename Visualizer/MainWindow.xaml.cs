using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace Visualizer
{
	public partial class MainWindow : Window
	{
		private SerialPort m_Port = null;

		public MainWindow()
		{
			InitializeComponent();

			OpenCloseB.Click += OpenCloseB_Click;

			PortsCB.ItemsSource = SerialPort.GetPortNames();

			BaudRatesCB.SelectedIndex = 0;
			BaudRatesCB.SelectionChanged += BaudRatesCB_SelectionChanged;

			UpdateTools();
		}

		private void DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			Console.WriteLine(m_Port.ReadLine());
		}

		private void OpenCloseB_Click(object sender, RoutedEventArgs e)
		{
			if (m_Port == null)
			{
				m_Port = new SerialPort(PortsCB.SelectedItem.ToString());
				m_Port.BaudRate = Convert.ToInt32(BaudRatesCB.SelectedItem.ToString());
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
			else
			{
				m_Port.Close();
				m_Port = null;
			}

			UpdateTools();
		}

		private void BaudRatesCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (m_Port == null)
				return;

			m_Port.BaudRate = Convert.ToInt32(BaudRatesCB.SelectedItem.ToString());
		}

		private void UpdateTools()
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
