namespace Evaluator.Core;

public class EvaluationResult
{
    public string InfixExpression { get; set; } = string.Empty;
    public string PostfixExpression { get; set; } = string.Empty;
    public double Result { get; set; }
    public List<string> CalculationSteps { get; set; } = new List<string>();
}

public class ExpressionEvaluator
{
    public static double Evaluate(string infix)
    {
        var postfix = InfixToPostfix(infix);
        return Calculate(postfix);
    }

    public static string GetPostfixExpression(string infix)
    {
        return InfixToPostfix(infix);
    }

    public static EvaluationResult EvaluateDetailed(string infix)
    {
        var result = new EvaluationResult
        {
            InfixExpression = infix
        };

        result.PostfixExpression = InfixToPostfix(infix);
        result.Result = CalculateWithSteps(result.PostfixExpression, result.CalculationSteps);

        return result;
    }

    private static string InfixToPostfix(string infix)
    {
        var stack = new Stack<char>();
        var postfix = new System.Text.StringBuilder();

        for (int i = 0; i < infix.Length; i++)
        {
            char current = infix[i];

            if (char.IsWhiteSpace(current)) continue;

            // Improved decimal number detection
            if (char.IsDigit(current) || current == '.')
            {
                // Read complete number (including decimals and multiple digits)
                bool hasDecimal = false;
                while (i < infix.Length &&
                       (char.IsDigit(infix[i]) || infix[i] == '.' ||
                        (infix[i] == '.' && !hasDecimal)))
                {
                    if (infix[i] == '.')
                    {
                        if (hasDecimal) break; // Only one decimal point allowed
                        hasDecimal = true;
                    }
                    postfix.Append(infix[i]);
                    i++;
                }
                i--; // Step back because for loop will increment
                postfix.Append(' '); // Separator between numbers
            }
            else if (IsOperator(current))
            {
                if (current == '(')
                {
                    stack.Push(current);
                }
                else if (current == ')')
                {
                    // Pop all operators until '(' is found
                    while (stack.Count > 0 && stack.Peek() != '(')
                    {
                        postfix.Append(stack.Pop());
                        postfix.Append(' ');
                    }
                    if (stack.Count > 0 && stack.Peek() == '(')
                        stack.Pop(); // Remove the '('
                }
                else
                {
                    // While there are operators with higher or equal priority in stack
                    while (stack.Count > 0 && stack.Peek() != '(' &&
                           Priority(current) <= Priority(stack.Peek()))
                    {
                        postfix.Append(stack.Pop());
                        postfix.Append(' ');
                    }
                    stack.Push(current);
                }
            }
        }

        // Pop all remaining operators
        while (stack.Count > 0)
        {
            postfix.Append(stack.Pop());
            postfix.Append(' ');
        }

        return postfix.ToString().Trim();
    }

    private static bool IsOperator(char item) => item is '^' or '/' or '*' or '%' or '+' or '-' or '(' or ')';

    private static int Priority(char op) => op switch
    {
        '^' => 3,
        '*' or '/' or '%' => 2,
        '+' or '-' => 1,
        _ => 0, // For '(' and others
    };

    private static double Calculate(string postfix)
    {
        var steps = new List<string>();
        return CalculateWithSteps(postfix, steps);
    }

    private static double CalculateWithSteps(string postfix, List<string> steps)
    {
        var stack = new Stack<double>();
        var tokens = postfix.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            // Improved decimal number parsing
            if (IsNumber(token))
            {
                if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double number))
                {
                    stack.Push(number);
                    steps.Add($"Push: {number}");
                }
                else
                {
                    throw new InvalidOperationException($"Invalid number format: {token}");
                }
            }
            else if (token.Length == 1 && IsOperator(token[0]))
            {
                if (stack.Count < 2)
                    throw new InvalidOperationException("Invalid expression: missing operands");

                double op2 = stack.Pop();
                double op1 = stack.Pop();
                double result = CalculateOperation(op1, token[0], op2);
                stack.Push(result);

                steps.Add($"Operation: {op1} {token[0]} {op2} = {result}");
            }
            else
            {
                throw new InvalidOperationException($"Invalid token: {token}");
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Incomplete expression");

        return stack.Pop();
    }

    private static bool IsNumber(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        bool hasDecimal = false;
        foreach (char c in token)
        {
            if (char.IsDigit(c)) continue;
            if (c == '.' && !hasDecimal)
            {
                hasDecimal = true;
                continue;
            }
            return false;
        }
        return true;
    }

    private static double CalculateOperation(double op1, char operation, double op2) => operation switch
    {
        '*' => op1 * op2,
        '/' => op2 == 0 ? throw new DivideByZeroException() : op1 / op2,
        '^' => Math.Pow(op1, op2),
        '+' => op1 + op2,
        '-' => op1 - op2,
        '%' => op1 % op2,
        _ => throw new InvalidOperationException($"Unsupported operator: {operation}")
    };
}