using InteractiveDataDisplay.WPF;
using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace Visualizer
{
	public partial class MainWindow : Window
	{
		private SerialPort m_Port = null;

		LineGraph lg = new LineGraph();

		public MainWindow()
		{
			InitializeComponent();

			OpenCloseB.Click += OpenCloseB_Click;

			PortsCB.ItemsSource = SerialPort.GetPortNames();

			BaudRatesCB.SelectedIndex = 0;
			BaudRatesCB.SelectionChanged += BaudRatesCB_SelectionChanged;

			Update();

			GraphLines.Children.Add(lg);
			lg.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, (byte)(i * 10), 0));
			lg.Description = string.Format("Data series {0}", i + 1);
			lg.StrokeThickness = 2;
		}

		int i = 1;

		private void TimerTick()
		{
			PointCollection points = lg.Points;

			points.Add(new Point(i, i));

			lg.Points = points;
			++i;
		}

		private void DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				Console.WriteLine(m_Port.ReadLine());
			}
			catch
			{
				return;
			}

			Application.Current.Dispatcher.Invoke(TimerTick);
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
