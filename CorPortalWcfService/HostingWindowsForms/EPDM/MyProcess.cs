using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HostingWindowsForms
{
    public class MyProcess
    {
        private ManualResetEvent _doneEvent;

        public MyProcess(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }

        public void MyProcessThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;

            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("thread {0} started..." + threadIndex + "\r\n")));
            StartProcess();
            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("thread {0} end..." + threadIndex + "\r\n")));

            // Indicates that the process had been completed
            _doneEvent.Set();
        }

        public void StartProcess()
        {
            ClassOfTasks c = new ClassOfTasks();

            c.Taskes();
        }
    }
}