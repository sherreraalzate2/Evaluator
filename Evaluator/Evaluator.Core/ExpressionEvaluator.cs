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

            
            if (char.IsDigit(current) || current == '.')
            {
                
                while (i < infix.Length && (char.IsDigit(infix[i]) || infix[i] == '.'))
                {
                    postfix.Append(infix[i]);
                    i++;
                }
                i--; 
                postfix.Append(' '); 
            }
            else if (IsOperator(current))
            {
                if (current == '(')
                {
                    stack.Push(current);
                }
                else if (current == ')')
                {
                    
                    while (stack.Count > 0 && stack.Peek() != '(')
                    {
                        postfix.Append(stack.Pop());
                        postfix.Append(' ');
                    }
                    if (stack.Count > 0 && stack.Peek() == '(')
                        stack.Pop(); 
                }
                else
                {
                    
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
        _ => 0, 
    };

    private static double Calculate(string postfix)
    {
        var steps = new List<string>();
        return CalculateWithSteps(postfix, steps);
    }

    private static double CalculateWithSteps(string postfix, List<string> steps)
    {
        var stack = new Stack<double>();
        
        var tokens = SplitPostfixTokens(postfix);

        foreach (var token in tokens)
        {
            
            if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double number))
            {
                stack.Push(number);
                steps.Add($"Push: {number}");
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

    
    private static List<string> SplitPostfixTokens(string postfix)
    {
        var tokens = new List<string>();
        var currentToken = new System.Text.StringBuilder();

        for (int i = 0; i < postfix.Length; i++)
        {
            char current = postfix[i];

            if (char.IsWhiteSpace(current))
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else if (char.IsDigit(current) || current == '.')
            {
                currentToken.Append(current);
            }
            else if (IsOperator(current))
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                tokens.Add(current.ToString());
            }
        }

        
        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
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