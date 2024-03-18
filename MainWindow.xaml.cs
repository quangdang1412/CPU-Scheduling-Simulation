using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
			int burstTime, arrivalTime, processID, priority;
			if (int.TryParse(burst_text.Text, out burstTime) &&
				int.TryParse(arrival_text.Text, out arrivalTime) &&
				int.TryParse(process_textbox.Text, out processID) &&
				int.TryParse(arrival_text.Text, out priority))
			{
				People.Add(new Process { BurstTime = burstTime, ArrivalTime = arrivalTime, ProcessID = processID, Priority = priority });
				arrival_text.Clear();
				burst_text.Clear();
				process_textbox.Clear();
				arrival_text.Clear();
			}
			else
			{
				MessageBox.Show("Invalid input. Please enter valid numbers.");
			}
		}


		private void btn_Calculate_Click(object sender, RoutedEventArgs e)
		{
			List<Process> processes = GetDataFromDataGrid();
			select(processes);
		}
		private void select(List<Process> processes)
		{
			if (comboBox.SelectedIndex == 0)
			{
				FCFS(processes);
				dataGrid.ItemsSource = null;
				dataGrid.ItemsSource = processes;
			}
			else if (comboBox.SelectedIndex == 1)
			{
				SJF(processes);
				dataGrid.ItemsSource = null;
				dataGrid.ItemsSource = processes;
			}
			else if (comboBox.SelectedIndex == 2)
			{
				SJF_Preemptive(processes);
				dataGrid.ItemsSource = null;
				dataGrid.ItemsSource = processes;
			}
			else if (comboBox.SelectedIndex == 3)
			{
				int quantum;
				if (int.TryParse(qTextBox.Text, out quantum))
				{
					RoundRobin(processes, quantum);

					dataGrid.ItemsSource = null;
					dataGrid.ItemsSource = processes;
				}
				else
				{
					MessageBox.Show("Invalid quantum value. Please enter a valid integer.");
				}
			}
			else if (comboBox.SelectedIndex == 4)
			{
				foreach (var process in processes)
				{
					process.ArrivalTime = 0;
					Console.WriteLine($"{process.ProcessID}\t\t| {process.ArrivalTime}");
				}
				Priority(processes);
				dataGrid.ItemsSource = null;
				dataGrid.ItemsSource = processes;
			}
			else if (comboBox.SelectedIndex == 5)
			{
				int quantum;
				if (int.TryParse(qTextBox.Text, out quantum))
				{
					foreach (var process in processes)
					{
						process.ArrivalTime = 0;
					}
					PriorityWithRoundRobinQuantum(processes, quantum);
					dataGrid.ItemsSource = null;
					dataGrid.ItemsSource = processes;
				}
				else
				{
					MessageBox.Show("Invalid quantum value. Please enter a valid integer.");
				}
			}
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
			double widthPerUnit = 20; // Độ rộng của mỗi đơn vị thời gian
			double heightPerProcess = 30; // Độ cao của mỗi dòng quá trình
			double topPosition = 50; // Vị trí top chung cho tất cả các thanh

			double currentTime = 0;
			double totalWidth = 0;
			double totalWaitingTime = 0;
			//clear
			canvas.Children.Clear();

			// Mảng màu để gán màu cho mỗi khoảng thời gian
			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			//set bằng 0 mỗi lúc vẽ đầu dòng
			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);

			foreach (var process in processes.OrderBy(p => p.ArrivalTime))
			{
				// Tính thời gian chờ đợi
				double waitingTime = Math.Max(0, currentTime - process.ArrivalTime);
				totalWaitingTime += waitingTime;

				// Gán thời gian chờ đợi cho quá trình
				process.WaitingTime = waitingTime;

				// Tính toán thời gian bắt đầu và kết thúc của quá trình trên biểu đồ Gantt
				double startTime = Math.Max(currentTime, process.ArrivalTime);
				double endTime = startTime + process.BurstTime;

				// Vẽ thanh trạng thái của quá trình trên biểu đồ Gantt
				Rectangle rect = new Rectangle();
				rect.Fill = colors[(int)currentTime % colors.Length];
				rect.Width = process.BurstTime * widthPerUnit;
				rect.Height = heightPerProcess;
				Canvas.SetLeft(rect, totalWidth + (startTime - currentTime) * widthPerUnit);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);


				// Vẽ thời gian kết thúc của mỗi quá trình ở cuối của thanh
				TextBlock endTextBlock = new TextBlock();
				endTextBlock.Text = (int)endTime + ""; // Thời gian kết thúc của quá trình
				Canvas.SetLeft(endTextBlock, totalWidth + (endTime - currentTime) * widthPerUnit - 10);
				Canvas.SetTop(endTextBlock, topPosition - 20);
				canvas.Children.Add(endTextBlock);

				// Cập nhật thời gian hiện tại và chiều rộng tổng cộng của biểu đồ
				currentTime = endTime;
				totalWidth += rect.Width;
			}

			// Đặt kích thước cho Canvas dựa trên kích thước của biểu đồ Gantt
			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess; // Chiều cao chỉ cần đủ để hiển thị dòng quá trình

			// Tính thời gian trung bình chờ đợi
			double averageWaitingTime = totalWaitingTime / processes.Count;
			// Hiển thị thông tin
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();
		}



		private void SJF(List<Process> processes)
		{
			double widthPerUnit = 20; // Độ rộng của mỗi đơn vị thời gian
			double heightPerProcess = 30; // Độ cao của mỗi dòng quá trình
			double topPosition = 50; // Vị trí top chung cho tất cả các thanh
			double totalWidth = 0;

			// Xóa tất cả các thành phần cũ trên Canvas trước khi vẽ
			canvas.Children.Clear();

			int currentTime = 0;
			double totalWaitingTime = 0;

			// Sắp xếp các quá trình theo thứ tự tăng dần của thời gian đến
			processes = processes.OrderBy(p => p.ArrivalTime).ToList();

			List<Process> finishedProcesses = new List<Process>();
			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			//set bằng 0 mỗi lúc vẽ đầu dòng
			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);

			// Cho đến khi tất cả các quá trình đã hoàn thành
			while (processes.Any())
			{
				// Tìm các quá trình đã đến vào thời điểm hiện tại
				var arrivedProcesses = processes.Where(p => p.ArrivalTime <= currentTime).ToList();

				// Nếu không có quá trình nào đã đến, tăng thời gian hiện tại đến thời gian đến của quá trình tiếp theo
				if (!arrivedProcesses.Any())
				{
					currentTime = processes.Min(p => p.ArrivalTime);
					continue;
				}

				// Sắp xếp các quá trình đã đến theo thứ tự tăng dần của thời gian chạy
				var shortestProcess = arrivedProcesses.OrderBy(p => p.BurstTime).First();

				// Tính thời gian chờ đợi
				double waitingTime = currentTime - shortestProcess.ArrivalTime;
				totalWaitingTime += waitingTime;

				// Gán thời gian chờ đợi cho quá trình
				shortestProcess.WaitingTime = waitingTime;

				// Tính toán thời gian bắt đầu và kết thúc của quá trình trên biểu đồ Gantt
				double startTime = Math.Max(currentTime, shortestProcess.ArrivalTime);
				double endTime = startTime + shortestProcess.BurstTime;
				Console.WriteLine(startTime + " " + endTime);
				// Vẽ thanh trạng thái của quá trình trên biểu đồ Gantt
				Rectangle rect = new Rectangle();
				rect.Fill = colors[(int)currentTime % colors.Length];
				rect.Width = shortestProcess.BurstTime * widthPerUnit;
				rect.Height = heightPerProcess;
				Canvas.SetLeft(rect, totalWidth + (startTime - currentTime) * widthPerUnit);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);

				// Vẽ thời gian kết thúc của mỗi quá trình ở cuối của thanh
				TextBlock endTextBlock = new TextBlock();
				endTextBlock.Text = endTime.ToString(); // Thời gian kết thúc của quá trình
				Canvas.SetLeft(endTextBlock, totalWidth + (endTime - currentTime) * widthPerUnit - 10);
				Canvas.SetTop(endTextBlock, topPosition - 20);
				canvas.Children.Add(endTextBlock);

				// Cập nhật thời gian hiện tại và loại bỏ quá trình đã chạy
				currentTime = (int)endTime;
				processes.Remove(shortestProcess);
				finishedProcesses.Add(shortestProcess);

				// Cập nhật tổng chiều rộng của biểu đồ Gantt
				totalWidth += rect.Width;
			}

			// Tính thời gian chờ đợi trung bình
			double averageWaitingTime = totalWaitingTime / finishedProcesses.Count;
			// Hiển thị thông tin
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();

			// Đặt kích thước cho Canvas dựa trên kích thước của biểu đồ Gantt
			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess; // Chiều cao chỉ cần đủ để hiển thị dòng quá trình
		}
		private void SJF_Preemptive(List<Process> processes)
		{
			double widthPerUnit = 20; // Độ rộng của mỗi đơn vị thời gian
			double heightPerProcess = 30; // Độ cao của mỗi dòng quá trình
			double topPosition = 50; // Vị trí top chung cho tất cả các thanh
			double totalWidth = 0;
			double averageWaitingTime = 0;
			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			// Xóa tất cả các thành phần cũ trên Canvas trước khi vẽ
			canvas.Children.Clear();


			//set bằng 0 mỗi lúc vẽ đầu dòng
			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);

			// Sắp xếp các quá trình theo thứ tự tăng dần của thời gian đến
			processes = processes.OrderBy(p => p.ArrivalTime).ToList();

			foreach (Process process in processes)
			{
				process.RemainingTime = process.BurstTime;
			}
			int currentTime = 0;
			int totalProcesses = processes.Count;
			int completedProcesses = 0;
			double totalWaitingTime = 0;

			while (completedProcesses < totalProcesses)
			{
				Process shortestJob = null;
				int shortestTime = int.MaxValue;

				foreach (Process process in processes)
				{
					if (process.ArrivalTime <= currentTime && process.RemainingTime < shortestTime && process.RemainingTime > 0)
					{
						shortestJob = process;
						shortestTime = process.RemainingTime;
					}
				}

				// Tính toán thời gian chờ đợi cho các tiến trình khác
				foreach (Process process in processes)
				{
					if (process != shortestJob && process.ArrivalTime <= currentTime && process.RemainingTime > 0)
					{
						process.WaitingTime++;
					}
				}

				// Bắt đầu quá trình
				double startTime = currentTime;

				// Giảm thời gian còn lại của quá trình đi 1 đơn vị thời gian
				shortestJob.RemainingTime--;

				// Nếu quá trình đã hoàn thành, cập nhật thông tin và tăng biến đếm
				if (shortestJob.RemainingTime == 0)
				{
					completedProcesses++;
					totalWaitingTime += shortestJob.WaitingTime;
				}

				// Tăng thời gian hiện tại lên 1 đơn vị thời gian
				currentTime++;

				// Kết thúc quá trình
				double endTime = currentTime;

				// Vẽ thanh trạng thái của quá trình trên biểu đồ Gantt
				Rectangle rect = new Rectangle();
				rect.Fill = colors[(int)shortestJob.ProcessID % colors.Length];
				rect.Width = (endTime - startTime) * widthPerUnit;
				rect.Height = heightPerProcess;
				Canvas.SetLeft(rect, totalWidth);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);

				// Vẽ thời gian kết thúc của mỗi quá trình ở cuối của thanh
				TextBlock endTextBlock = new TextBlock();
				endTextBlock.Text = endTime.ToString(); // Thời gian kết thúc của quá trình
				Canvas.SetLeft(endTextBlock, totalWidth + rect.Width);
				Canvas.SetTop(endTextBlock, topPosition - 20);
				canvas.Children.Add(endTextBlock);

				// Cập nhật tổng chiều rộng của biểu đồ Gantt
				totalWidth += rect.Width;
			}

			// Tính thời gian chờ đợi trung bình
			averageWaitingTime = totalWaitingTime / totalProcesses;

			// Hiển thị thông tin
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();

			// Đặt kích thước cho Canvas dựa trên kích thước của biểu đồ Gantt
			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess;
		}

		private void ShowQInputTextBox(bool show)
		{
			if (qLabel != null && qTextBox != null)
			{
				if (show)
				{
					// Hiển thị TextBox và Label
					qLabel.Visibility = Visibility.Visible;
					qTextBox.Visibility = Visibility.Visible;
				}
				else
				{
					// Ẩn TextBox và Label
					qLabel.Visibility = Visibility.Collapsed;
					qTextBox.Visibility = Visibility.Collapsed;
				}
			}
		}
		private void RoundRobin(List<Process> processes, int q)
		{
			double widthPerUnit = 20; // Độ rộng của mỗi đơn vị thời gian
			double heightPerProcess = 30; // Độ cao của mỗi dòng quá trình
			double topPosition = 50; // Vị trí top chung cho tất cả các thanh
			double totalWidth = 0;
			double averageWaitingTime = 0;
			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			// Xóa tất cả các thành phần cũ trên Canvas trước khi vẽ
			canvas.Children.Clear();

			//set bằng 0 mỗi lúc vẽ đầu dòng
			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);


			foreach (Process process in processes)
			{
				process.RemainingTime = process.BurstTime;
			}

			int currentTime = 0;
			int totalProcesses = processes.Count;
			int completedProcesses = 0;
			double totalWaitingTime = 0;

			while (completedProcesses < totalProcesses)
			{
				foreach (Process process in processes)
				{
					if (process.RemainingTime > 0)
					{
						// Bắt đầu quá trình
						double startTime = currentTime;

						// Giảm thời gian còn lại của quá trình đi q đơn vị thời gian hoặc hết thời gian còn lại nếu nhỏ hơn q
						int timeToExecute = Math.Min(q, process.RemainingTime);
						process.RemainingTime -= timeToExecute;

						// Tính thời gian chờ đợi cho các quá trình khác
						foreach (Process p in processes)
						{
							if (p != process && p.RemainingTime > 0)
							{
								p.WaitingTime += timeToExecute;
							}
						}

						// Tăng thời gian hiện tại lên timeToExecute
						currentTime += timeToExecute;

						// Kết thúc quá trình
						double endTime = currentTime;

						// Nếu quá trình đã hoàn thành, cập nhật thông tin và tăng biến đếm
						if (process.RemainingTime == 0)
						{
							completedProcesses++;
							totalWaitingTime += process.WaitingTime;
						}

						// Vẽ thanh trạng thái của quá trình trên biểu đồ Gantt
						Rectangle rect = new Rectangle();
						rect.Fill = colors[(int)process.ProcessID % colors.Length];
						rect.Width = (endTime - startTime) * widthPerUnit;
						rect.Height = heightPerProcess;
						Canvas.SetLeft(rect, totalWidth);
						Canvas.SetTop(rect, topPosition);
						canvas.Children.Add(rect);

						// Vẽ thời gian kết thúc của mỗi quá trình ở cuối của thanh
						TextBlock endTextBlock = new TextBlock();
						endTextBlock.Text = endTime.ToString(); // Thời gian kết thúc của quá trình
						Canvas.SetLeft(endTextBlock, totalWidth + rect.Width);
						Canvas.SetTop(endTextBlock, topPosition - 20);
						canvas.Children.Add(endTextBlock);

						// Cập nhật tổng chiều rộng của biểu đồ Gantt
						totalWidth += rect.Width;
					}
				}
			}

			// Tính thời gian chờ đợi trung bình
			averageWaitingTime = totalWaitingTime / totalProcesses;

			// Hiển thị thông tin
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();

			// Đặt kích thước cho Canvas dựa trên kích thước của biểu đồ Gantt
			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess;
		}
		private void Priority(List<Process> processes)
		{
			// Sắp xếp theo priority
			processes.Sort((p1, p2) => p1.Priority.CompareTo(p2.Priority));

			double currentTime = 0;
			double totalWidth = 0;
			double heightPerProcess = 30;
			double topPosition = 50;
			double totalWaitingTime = 0;

			canvas.Children.Clear();

			Brush[] colors = { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			TextBlock startBlock = new TextBlock();
			startBlock.Text = "0";
			Canvas.SetLeft(startBlock, totalWidth);
			Canvas.SetTop(startBlock, topPosition - 20);
			canvas.Children.Add(startBlock);

			foreach (var process in processes)
			{
				double waitingTime = currentTime;
				totalWaitingTime += waitingTime;
				process.WaitingTime = waitingTime;

				double startTime = currentTime;
				double endTime = startTime + process.BurstTime;

				Rectangle rect = new Rectangle();
				rect.Fill = colors[(int)currentTime % colors.Length];
				rect.Width = process.BurstTime * 20;
				rect.Height = heightPerProcess;
				Canvas.SetLeft(rect, totalWidth + (startTime - currentTime) * 20);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);

				TextBlock endTextBlock = new TextBlock();
				endTextBlock.Text = endTime.ToString();
				Canvas.SetLeft(endTextBlock, totalWidth + (endTime - currentTime) * 20 - 10);
				Canvas.SetTop(endTextBlock, topPosition - 20);
				canvas.Children.Add(endTextBlock);

				currentTime = endTime;
				totalWidth += rect.Width;
			}

			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess;

			double averageWaitingTime = totalWaitingTime / processes.Count;

			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();
		}
		private void PriorityWithRoundRobinQuantum(List<Process> processes, int quantum)
		{
			double totalWidth = 0;
			double heightPerProcess = 30;
			double topPosition = 50;

			// Sắp xếp các tiến trình theo Ưu tiên (và Thời gian đến nếu cần)
			var sortedProcesses = processes.OrderBy(p => p.Priority).ToList();

			double currentTime = 0;
			double startPosition = 0;
			Brush[] colors = new Brush[] { Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.Orange, Brushes.Yellow };

			// Sử dụng một danh sách để đại diện cho readyQueue
			var readyQueue = new List<Process>();

			double totalWaitingTime = 0; // Tổng thời gian chờ đợi của tất cả các quá trình

			canvas.Children.Clear();

			// Xử lý các tiến trình sử dụng Priority với Round Robin
			while (sortedProcesses.Count > 0 || readyQueue.Count > 0)
			{
				// Di chuyển các tiến trình đã đến vào readyQueue
				while (sortedProcesses.Count > 0)
				{
					readyQueue.Add(sortedProcesses[0]);
					sortedProcesses.RemoveAt(0);
				}

				// Sắp xếp hàng đợi sẵn sàng theo Priority
				readyQueue.Sort((x, y) => x.Priority.CompareTo(y.Priority));

				var process = readyQueue[0];
				readyQueue.RemoveAt(0);

				Rectangle rect = new Rectangle
				{
					Width = (process.BurstTime < quantum ? process.BurstTime : quantum) * 10,
					Height = heightPerProcess,
					Fill = colors[process.ProcessID % colors.Length]
				};
				Canvas.SetLeft(rect, startPosition);
				Canvas.SetTop(rect, topPosition);
				canvas.Children.Add(rect);

				TextBlock timeText = new TextBlock
				{
					Text = currentTime.ToString(),
					Foreground = Brushes.Black
				};
				Canvas.SetLeft(timeText, startPosition - 5);
				Canvas.SetTop(timeText, topPosition - 20);
				canvas.Children.Add(timeText);

				// Tính toán thời gian tiến trình sẽ chạy
				int runTime = process.BurstTime < quantum ? process.BurstTime : quantum;
				process.BurstTime -= runTime;
				currentTime += runTime;
				startPosition += rect.Width;

				// Tính waiting time của các quá trình khác trong hàng đợi
				foreach (var p in readyQueue)
				{
					if (p != process)
					{
						p.WaitingTime += runTime;
					}
				}

				// Nếu tiến trình chưa kết thúc, thêm nó trở lại danh sách tiến trình
				if (process.BurstTime > 0)
				{
					sortedProcesses.Add(process);
				}
				else
				{
					// Nếu tiến trình đã hoàn thành, cập nhật thời gian chờ đợi
					totalWaitingTime += process.WaitingTime;
				}

			}

			totalWidth = startPosition;

			//Set text cuối 
			TextBlock finalTimeText = new TextBlock
			{
				Text = currentTime.ToString(),
				Foreground = Brushes.Black
			};
			Canvas.SetLeft(finalTimeText, startPosition);
			Canvas.SetTop(finalTimeText, topPosition - 20); // Đặt phía trên thanh biểu diễn cuối cùng
			canvas.Children.Add(finalTimeText);

			canvas.Width = totalWidth;
			canvas.Height = topPosition + heightPerProcess;

			// Tính toán và hiển thị thời gian chờ đợi trung bình của tất cả các quá trình
			double averageWaitingTime = totalWaitingTime / processes.Count;
			averageWaitingTimeTextBox.Text = averageWaitingTime.ToString();

			// Cập nhật lại nguồn dữ liệu cho DataGrid để hiển thị các thay đổi
			dataGrid.ItemsSource = null;
			dataGrid.ItemsSource = processes.OrderBy(p => p.ProcessID).ToList();
		}

		private void btn_Rest_Click(object sender, RoutedEventArgs e)
		{
			canvas.Children.Clear();
			People.Clear();

		}
		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboBox.SelectedItem != null)
			{
				string selectedMethod = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
				if (selectedMethod == "Round Robin")
				{
					// ẩn cột ArrivalTime
					dataGrid.Columns[1].Visibility = Visibility.Collapsed;
					ShowQInputTextBox(true);
				}
				else if (selectedMethod == "Priority")
				{
					// Hiển thị cột Priority và ẩn cột ArrivalTime
					dataGrid.Columns[1].Visibility = Visibility.Collapsed;
					dataGrid.Columns[3].Visibility = Visibility.Visible;
					label_gen.Content = "Priority";
					ShowQInputTextBox(false);
				}
				else if (selectedMethod == "Priority with q")
				{
					// Hiển thị cột Priority và ẩn cột ArrivalTime
					dataGrid.Columns[1].Visibility = Visibility.Collapsed;
					dataGrid.Columns[3].Visibility = Visibility.Visible;
					label_gen.Content = "Priority";
					ShowQInputTextBox(true);
				}
				else
				{
					// Ẩn cột Priority và hiển thị cột ArrivalTime
					dataGrid.Columns[1].Visibility = Visibility.Visible;
					dataGrid.Columns[3].Visibility = Visibility.Collapsed;
					ShowQInputTextBox(false);
				}
			}
		}
	}
}
