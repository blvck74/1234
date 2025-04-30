using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLb1
{
    public static class DashboardHelper
    {
        public static Action? OnDashboardUpdate;

        public static void UpdateDashboard()
        {
            // Вызов события для обновления дашборда
            OnDashboardUpdate?.Invoke();
        }
    }
}