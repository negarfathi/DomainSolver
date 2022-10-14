using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class Solver_Bool
    {
        private bool evaluationResult;
        private List<Tuple<string, bool>> methodsValue = new List<Tuple<string, bool>>();

        private string sourceCodePath;
        private string pathConstraint_Bool;
        public Solver_Bool(string sourceCodePath, string pathConstraint_Bool)
        {
            this.sourceCodePath = sourceCodePath;
            this.pathConstraint_Bool = pathConstraint_Bool;
        }
        public void ProcessMethods()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_Bool).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            var invocationExpressions = root.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();
            foreach (var i in invocationExpressions)
            {
                SyntaxNode j = i;
                bool flag = true;
                do
                {
                    if ((j != i) && (j.Kind() == SyntaxKind.InvocationExpression))
                    {
                        flag = false;
                        break;
                    }
                    j = j.Parent;
                } while (j != null);
                if (flag == true)
                {
                    list.Add(i);
                }
            }
            list.Reverse();
            foreach (var i in list)
            {
                int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == i.ToString());
                string namespaceName = Form1.methodCallsInformation[index].Item2;
                string className = Form1.methodCallsInformation[index].Item3;
                string methodName = Form1.methodCallsInformation[index].Item4;
                List<Tuple<SyntaxNode, Tuple<string, int>>> arguments = Form1.methodCallsInformation[index].Item6;
                DomainCalculator_Method methodDomainCalculator = new DomainCalculator_Method(sourceCodePath, namespaceName, className, methodName, arguments);
                Tuple<bool, bool> methodDomain = methodDomainCalculator.CalculateMethodDomain_Bool();
                string domain = null;
                if ((methodDomain.Item1 == true) && (methodDomain.Item2 == true))
                {
                    domain = i.ToString();
                }
                else if (methodDomain.Item1 == true)
                {
                    methodsValue.Add(Tuple.Create(i.ToString(), true));
                    domain = "true";
                }
                else if (methodDomain.Item2 == true)
                {
                    methodsValue.Add(Tuple.Create(i.ToString(), false));
                    domain = "false";
                }
                var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
            }
            pathConstraint_Bool = root.ToString();
        }
        public Tuple<string, List<List<Tuple<string, bool>>>> GenerateOutputs()
        {
            List<string> list = new List<string>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_Bool).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            var invocationExpressions = root.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();
            foreach (var i in invocationExpressions)
            {
                if (list.Contains(i.ToString()) == false)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j != i) && (j.Kind() == SyntaxKind.InvocationExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i.ToString());
                    }
                }
            }
            var elementAccessExpressions = root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
            {
                if (list.Contains(i.ToString()) == false)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if (j.Kind() == SyntaxKind.InvocationExpression)
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i.ToString());
                    }
                }
            }
            var memberAccessExpressions = root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
            {
                if (list.Contains(i.ToString()) == false)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if (j.Kind() == SyntaxKind.InvocationExpression || (j.Kind() == SyntaxKind.ElementAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i.ToString());
                    }
                }
            }
            var identifierNames = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                if ((i.ToString().ToLower() != "true") && (i.ToString().ToLower() != "false") && (list.Contains(i.ToString()) == false))
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j.Kind() == SyntaxKind.InvocationExpression) || (j.Kind() == SyntaxKind.ElementAccessExpression) || (j.Kind() == SyntaxKind.SimpleMemberAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i.ToString());
                    }
                }
            }
            List<List<Tuple<string, bool>>> outputsList = new List<List<Tuple<string, bool>>>();
            List<Tuple<string, bool>> outputs = new List<Tuple<string, bool>>();
            int columns = list.Count;
            if (columns == 0)
            {
                EvaluatePathConstraint(pathConstraint_Bool, null, null);
                if (evaluationResult == true)
                {
                    foreach (var i in methodsValue)
                    {
                        outputs.Add(i);
                    }
                    outputsList.Add(new List<Tuple<string, bool>>(outputs));
                    return Tuple.Create("1", outputsList);
                }
                return Tuple.Create("2", outputsList);
            }
            int rows = (int)Math.Pow(2, columns);
            bool[,] truthTable = new bool[rows, columns];
            for (int i = columns - 1; i >= 0; i--)
            {
                bool value = false;
                int counter;
                for (int j = 0; j < rows; j = j + (int)Math.Pow(2, (columns - 1) - i))
                {
                    counter = j;
                    if (value == true)
                    {
                        value = false;
                    }
                    else if (value == false)
                    {
                        value = true;
                    }
                    while ((counter < rows) && (counter < j + (int)Math.Pow(2, (columns - 1) - i)))
                    {
                        truthTable[counter, i] = value;
                        counter = counter + 1;
                    }
                }
            }
            bool[] array = new bool[columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    array[j] = truthTable[i, j];
                }
                EvaluatePathConstraint(pathConstraint_Bool, list, array);
                if (evaluationResult == true)
                {
                    foreach (var j in methodsValue)
                    {
                        outputs.Add(j);
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        outputs.Add(Tuple.Create(list[j], array[j]));
                    }
                    outputsList.Add(new List<Tuple<string, bool>>(outputs));
                    outputs.Clear();
                }
            }
            return Tuple.Create("3", outputsList);
        }
        private void EvaluatePathConstraint(string x, List<string> y, bool[] z)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j != i) && (j.Kind() == SyntaxKind.InvocationExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i);
                    }
                }
                else if (i.Kind() == SyntaxKind.ElementAccessExpression)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if (j.Kind() == SyntaxKind.InvocationExpression)
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i);
                    }
                }
                else if (i.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j.Kind() == SyntaxKind.InvocationExpression) || (j.Kind() == SyntaxKind.ElementAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i);
                    }
                }
                else if ((i.Kind() == SyntaxKind.IdentifierName) && (i.ToString().ToLower() != "true") && (i.ToString().ToLower() != "false"))
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j.Kind() == SyntaxKind.InvocationExpression) || (j.Kind() == SyntaxKind.ElementAccessExpression) || (j.Kind() == SyntaxKind.SimpleMemberAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i);
                    }
                }
                else if ((i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression) || (i.Kind() == SyntaxKind.LogicalAndExpression) || (i.Kind() == SyntaxKind.LogicalOrExpression) || (i.Kind() == SyntaxKind.LogicalNotExpression) || (i.Kind() == SyntaxKind.ParenthesizedExpression))
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if (j.Kind() == SyntaxKind.InvocationExpression)
                        {
                            flag = false;
                            break;
                        }
                        j = j.Parent;
                    } while (j != null);
                    if (flag == true)
                    {
                        list.Add(i);
                    }
                }
            }
            list.Reverse();
            if (list.Count == 0)
            {
                evaluationResult = bool.Parse(x);
                return;
            }
            foreach (var i in list)
            {
                string left;
                string right;
                string result = null;
                switch (i.Kind())
                {
                    case SyntaxKind.InvocationExpression:
                    case SyntaxKind.ElementAccessExpression:
                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.IdentifierName:
                        result = (z[y.IndexOf(i.ToString())]).ToString();
                        break;
                    case SyntaxKind.EqualsExpression:
                        left = i.ChildNodes().First().ToString();
                        right = i.ChildNodes().Last().ToString();
                        if (((left.ToLower() == "true") && (right.ToLower() == "true")) || ((left.ToLower() == "false") && (right.ToLower() == "false")))
                        {
                            result = "true";
                        }
                        else
                        {
                            result = "false";
                        }
                        break;
                    case SyntaxKind.NotEqualsExpression:
                        left = i.ChildNodes().First().ToString();
                        right = i.ChildNodes().Last().ToString();
                        if (((left.ToLower() == "true") && (right.ToLower() == "false")) || ((left.ToLower() == "false") && (right.ToLower() == "true")))
                        {
                            result = "true";
                        }
                        else
                        {
                            result = "false";
                        }
                        break;
                    case SyntaxKind.LogicalAndExpression:
                        left = i.ChildNodes().First().ToString();
                        right = i.ChildNodes().Last().ToString();
                        result = (bool.Parse(left) && bool.Parse(right)).ToString();
                        break;
                    case SyntaxKind.LogicalOrExpression:
                        left = i.ChildNodes().First().ToString();
                        right = i.ChildNodes().Last().ToString();
                        result = (bool.Parse(left) || bool.Parse(right)).ToString();
                        break;
                    case SyntaxKind.LogicalNotExpression:
                        result = (!(bool.Parse(i.ChildNodes().First().ToString()))).ToString();
                        break;
                    case SyntaxKind.ParenthesizedExpression:
                        result = i.ChildNodes().First().ToString();
                        break;
                    default:
                        break;
                }
                var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                EvaluatePathConstraint(newRoot.ToString(), y, z);
                break;
            }
        }
    }
}