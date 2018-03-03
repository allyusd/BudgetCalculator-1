using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
    internal class Period
    {
        public Period(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public int EffectiveDays()
        {
            return (End - Start).Days + 1;
        }
    }

    internal class Accounting
    {
        private readonly IRepository<Budget> _repo;

        public Accounting(IRepository<Budget> repo)
        {
            _repo = repo;
        }

        public decimal Calculate(DateTime start, DateTime end)
        {
            if (start > end)
            {
                throw new ArgumentException();
            }

            return IsSameMonth(new Period(start, end))
                ? GetOneMonthAmount(new Period(start, end))
                : GetRangeMonthAmount(start, end, new Period(start, end));
        }

        private decimal GetRangeMonthAmount(DateTime start, DateTime end, Period period)
        {
            var monthCount = end.MonthDifference(start);
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                if (index == 0)
                {
                    total += GetOneMonthAmount(new Period(start, start.LastDate()));
                }
                else if (index == monthCount)
                {
                    total += GetOneMonthAmount(new Period(end.FirstDate(), end));
                }
                else
                {
                    var now = start.AddMonths(index);
                    total += GetOneMonthAmount(new Period(now.FirstDate(), now.LastDate()));
                }
            }
            return total;
        }

        private bool IsSameMonth(Period period)
        {
            return period.Start.Year == period.End.Year && period.Start.Month == period.End.Month;
        }

        private int GetOneMonthAmount(Period period)
        {
            var budgets = this._repo.GetAll();
            var budget = budgets.Get(period.Start);
            if (budget == null)
            {
                return 0;
            }

            var dailyAmount = budget.DailyAmount();

            return dailyAmount * period.EffectiveDays();
        }
    }

    public static class BudgetExtension
    {
        public static Budget Get(this List<Budget> list, DateTime date)
        {
            return list.FirstOrDefault(r => r.YearMonth == date.ToString("yyyyMM"));
        }
    }

    public static class DateTimeExtension
    {
        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static DateTime LastDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime FirstDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

    }
}