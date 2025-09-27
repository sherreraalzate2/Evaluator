using Evaluator.Core;

Console.WriteLine("Hello, Evaluator");

var expressions = new[]
{
    "4*5/(4+6)",
    "4*(5+6-(8/2^3)-7)-1",
    "123.89^(1.6/2.789)"
};

foreach (var infix in expressions)
{
    var result = ExpressionEvaluator.Evaluate(infix);
    Console.WriteLine($"Infix: {infix}");
    Console.WriteLine($"Postfix: {ExpressionEvaluator.GetPostfixExpression(infix)}");
    Console.WriteLine($"Result: {result}\n");
}