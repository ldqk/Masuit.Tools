#if NET5_0_OR_GREATER
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Threading.Channels;

namespace Masuit.Tools.Models;

/// <summary>
/// 贷款模型
/// </summary>
/// <param name="Loan">贷款本金</param>
/// <param name="Rate">贷款初始利率</param>
/// <param name="Start">开始还款日</param>
public record LoanModel(decimal Loan, decimal Rate, int Period, DateTime Start, LoanType LoanType = LoanType.EquivalentInterest)
{
	public Dictionary<DateTime, decimal?> RateAdjustments { get; set; } = new();

	public List<PrepaymentOption> Prepayments { get; set; } = new();

	private static decimal CumIPMT(decimal rate, decimal loan, int period)
	{
		double interest = 0;
		for (int i = 1; i <= period; i++)
		{
			interest += Financial.IPmt((double)(rate / 12), i, period, (double)loan);
		}

		return interest.ToDecimal(2);
	}

	/// <summary>
	/// 生成还款计划
	/// </summary>
	/// <returns></returns>
	public LoanResult Payment()
	{
		var result = LoanType == LoanType.EquivalentPrincipal ? PrepaymentPrincipal() : PrepaymentInterest();
		for (var i = 1; i < result.Plans.Count; i++)
		{
			result.Plans[i].Period = i + 1;
			if (result.Plans[i].LoanType != result.Plans[i - 1].LoanType)
			{
				result.Plans[i].Repayment = result.Plans[i - 1].Balance - result.Plans[i].Balance - result.Plans[i].Amount;
			}
		}

		return result;
	}

	private LoanResult PrepaymentInterest()
	{
		var list = new List<PaymentPlan>()
		{
			new()
			{
				Date = Start,
				LoanType = LoanType.EquivalentInterest
			}
		};
		var pmt = -Financial.Pmt((double)(Rate / 12), Period, (double)Loan);
		list[0].Rate = Rate;
		list[0].Period = 1;
		list[0].PeriodLeft = Period;
		list[0].Payment = pmt.ToDecimal(2);
		list[0].Interest = Math.Round(Loan * Rate / 12, 2, MidpointRounding.AwayFromZero);
		list[0].Amount = list[0].Payment - list[0].Interest;
		list[0].Balance = Loan - list[0].Amount;
		for (var i = 1; i < Period; i++)
		{
			var current = Start.AddMonths(i);
			var adj = RateAdjustments.FirstOrDefault(x => x.Key <= current && x.Key > current.AddMonths(-1));
			var newRate = adj.Value ?? list[i - 1].Rate;
			var prepayment = Prepayments.FirstOrDefault(x => x.Date <= current && x.Date > current.AddMonths(-1));
			if (prepayment?.ChangeType is LoanType.EquivalentPrincipal)
			{
				list.AddRange(new LoanModel(list[i - 1].Balance - prepayment.Amount, newRate, list[i - 1].PeriodLeft - 1, current, LoanType.EquivalentPrincipal)
				{
					Prepayments = Prepayments,
					RateAdjustments = RateAdjustments
				}.PrepaymentPrincipal().Plans);
				break;
			}

			list.Add(new PaymentPlan()
			{
				Period = i,
				Date = current,
				LoanType = LoanType.EquivalentInterest
			});
			list[i].Rate = newRate;
			list[i].Repayment = prepayment?.Amount ?? 0;
			if (Prepayments.FirstOrDefault(x => x.Date <= current.AddMonths(-1) && x.Date > current.AddMonths(-2))?.ReducePeriod == true)
			{
				var leftPeriod = (int)Math.Round(-Math.Log((double)(1 - (list[i - 1].Balance * list[i].Rate / 12) / list[i - 1].Payment)) / Math.Log((double)(1 + list[i].Rate / 12)));
				list[i].PeriodReduce = Period - list.Count + 1 - leftPeriod;
				list[i].PeriodLeft = leftPeriod;
			}
			else
			{
				list[i].PeriodLeft = list[i - 1].PeriodLeft - 1;
			}
			list[i].Payment = -Financial.Pmt((double)(list[i].Rate / 12), list[i].PeriodLeft, (double)list[i - 1].Balance).ToDecimal(2);
			if ((current - adj.Key).TotalDays > 0 && (current - adj.Key).TotalDays < 30)
			{
				var days = (decimal)(list[i].Date - list[i - 1].Date).TotalDays;
				list[i].Payment = list[i - 1].Payment / days * (decimal)Math.Abs((adj.Key - list[i - 1].Date).TotalDays) + list[i].Payment / days * (decimal)Math.Abs((current - adj.Key).TotalDays);
			}
			list[i].Interest = Math.Round(list[i - 1].Balance * list[i].Rate / 12, 2);
			list[i].Amount = Math.Round(list[i].Payment - list[i].Interest, 2);
			list[i].Balance = Math.Round(list[i - 1].Balance - list[i].Amount - list[i].Repayment, 2);
			if (list[i].Balance <= 0)
			{
				list[i].Payment += list[i].Balance;
				break;
			}
		}

		var totalInterest = -CumIPMT(Rate, Loan, Period);
		return new LoanResult(totalInterest, list);
	}

