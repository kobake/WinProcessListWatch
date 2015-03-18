using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ProcessListWatch
{
	struct ProcessInfo{
		public string id;
		public string path;

		public ProcessInfo(string id, string path){
			this.id = id;
			this.path = path;
		}
	}

	class Program
	{
		static volatile bool endFlag = false;

		static void Main(string[] args)
		{
			Thread t = new Thread(new ThreadStart(ThreadProc));
			t.Start();

			// Enterを受け付ける。
			// Ctrl+Zでスマートに(?)終了。
			// Ctrl+Cのほうがよく用いられる気はする。
			while (Console.ReadLine() != null)
			{
				Thread.Sleep(10);
			}
			endFlag = true;
			t.Join();
			Console.WriteLine("end.");
		}

		static Dictionary<string, ProcessInfo> GetProcesses()
		{
			Dictionary<string, ProcessInfo> ret = new Dictionary<string, ProcessInfo>();
			using (var mc = new System.Management.ManagementClass("Win32_Process"))
			using (var moc = mc.GetInstances())
			{
				// var dic = new Dictionary<string, string>();
				foreach (var mo in moc)
				{
					if (mo["ProcessId"] != null && mo["ExecutablePath"] != null /*&& !dic.ContainsKey(mo["ProcessId"].ToString())*/)
					{
						// dic.Add(mo["ProcessId"].ToString(), mo["ExecutablePath"].ToString());
						ret.Add(mo["ProcessId"].ToString(), new ProcessInfo(mo["ProcessId"].ToString(), mo["ExecutablePath"].ToString()));
					}
					mo.Dispose();
				}
			}
			return ret;
		}

		static void ThreadProc()
		{
			Dictionary<string, ProcessInfo> oldProcesses = GetProcesses();
			try
			{
				while (true)
				{
					if (endFlag) break;
					Thread.Sleep(300);

					// 現時点のプロセス一覧
					Dictionary<string, ProcessInfo> newProcesses = GetProcesses();
					
					// 終了したプロセスの検出
					foreach(string id in oldProcesses.Keys){
						if (!newProcesses.Keys.Contains(id))
						{
							Console.WriteLine("END: {0} {1}", id, oldProcesses[id].path);
						}
					}
					
					// 開始されたプロセスの検出
					foreach (string id in newProcesses.Keys)
					{
						if (!oldProcesses.Keys.Contains(id))
						{
							Console.WriteLine("START: {0} {1}", id, newProcesses[id].path);
						}
					}

					// 旧プロセス一覧の差し替え
					oldProcesses = newProcesses;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.ToString());
			}
			Console.WriteLine("thread end.");
		}
	}
}
