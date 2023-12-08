using System;

namespace OWOP_cs.OWOP.Instance
{
    internal class Bucket
    {
        private DateTime lastCheck;
        private double allowance;
        public double rate;
        public double time;
        private bool infinite;

        public Bucket(double rate, double time, bool infinite)
        {
            this.lastCheck = DateTime.Now;
            this.allowance = rate;
            this.rate = rate;
            this.time = time;
            this.infinite = infinite;
        }

        private void Update()
        {
            this.allowance += (DateTime.Now - this.lastCheck).TotalSeconds * (this.rate / this.time);
            this.lastCheck = DateTime.Now;

            if (this.allowance > this.rate)
            {
                this.allowance = this.rate;
            }
        }

        public bool CanSpend(double count)
        {
            if (this.infinite)
            {
                return true;
            }

            Update();

            if (this.allowance < count)
            {
                return false;
            }

            this.allowance -= count;
            return true;
        }

        public double GetTimeToRestore()
        {
            if (this.allowance >= this.rate)
            {
                return 0;
            }

            return (this.rate - this.allowance) / (this.rate / this.time);
        }

        public async System.Threading.Tasks.Task WaitUntilRestore()
        {
            double restoreTime = GetTimeToRestore() * 1000;
            await System.Threading.Tasks.Task.Delay((int)restoreTime);
        }
    }
}
