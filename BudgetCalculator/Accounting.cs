﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
    internal class Accounting
    {
        private readonly IRepository<Budget> _repo;

        public Accounting(IRepository<Budget> repo)
        {
            _repo = repo;
        }

        public decimal Calculate(DateTime start, DateTime end)
        {
            var period = new Period(start, end);

            return IsSameMonth(period)
                ? GetOneMonthAmount(period)
                : GetRangeMonthAmount(period);
        }

        private decimal GetRangeMonthAmount(Period period)
        {
            var monthCount = period.TotalMonths();
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                if (index == 0)
                {
                    var firstMonthPeriod = new Period(period.Start, period.Start.LastDate());
                    var firstMonthBudgetAmount = GetOneMonthAmount(firstMonthPeriod);
                    total += firstMonthBudgetAmount;
                }
                else if (index == monthCount)
                {
                    var lastMonthPeriod = new Period(period.End.FirstDate(), period.End);
                    var lastMonthBudgetAmount = GetOneMonthAmount(lastMonthPeriod);
                    total += lastMonthBudgetAmount;
                }
                else
                {
                    var now = period.Start.AddMonths(index);
                    var fullMonthPeriod = new Period(now.FirstDate(), now.LastDate());
                    var fullMonthBudgetAmount = GetOneMonthAmount(fullMonthPeriod);
                    total += fullMonthBudgetAmount;
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
            var budget = this._repo.GetAll().Get(period.Start);
            if (budget == null)
            {
                return 0;
            }

            var validDays = EffectiveDays(period.Start, period.End);
            return budget.DailyAmount() * validDays;
        }

        private int EffectiveDays(DateTime start, DateTime end)
        {
            return (end.AddDays(1) - start).Days;
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