	private LoanResult PrepaymentPrincipal()
	{
		var list = new List<PaymentPlan>()
		{
			new()
			{
				Date = Start,
				LoanType = LoanType.EquivalentPrincipal,
				PeriodLeft = Period
			}
		};
		list[0].Rate = Rate;
		list[0].Period = 1;
		list[0].Interest = Math.Round(Loan * Rate / 12, 2, MidpointRounding.AwayFromZero);
		list[0].Amount = Math.Round(Loan / Period, 2, MidpointRounding.AwayFromZero);
		list[0].Payment = Math.Round(list[0].Amount + list[0].Interest, 2, MidpointRounding.AwayFromZero);
		list[0].Balance = Math.Round(Loan - list[0].Amount, 2, MidpointRounding.AwayFromZero);
		for (var i = 1; i < Period; i++)
		{
			var current = Start.AddMonths(i);
			var adj = RateAdjustments.FirstOrDefault(x => x.Key <= current && x.Key > current.AddMonths(-1));
			var newRate = adj.Value ?? list[i - 1].Rate;
			var prepayment = Prepayments.FirstOrDefault(x => x.Date <= current && x.Date > current.AddMonths(-1));
			if (prepayment?.ChangeType is LoanType.EquivalentInterest)
			{
				list.AddRange(new LoanModel(list[i - 1].Balance - prepayment.Amount, newRate, list[i - 1].PeriodLeft, current)
				{
					Prepayments = Prepayments,
					RateAdjustments = RateAdjustments
				}.PrepaymentInterest().Plans);
				break;
			}

			list.Add(new PaymentPlan()
			{
				Period = i,
				Date = current,
				LoanType = LoanType.EquivalentPrincipal
			});
			list[i].Rate = newRate;
			list[i].Repayment = prepayment?.Amount ?? 0;
			list[i].Interest = Math.Round(list[i - 1].Balance * list[i].Rate / 12, 2, MidpointRounding.AwayFromZero);
			if ((current - adj.Key).TotalDays > 0 && (current - adj.Key).TotalDays < 30)
			{
				var days = (decimal)(list[i].Date - list[i - 1].Date).TotalDays;
				list[i].Interest = list[i - 1].Interest / days * (decimal)Math.Abs((adj.Key - list[i - 1].Date).TotalDays) + list[i].Interest / days * (decimal)Math.Abs((current - adj.Key).TotalDays);
			}

			if (prepayment?.ReducePeriod == true)
			{
				list[i].PeriodReduce = (int)Math.Round(list[i].Repayment / (Loan / Period));
				list[i].PeriodLeft = list[i - 1].PeriodLeft - list[i].PeriodReduce - 1;
			}
			else
			{
				list[i].PeriodLeft = list[i - 1].PeriodLeft - 1;
			}

			list[i].Amount = Math.Round(list[i - 1].Balance / (Period - i - list.Sum(p => p.PeriodReduce)), 2, MidpointRounding.AwayFromZero);
			list[i].Payment = Math.Round(list[i].Amount + list[i].Interest, 2, MidpointRounding.AwayFromZero);
			list[i].Balance = Math.Round(list[i - 1].Balance - list[i].Amount - list[i].Repayment, 2, MidpointRounding.AwayFromZero);
			if (list[i].Balance <= 0)
			{
				list[i].Payment += list[i].Balance;
				break;
			}
		}

		var totalInterest = Loan * Rate / 12 * (Period + 1) / 2;
		return new LoanResult(totalInterest, list);
	}
}

