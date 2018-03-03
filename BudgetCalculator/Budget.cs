using System;

namespace BudgetCalculator
{
    public class Budget
    {
        public string YearMonth { get; set; }
        public int Amount { get; set; }

        private int TotalDays()
        {
            var budgetDate = DateTime.ParseExact(this.YearMonth + "01", "yyyyMMdd", null);
            return DateTime.DaysInMonth(budgetDate.Year, budgetDate.Month);
        }

        public int DailyAmount()
        {
            return this.Amount / TotalDays();
        }
    }
}