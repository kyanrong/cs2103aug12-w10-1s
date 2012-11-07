using System;
using System.Timers;

namespace Type
{
    #region Delegates
    public delegate void DateChangeEventHandler(object sender, EventArgs e);
    #endregion

    //@author A0092104U
    public class DateChangeNotifier
    {
        #region Events
        public event DateChangeEventHandler dateChange;
        #endregion

        #region Fields
        private Timer timer;
        private DateTime lastCheck;
        #endregion

        #region Constructors
        public DateChangeNotifier()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            ResetTimer();
        }

        ~DateChangeNotifier()
        {
            timer.Stop();
        }
        #endregion

        #region Event Methods
        protected virtual void OnDateChange(EventArgs e)
        {
            if (dateChange != null) dateChange(this, e);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetTimer();
            OnDateChange(EventArgs.Empty);
        }
        #endregion

        #region Helper Methods
        private void ResetTimer()
        {
            timer.Stop();
            lastCheck = DateTime.Now;
            timer.Interval = GetIntervalsToTomorrow();
            timer.Start();
        }

        private double GetIntervalsToTomorrow()
        {
            const double DELAY_FACTOR = 1.0;

            var tomorrow = DateTime.Today.AddDays(1.0).AddSeconds(DELAY_FACTOR);
            return ((tomorrow - DateTime.Now).TotalMilliseconds);
        }
        #endregion
    }
}
