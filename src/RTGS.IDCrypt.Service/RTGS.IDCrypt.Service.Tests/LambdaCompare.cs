using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RTGS.IDCrypt.Service.Tests;


/// <summary>
/// Class to compare Lambda expressions
/// Taken from https://stackoverflow.com/questions/283537/most-efficient-way-to-test-equality-of-lambda-expressions/24528357#24528357
/// </summary>
public static class LambdaCompare
{
	public static bool Eq<TSource, TValue>(
		Expression<Func<TSource, TValue>> x,
		Expression<Func<TSource, TValue>> y) =>
		ExpressionsEqual(x, y, null, null);

	public static bool Eq<TSource1, TSource2, TValue>(
		Expression<Func<TSource1, TSource2, TValue>> x,
		Expression<Func<TSource1, TSource2, TValue>> y) =>
		ExpressionsEqual(x, y, null, null);

	public static Expression<Func<Expression<Func<TSource, TValue>>, bool>> Eq<TSource, TValue>(Expression<Func<TSource, TValue>> y) => x => ExpressionsEqual(x, y, null, null);

	private static bool ExpressionsEqual(Expression x, Expression y, LambdaExpression rootX, LambdaExpression rootY)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}

		if (x == null || y == null)
		{
			return false;
		}

		var valueX = TryCalculateConstant(x);
		var valueY = TryCalculateConstant(y);

		if (valueX.IsDefined && valueY.IsDefined)
		{
			return ValuesEqual(valueX.Value, valueY.Value);
		}

		if (x.NodeType != y.NodeType
			|| x.Type != y.Type)
		{
			return IsAnonymousType(x.Type) && IsAnonymousType(y.Type)
				? throw new NotImplementedException("Comparison of Anonymous Types is not supported")
				: false;
		}

		switch (x)
		{
			case LambdaExpression expression:
				{
					var ly = (LambdaExpression)y;
					var paramsX = expression.Parameters;
					var paramsY = ly.Parameters;
					return CollectionsEqual(paramsX, paramsY, expression, ly) && ExpressionsEqual(expression.Body, ly.Body, expression, ly);
				}
			case MemberExpression expression:
				{
					var mey = (MemberExpression)y;
					return Equals(expression.Member, mey.Member) && ExpressionsEqual(expression.Expression, mey.Expression, rootX, rootY);
				}
			case BinaryExpression expression:
				{
					var by = (BinaryExpression)y;
					return expression.Method == @by.Method && ExpressionsEqual(expression.Left, @by.Left, rootX, rootY) &&
						   ExpressionsEqual(expression.Right, @by.Right, rootX, rootY);
				}
			case UnaryExpression expression:
				{
					var uy = (UnaryExpression)y;
					return expression.Method == uy.Method && ExpressionsEqual(expression.Operand, uy.Operand, rootX, rootY);
				}
			case ParameterExpression expression:
				{
					var py = (ParameterExpression)y;
					return rootX.Parameters.IndexOf(expression) == rootY.Parameters.IndexOf(py);
				}
			case MethodCallExpression expression:
				{
					var cy = (MethodCallExpression)y;
					return expression.Method == cy.Method
						   && ExpressionsEqual(expression.Object, cy.Object, rootX, rootY)
						   && CollectionsEqual(expression.Arguments, cy.Arguments, rootX, rootY);
				}
			case MemberInitExpression expression:
				{
					var miy = (MemberInitExpression)y;
					return ExpressionsEqual(expression.NewExpression, miy.NewExpression, rootX, rootY)
						   && MemberInitsEqual(expression.Bindings, miy.Bindings, rootX, rootY);
				}
			case NewArrayExpression expression:
				{
					var ny = (NewArrayExpression)y;
					return CollectionsEqual(expression.Expressions, ny.Expressions, rootX, rootY);
				}
			case NewExpression expression:
				{
					var ny = (NewExpression)y;
					return
						Equals(expression.Constructor, ny.Constructor)
						&& CollectionsEqual(expression.Arguments, ny.Arguments, rootX, rootY)
						&& ((expression.Members == null && ny.Members == null)
							|| (expression.Members != null && ny.Members != null && CollectionsEqual(expression.Members, ny.Members)));
				}
			case ConditionalExpression expression:
				{
					var cy = (ConditionalExpression)y;
					return
						ExpressionsEqual(expression.Test, cy.Test, rootX, rootY)
						&& ExpressionsEqual(expression.IfFalse, cy.IfFalse, rootX, rootY)
						&& ExpressionsEqual(expression.IfTrue, cy.IfTrue, rootX, rootY);
				}
			default:
				throw new NotImplementedException(x.ToString());
		}
	}

	private static bool IsAnonymousType(Type type)
	{
		var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
		var nameContainsAnonymousType = type.FullName?.Contains("AnonymousType") == true;
		var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

		return isAnonymousType;
	}

	private static bool MemberInitsEqual(ICollection<MemberBinding> bx, ICollection<MemberBinding> by,
		LambdaExpression rootX, LambdaExpression rootY)
	{
		if (bx.Count != by.Count)
		{
			return false;
		}

		if (bx.Concat(by).Any(b => b.BindingType != MemberBindingType.Assignment))
		{
			throw new NotImplementedException("Only MemberBindingType.Assignment is supported");
		}

		return
			bx.Cast<MemberAssignment>().OrderBy(b => b.Member.Name)
				.Select((b, i) => new { Expr = b.Expression, b.Member, Index = i })
				.Join(
					by.Cast<MemberAssignment>().OrderBy(b => b.Member.Name)
						.Select((b, i) => new { Expr = b.Expression, b.Member, Index = i }),
					o => o.Index, o => o.Index,
					(xe, ye) => new { XExpr = xe.Expr, XMember = xe.Member, YExpr = ye.Expr, YMember = ye.Member })
				.All(o => Equals(o.XMember, o.YMember) && ExpressionsEqual(o.XExpr, o.YExpr, rootX, rootY));
	}

	private static bool ValuesEqual(object x, object y) =>
		ReferenceEquals(x, y) || (x is ICollection xCollection && y is ICollection yCollection
			? CollectionsEqual(xCollection, yCollection)
			: Equals(x, y));

	private static ConstantValue TryCalculateConstant(Expression e)
	{
		switch (e)
		{
			case ConstantExpression expression:
				return new ConstantValue(true, expression.Value);
			case MemberExpression expression:
				{
					var parentValue = TryCalculateConstant(expression.Expression);
					if (parentValue.IsDefined)
					{
						var result =
							expression.Member is FieldInfo info
								? info.GetValue(parentValue.Value)
								: ((PropertyInfo)expression.Member).GetValue(parentValue.Value);
						return new ConstantValue(true, result);
					}

					break;
				}
			case NewArrayExpression expression:
				{
					var result = expression.Expressions.Select(TryCalculateConstant);
					IEnumerable<ConstantValue> constantValues = result as ConstantValue[] ?? result.ToArray();
					if (constantValues.All(i => i.IsDefined))
					{
						return new ConstantValue(true, constantValues.Select(i => i.Value).ToArray());
					}

					break;
				}
			case ConditionalExpression expression:
				{
					var evaluatedTest = TryCalculateConstant(expression.Test);
					if (evaluatedTest.IsDefined)
					{
						return TryCalculateConstant(Equals(evaluatedTest.Value, true) ? expression.IfTrue : expression.IfFalse);
					}

					break;
				}
			default:
				return default;
		}

		return default;
	}

	private static bool CollectionsEqual(IEnumerable<Expression> x, IEnumerable<Expression> y, LambdaExpression rootX, LambdaExpression rootY)
	{
		IEnumerable<Expression> expressions = x as Expression[] ?? x.ToArray();
		IEnumerable<Expression> enumerable = y as Expression[] ?? y.ToArray();
		return expressions.Count() == enumerable.Count()
			   && expressions.Select((e, i) => new { Expr = e, Index = i })
				   .Join(enumerable.Select((e, i) => new { Expr = e, Index = i }),
					   o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
				   .All(o => ExpressionsEqual(o.X, o.Y, rootX, rootY));
	}

	private static bool CollectionsEqual(ICollection x, ICollection y) =>
		x.Count == y.Count
		&& x.Cast<object>().Select((e, i) => new { Expr = e, Index = i })
			.Join(y.Cast<object>().Select((e, i) => new { Expr = e, Index = i }),
				o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
			.All(o => Equals(o.X, o.Y));

	private struct ConstantValue
	{
		public ConstantValue(bool isDefined, object value)
			: this()
		{
			IsDefined = isDefined;
			Value = value;
		}

		public bool IsDefined { get; }

		public object Value { get; }
	}
}
