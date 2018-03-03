using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BudgetCalculator
{
    [TestClass]
    public class UnitTest1
    {
        private IRepository<Budget> _repository = Substitute.For<IRepository<Budget>>();
        private Accounting target;

        [TestInitialize]
        public void TestInit()
        {
            target = new Accounting(_repository);
        }

        [TestMethod]
        public void no_budget()
        {
            GivenBudget();
            TotalAmountShouldBe(new DateTime(2018, 3, 1), new DateTime(2018, 3, 1), 0 );
        }

        private void TotalAmountShouldBe(DateTime start, DateTime end, int expected)
        {
            target.Calculate(start, end).Should().Be(expected);
        }

        private Accounting BudgetCalculat(List<Budget> budgets)
        {
            _repository.GetAll().Returns(budgets);
            return new Accounting(_repository);
        }

        private void GivenBudget(params Budget[] budgets)
        {
            _repository.GetAll().Returns(budgets.ToList());
        }

        [TestMethod]
        public void datetime_is_not_valid()
        {
            GivenBudget();
            Action actual = () => target.Calculate(new DateTime(2018, 3, 1), new DateTime(2018, 2, 1));
            actual.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void one_month()
        {
            GivenBudget(
                    new Budget() { YearMonth = "201801", Amount = 62 }
                );

            TotalAmountShouldBe(new DateTime(2018, 1, 1),new DateTime(2018, 1, 31),62);
        }

        [TestMethod]
        public void inside_month()
        {
            GivenBudget(new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(new DateTime(2018, 1, 1), new DateTime(2018, 1, 15), 30);
        }

        [TestMethod]
        public void period_over_budget()
        {
            GivenBudget( new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(new DateTime(2018, 2, 1), new DateTime(2018, 2, 15), 0);
        }

        [TestMethod]
        public void two_months()
        {
            GivenBudget(
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 }
            );

            TotalAmountShouldBe(new DateTime(2018, 1, 1), new DateTime(2018, 2, 28), 342);
        }

        [TestMethod]
        public void 當一月預算為62_二月預算為280_三月預算為62_一月一號到三月十號_預算拿到362()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 62 },
            });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.Calculate(start, end);

            actual.Should().Be(362);
        }

        [TestMethod]
        public void 當一月預算為62_二月預算為0_三月預算為62_一月一號到三月十號_預算拿到82()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 0 },
                new Budget() { YearMonth = "201803", Amount = 62 },
            });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.Calculate(start, end);

            actual.Should().Be(82);
        }

        [TestMethod]
        public void 當十二月預算為310一月預算為310_二月預算為280_三月預算為310_十二月一號到三月十號_預算拿到1000()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201712", Amount = 310 },
                new Budget() { YearMonth = "201801", Amount = 310 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 310 },
            });
            var start = new DateTime(2017, 12, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.Calculate(start, end);

            actual.Should().Be(1000);
        }
    }
}