/// <summary>
/// 贷款方式
/// </summary>
public enum LoanType
{
	/// <summary>
	/// 等额本息
	/// </summary>
	EquivalentPrincipal,

	/// <summary>
	/// 等额本金
	/// </summary>
	EquivalentInterest,
}

/// <summary>
/// 提前还款选项
/// </summary>
/// <param name="Date">提前还款时间</param>
/// <param name="Amount">提前还款金额</param>
/// <param name="ReducePeriod">是否减少期数</param>
/// <param name="ChangeType">新还款方式(若还款方式改变，不支持减少期数)</param>
public record PrepaymentOption(DateTime Date, decimal Amount, bool ReducePeriod = false, LoanType? ChangeType = null);

/// <summary>
/// 贷款结果
/// </summary>
/// <param name="TotalInterest">总利息</param>
/// <param name="Plans">还款计划</param>
public record LoanResult(decimal TotalInterest, List<PaymentPlan> Plans)
{
	/// <summary>
	/// 总提前还款额
	/// </summary>
	public decimal TotalRepayment => Plans.Sum(e => e.Repayment);

	/// <summary>
	/// 实际总利息
	/// </summary>
	public decimal ActualInterest => Plans.Sum(e => e.Interest);

	/// <summary>
	/// 实际还款总额
	/// </summary>
	public decimal ActualPayment => Plans.Sum(e => e.Payment + e.Repayment);

	/// <summary>
	/// 节省利息
	/// </summary>
	public decimal SavedInterest => TotalInterest - ActualInterest;

	public void Deconstruct(out decimal totalInterest, out decimal actualInterest, out decimal totalRepayment, out List<PaymentPlan> paymentPlans)
	{
		totalInterest = TotalInterest;
		actualInterest = ActualInterest;
		totalRepayment = TotalRepayment;
		paymentPlans = Plans;
	}

	public void Deconstruct(out decimal totalInterest, out decimal actualInterest, out decimal savedInterest, out decimal totalRepayment, out List<PaymentPlan> paymentPlans)
	{
		totalInterest = TotalInterest;
		actualInterest = ActualInterest;
		totalRepayment = TotalRepayment;
		paymentPlans = Plans;
		savedInterest = SavedInterest;
	}

	public void Deconstruct(out decimal totalInterest, out decimal actualInterest, out decimal savedInterest, out decimal totalRepayment, out decimal actualPayment, out List<PaymentPlan> paymentPlans)
	{
		totalInterest = TotalInterest;
		actualInterest = ActualInterest;
		totalRepayment = TotalRepayment;
		paymentPlans = Plans;
		savedInterest = SavedInterest;
		actualPayment = ActualPayment;
	}
}

public record PaymentPlan
{
	/// <summary>
	/// 期数
	/// </summary>
	public int Period { get; internal set; } = 12;

	/// <summary>
	/// 还款日
	/// </summary>
	public DateTime Date { get; internal set; } = DateTime.Now;

	/// <summary>
	/// 月供
	/// </summary>
	public decimal Payment { get; internal set; }

	/// <summary>
	/// 年利率
	/// </summary>
	public decimal Rate { get; internal set; }

	/// <summary>
	/// 月还利息
	/// </summary>
	public decimal Interest { get; internal set; }

	/// <summary>
	/// 月还本金
	/// </summary>
	public decimal Amount { get; internal set; }

	/// <summary>
	/// 当期提前还款额
	/// </summary>
	public decimal Repayment { get; internal set; }

	/// <summary>
	/// 每期剩余本金
	/// </summary>
	public decimal Balance { get; internal set; }

	/// <summary>
	/// 贷款类型（默认等额本息）
	/// </summary>
	public LoanType LoanType { get; internal set; }

	internal int PeriodReduce { get; set; }
	internal int PeriodLeft { get; set; }
}
#endif
