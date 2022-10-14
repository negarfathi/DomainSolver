using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class Solver_Array
    {
        private string result;

        private string pathConstraint_Array;
        public Solver_Array(string pathConstraint_Array)
        {
            this.pathConstraint_Array = pathConstraint_Array;
        }
        public List<List<Tuple<string, object>>> GenerateOutputs(List<List<Tuple<string, object>>> x)
        {
            List<Tuple<string, SyntaxKind, string>> list = new List<Tuple<string, SyntaxKind, string>>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_Array).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if ((i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression))
                {
                    Tuple<string, SyntaxKind, string> tuple = Tuple.Create(i.ChildNodes().First().ToString(), i.Kind(), i.ChildNodes().Last().ToString());
                    if (list.Contains(tuple) == false)
                    {
                        list.Add(tuple);
                    }
                }
            }
            foreach (var i in list)
            {
                int index1 = Form1.variablesInformation.FindIndex(j => j.Item1 == i.Item1);
                int leftLength = Form1.variablesInformation[index1].Item2.Item2;
                int index2 = Form1.variablesInformation.FindIndex(j => j.Item1 == i.Item3);
                int rightLength = Form1.variablesInformation[index2].Item2.Item2;
                if ((i.Item2 == SyntaxKind.EqualsExpression) && (leftLength != rightLength))
                {
                    x.Clear();
                }
                for (int j = 0; j < x.Count; j++)
                {
                    List<Tuple<Tuple<string, string>, object>> leftArray = new List<Tuple<Tuple<string, string>, object>>();
                    List<Tuple<Tuple<string, string>, object>> rightArray = new List<Tuple<Tuple<string, string>, object>>();
                    for (int k = 0; k < x[j].Count; k++)
                    {
                        if (x[j][k].Item1.Split('[').First() == i.Item1)
                        {
                            string arrayIndex = x[j][k].Item1.Split('[').Last().Replace("]", "");
                            arrayIndex = ProcessConstants(arrayIndex);
                            arrayIndex = ProcessVariablesAndMethods(arrayIndex, x[j]);
                            ComputeArrayIndexDomain(arrayIndex);
                            string[] text = result.Replace("(", "").Replace(")", "").Split(',');
                            leftArray.Add(Tuple.Create(Tuple.Create(text[0], text[1]), x[j][k].Item2));
                            x[j].RemoveAt(k);
                            k = k - 1;
                        }
                        else if (x[j][k].Item1.Split('[').First() == i.Item3)
                        {
                            string arrayIndex = x[j][k].Item1.Split('[').Last().Replace("]", "");
                            arrayIndex = ProcessConstants(arrayIndex);
                            arrayIndex = ProcessVariablesAndMethods(arrayIndex, x[j]);
                            ComputeArrayIndexDomain(arrayIndex);
                            string[] text = result.Replace("(", "").Replace(")", "").Split(',');
                            rightArray.Add(Tuple.Create(Tuple.Create(text[0], text[1]), x[j][k].Item2));
                            x[j].RemoveAt(k);
                            k = k - 1;
                        }
                    }
                    int minLength = Math.Min(leftLength, rightLength);
                    for (int k = 0; k < minLength; k++)
                    {
                        x[j].Add(Tuple.Create(i.Item1 + "[" + k + "]", ComputeArrayElementDomain(leftArray, k)));
                        x[j].Add(Tuple.Create(i.Item3 + "[" + k + "]", ComputeArrayElementDomain(rightArray, k)));
                    }
                    if (leftLength > minLength)
                    {
                        for (int k = minLength; k < leftLength; k++)
                        {
                            x[j].Add(Tuple.Create(i.Item1 + "[" + k + "]", ComputeArrayElementDomain(leftArray, k)));
                        }
                    }
                    if (rightLength > minLength)
                    {
                        for (int k = minLength; k < rightLength; k++)
                        {
                            x[j].Add(Tuple.Create(i.Item3 + "[" + k + "]", ComputeArrayElementDomain(rightArray, k)));
                        }
                    }
                    switch (i.Item2)
                    {
                        case SyntaxKind.EqualsExpression:
                            for (int k = 0; k < minLength; k++)
                            {
                                int index3 = x[j].FindIndex(l => l.Item1 == i.Item1 + "[" + k + "]");
                                object left = x[j][index3].Item2;
                                int index4 = x[j].FindIndex(l => l.Item1 == i.Item3 + "[" + k + "]");
                                object right = x[j][index4].Item2;
                                Tuple<bool, object> checkEquality = CheckEquality(left, right);
                                if (checkEquality.Item1 == true)
                                {
                                    x[j][index3] = Tuple.Create(x[j][index3].Item1, checkEquality.Item2);
                                    x[j][index4] = Tuple.Create(x[j][index4].Item1, checkEquality.Item2);
                                }
                                else
                                {
                                    x.RemoveAt(j);
                                    j = j - 1;
                                    break;
                                }
                            }
                            break;
                        case SyntaxKind.NotEqualsExpression:
                            if (leftLength != rightLength)
                            {
                                break;
                            }
                            else
                            {
                                bool areEqual = true; ;
                                for (int k = 0; k < minLength; k++)
                                {
                                    int index3 = x[j].FindIndex(l => l.Item1 == i.Item1 + "[" + k + "]");
                                    object left = x[j][index3].Item2;
                                    int index4 = x[j].FindIndex(l => l.Item1 == i.Item3 + "[" + k + "]");
                                    object right = x[j][index4].Item2;
                                    Tuple<bool, object> checkEquality = CheckEquality(left, right);
                                    if (checkEquality.Item1 == false)
                                    {
                                        areEqual = false;
                                        break;
                                    }
                                }
                                if (areEqual == true)
                                {
                                    x.RemoveAt(j);
                                    j = j - 1;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return x;
        }
        private string ProcessConstants(string x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (((i.Kind() == SyntaxKind.NumericLiteralExpression) && (i.Parent.Kind() != SyntaxKind.UnaryMinusExpression)) || (i.Kind() == SyntaxKind.UnaryMinusExpression))
                {
                    SyntaxNode j = i;
                    bool flag = true;
                    do
                    {
                        if ((j.Kind() == SyntaxKind.InvocationExpression) || (j.Kind() == SyntaxKind.ElementAccessExpression) || (j.Kind() == SyntaxKind.SimpleMemberAccessExpression) || (j.Kind() == SyntaxKind.IdentifierName))
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
            foreach (var i in list)
            {
                string domain = "(" + i + "," + i + ")";
                var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
            }
            return root.ToString();
        }
        private string ProcessVariablesAndMethods(string x, List<Tuple<string, object>> y)
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
                else if (i.Kind() == SyntaxKind.IdentifierName)
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
            }
            list.Reverse();
            foreach (var i in list)
            {
                Tuple<string, string> domain1 = DetermineDomain(i, y);
                string domain2 = "(" + domain1.Item1 + "," + domain1.Item2 + ")";
                var subTree = SyntaxFactory.ParseExpression(domain2).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
            }
            return root.ToString();
        }
        private Tuple<string, string> DetermineDomain(SyntaxNode x, List<Tuple<string, object>> y)
        {
            int index = y.FindIndex(j => j.Item1 == x.ToString());
            Tuple<string, string> variableDomain = y[index].Item2 as Tuple<string, string>;
            return Tuple.Create(variableDomain.Item1, variableDomain.Item2);
        }
        private void ComputeArrayIndexDomain(string x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if ((i.Kind() == SyntaxKind.AddExpression) || (i.Kind() == SyntaxKind.SubtractExpression) || (i.Kind() == SyntaxKind.MultiplyExpression) || (i.Kind() == SyntaxKind.DivideExpression) || (i.Kind() == SyntaxKind.ParenthesizedExpression))
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
            if (list.Count == 0)
            {
                result = x;
                return;
            }
            list.Reverse();
            foreach (var i in list)
            {
                string domain = null;
                if ((i.Kind() == SyntaxKind.AddExpression) || (i.Kind() == SyntaxKind.SubtractExpression) || (i.Kind() == SyntaxKind.MultiplyExpression) || (i.Kind() == SyntaxKind.DivideExpression))
                {
                    string[] left = i.ChildNodes().First().ToString().Split(',');
                    left[0] = left[0].Remove(0, 1);
                    left[1] = left[1].Remove(left[1].Length - 1);
                    string[] right = i.ChildNodes().Last().ToString().Split(',');
                    right[0] = right[0].Remove(0, 1);
                    right[1] = right[1].Remove(right[1].Length - 1);
                    double leftDomain = 0;
                    double rightDomain = 0;
                    switch (i.Kind())
                    {
                        case SyntaxKind.AddExpression:
                            leftDomain = double.Parse(left[0]) + double.Parse(right[0]);
                            rightDomain = double.Parse(left[1]) + double.Parse(right[1]);
                            break;
                        case SyntaxKind.SubtractExpression:
                            leftDomain = double.Parse(left[0]) - double.Parse(right[1]);
                            rightDomain = double.Parse(left[1]) - double.Parse(right[0]);
                            break;
                        case SyntaxKind.MultiplyExpression:
                            leftDomain = Math.Min(Math.Min(double.Parse(left[0]) * double.Parse(right[0]), double.Parse(left[0]) * double.Parse(right[1])), Math.Min(double.Parse(left[1]) * double.Parse(right[0]), double.Parse(left[1]) * double.Parse(right[1])));
                            rightDomain = Math.Max(Math.Max(double.Parse(left[0]) * double.Parse(right[0]), double.Parse(left[0]) * double.Parse(right[1])), Math.Max(double.Parse(left[1]) * double.Parse(right[0]), double.Parse(left[1]) * double.Parse(right[1])));
                            break;
                        case SyntaxKind.DivideExpression:
                            if (double.Parse(right[0]) == 0)
                            {
                                right[0] = "1";
                            }
                            if (double.Parse(right[1]) == 0)
                            {
                                right[1] = "-1";
                            }
                            leftDomain = Math.Min(Math.Min(double.Parse(left[0]) / double.Parse(right[0]), double.Parse(left[0]) / double.Parse(right[1])), Math.Min(double.Parse(left[1]) / double.Parse(right[0]), double.Parse(left[1]) / double.Parse(right[1])));
                            rightDomain = Math.Max(Math.Max(double.Parse(left[0]) / double.Parse(right[0]), double.Parse(left[0]) / double.Parse(right[1])), Math.Max(double.Parse(left[1]) / double.Parse(right[0]), double.Parse(left[1]) / double.Parse(right[1])));
                            break;
                        default:
                            break;
                    }
                    if (leftDomain > rightDomain)
                    {
                        double temporaryVariable = leftDomain;
                        leftDomain = rightDomain;
                        rightDomain = temporaryVariable;
                    }
                    domain = "(" + leftDomain + "," + rightDomain + ")";
                }
                else if (i.Kind() == SyntaxKind.ParenthesizedExpression)
                {
                    domain = i.ChildNodes().First().ToString();
                }
                var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                ComputeArrayIndexDomain(newRoot.ToString());
                break;
            }
        }
        private object ComputeArrayElementDomain(List<Tuple<Tuple<string, string>, object>> x, int y)
        {
            foreach (var i in x)
            {
                if ((y >= double.Parse(i.Item1.Item1)) && (y <= double.Parse(i.Item1.Item2)))
                {
                    return i.Item2;
                }
            }
            return "ANY";
        }
        private Tuple<bool, object> CheckEquality(object x, object y)
        {
            string type1 = x.GetType().FullName;
            string type2 = y.GetType().FullName;
            if (type1 == type2)
            {
                switch (type1)
                {
                    case "System.Boolean":
                    case "System.Char":
                    case "System.String":
                        if (x == y)
                        {
                            return Tuple.Create(true, x);
                        }
                        else
                        {
                            return Tuple.Create(false, (object)null);
                        }
                    default:
                        Tuple<string, string> intersection = Intersection(x as Tuple<string, string>, y as Tuple<string, string>);
                        if ((intersection.Item1 == "") && (intersection.Item2 == ""))
                        {
                            return Tuple.Create(false, (object)null);
                        }
                        else
                        {
                            return Tuple.Create(true, (object)intersection);
                        }
                }
            }
            else
            {
                if (x.ToString() == "ANY")
                {
                    return Tuple.Create(true, y);
                }
                else if (y.ToString() == "ANY")
                {
                    return Tuple.Create(true, x);
                }
                else
                {
                    return Tuple.Create(false, (object)null);
                }
            }
        }
        private Tuple<string, string> Intersection(Tuple<string, string> x, Tuple<string, string> y)
        {
            Tuple<string, string> domain = null;
            if ((double.Parse(x.Item1) == double.Parse(x.Item2)) && (double.Parse(y.Item1) == double.Parse(y.Item2)))
            {
                if (double.Parse(x.Item1) == double.Parse(y.Item1))
                {
                    domain = Tuple.Create(x.Item1, x.Item1);
                }
                else
                {
                    domain = Tuple.Create("", "");
                }
            }
            else if (double.Parse(x.Item1) == double.Parse(x.Item2))
            {
                if ((double.Parse(x.Item1) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create("", "");
                }
                else
                {
                    domain = Tuple.Create(y.Item1, y.Item2);
                }
            }
            else if (double.Parse(y.Item1) == double.Parse(y.Item2))
            {
                if (((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > double.Parse(y.Item1)))
                {
                    domain = Tuple.Create("", "");
                }
                else
                {
                    domain = Tuple.Create(y.Item1, y.Item2);
                }
            }
            else
            {
                if ((double.Parse(x.Item1) <= double.Parse(y.Item1)) && ((double.Parse(x.Item2) - 0.001) >= (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create(y.Item1, y.Item2);
                }
                else if ((double.Parse(x.Item1) >= double.Parse(y.Item1)) && ((double.Parse(x.Item2) - 0.001) <= (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create(x.Item1, x.Item2);
                }
                else if (((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create("", "");
                }
                else if ((double.Parse(x.Item2) - 0.001) == double.Parse(y.Item1))
                {
                    domain = Tuple.Create(x.Item2, x.Item2);
                }
                else if (double.Parse(x.Item1) == (double.Parse(y.Item2) - 0.001))
                {
                    domain = Tuple.Create(x.Item1, x.Item1);
                }
                else if ((double.Parse(x.Item2) - 0.001) > double.Parse(y.Item1) && ((double.Parse(x.Item2) - 0.001) < (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create(y.Item1, x.Item2);
                }
                else if ((double.Parse(x.Item1) > double.Parse(y.Item1)) && (double.Parse(x.Item1) < (double.Parse(y.Item2) - 0.001)))
                {
                    domain = Tuple.Create(x.Item1, y.Item2);
                }
            }
            return domain;
        }
    }
}