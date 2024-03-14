using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

namespace HDH_1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<Process> People { get; set; } = new ObservableCollection<Process>();

		public MainWindow()
		{
			InitializeComponent();
			dataGrid.ItemsSource = People;
		}

		private void btn_add_Click(object sender, RoutedEventArgs e)
		{
			int burstTime, arrivalTime, processID;
			if (int.TryParse(burst_text.Text, out burstTime) &&
				int.TryParse(arrival_text.Text, out arrivalTime) &&
				int.TryParse(process_textbox.Text, out processID))
			{
				People.Add(new Process { BurstTime = burstTime, ArrivalTime = arrivalTime, ProcessID = processID });
				arrival_text.Clear();
				burst_text.Clear();
				process_textbox.Clear();
			}
			else
			{
				MessageBox.Show("Invalid input. Please enter valid numbers.");
			}

		}

		private void btn_Calculate_Click(object sender, RoutedEventArgs e)
		{
			List<Process> processes = GetDataFromDataGrid();
			FCFS(processes);
			DrawGanttChart(processes);
			dataGrid.ItemsSource = null;
			dataGrid.ItemsSource = processes;
		}
		private List<Process> GetDataFromDataGrid()
		{
			List<Process> processes = new List<Process>();

			foreach (var item in dataGrid.Items)
			{
				Process process = item as Process;
				if (process != null)
				{
					processes.Add(process);
				}
			}

			return processes;
		}

		private void FCFS(List<Process> processes)
		{
			int currentTime = 0;
			double totalWaitingTime = 0;

			foreach (var process in processes.OrderBy(p => p.ArrivalTime))
			{
				double waitingTime = Math.Max(0, currentTime - process.ArrivalTime);
				totalWaitingTime += waitingTime;

				process.WaitingTime = waitingTime;

				currentTime = Math.Max(currentTime, process.ArrivalTime) + process.BurstTime;

			}

			double averageWaitingTime = totalWaitingTime / processes.Count;
   
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();
		}

		private void DrawGanttChart(List<Process> processes)
		{
			double widthPerUnit = 20; // Độ rộng của mỗi đơn vị thời gian
			double heightPerProcess = 30; // Độ cao của mỗi dòng quá trình
			double topPosition = 50; // Vị trí top chung cho tất cả các thanh

			double currentTime = 0;
			double totalWidth = 0;

			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);

			foreach (var process in processes.OrderBy(p => p.ArrivalTime))
			{
				double startTime = Math.Max(currentTime, process.ArrivalTime);
				double endTime = startTime + process.BurstTime;

				Rectangle rect = new Rectangle();
				rect.Fill = colors[(int)currentTime % colors.Length]; // Lấy màu từ mảng màu
				rect.Width = process.BurstTime * widthPerUnit;
				rect.Height = heightPerProcess;
				Canvas.SetLeft(rect, totalWidth + (startTime - currentTime) * widthPerUnit);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);


				TextBlock endTextBlock = new TextBlock();
				endTextBlock.Text = (int)endTime + ""; // Thời gian kết thúc của quá trình
				Canvas.SetLeft(endTextBlock, totalWidth + (endTime - currentTime) * widthPerUnit - 10); // Đặt vị trí cho TextBlock ở cuối của thanh, trừ 10 để căn chỉnh
				Canvas.SetTop(endTextBlock, topPosition - 20);
				canvas.Children.Add(endTextBlock);

				currentTime = endTime;
				totalWidth += rect.Width;
			}

			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess; // Chiều cao chỉ cần đủ để hiển thị dòng quá trình
		}

		private void btn_Rest_Click(object sender, RoutedEventArgs e)
		{
			canvas.Children.Clear();
		}
	}
}
