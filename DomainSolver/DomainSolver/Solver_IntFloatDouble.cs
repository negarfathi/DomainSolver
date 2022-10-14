using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class Solver_IntFloatDouble
    {
        private string result;
        private int number = 0;
        private int divisionParameter = 3;
        private bool isSatisfiable = true;
        private string editedPathConstraint;
        public List<Tuple<string, Tuple<string, string>>> update = new List<Tuple<string, Tuple<string, string>>>();

        private string sourceCodePath;
        private string pathConstraint_IntFloatDouble;
        public Solver_IntFloatDouble(string sourceCodePath, string pathConstraint_IntFloatDouble)
        {
            this.sourceCodePath = sourceCodePath;
            this.pathConstraint_IntFloatDouble = pathConstraint_IntFloatDouble;
            editedPathConstraint = pathConstraint_IntFloatDouble;
        }
        public void ProcessConstants()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(editedPathConstraint).SyntaxTree;
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
            editedPathConstraint = root.ToString();
        }
        public void ProcessVariables()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(editedPathConstraint).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.ElementAccessExpression)
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
                Tuple<string, string> variableDomain = DetermineVariableDomain(i);
                string domain = "(" + variableDomain.Item1 + "," + variableDomain.Item2 + ")";
                var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
            }
            editedPathConstraint = root.ToString();
        }
        private Tuple<string, string> DetermineVariableDomain(SyntaxNode x)
        {
            Tuple<string, string> variableDomain = null;
            int index = Form1.variablesInformation.FindIndex(j => j.Item1 == x.ToString());
            string dataType = Form1.variablesInformation[index].Item2.Item1;
            switch (dataType)
            {
                case "int":
                case "System.Int32":
                    variableDomain = Tuple.Create("-8", "8");
                    break;
                case "float":
                case "System.Single":
                    variableDomain = Tuple.Create("-8", "8");
                    break;
                case "double":
                case "System.Double":
                    variableDomain = Tuple.Create("-8", "8");
                    break;
                default:
                    break;
            }
            return variableDomain;
        }
        public void ProcessMethods()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(editedPathConstraint).SyntaxTree;
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
                Tuple<string, string> methodDomain = methodDomainCalculator.CalculateMethodDomain_IntFloatDouble();
                string doamin = "(" + methodDomain.Item1 + "," + methodDomain.Item2 + ")";
                var subTree = SyntaxFactory.ParseExpression(doamin).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
            }
            editedPathConstraint = root.ToString();
        }
        public object GenerateOutputs()
        {
            List<List<Tuple<string, Tuple<string, string>>>> outputs = new List<List<Tuple<string, Tuple<string, string>>>>();
            ComputeOutputs(editedPathConstraint);
            if (isSatisfiable == false)
            {
                return "Unsatisfiable";
            }
            string[] results = result.Split(' ');
            results = results.Distinct().ToArray();
            for (int i = 0; i < results.Count(); i++)
            {
                List<Tuple<string, Tuple<string, string>>> list = new List<Tuple<string, Tuple<string, string>>>();
                string[] newResults = results[i].Split(new string[] { ")(" }, StringSplitOptions.None);
                newResults[0] = newResults[0].Remove(0, 1);
                newResults[newResults.Count() - 1] = newResults[newResults.Count() - 1].Remove(newResults[newResults.Count() - 1].Length - 1);
                for (int j = 0; j < newResults.Count(); j++)
                {
                    string[] text = newResults[j].Split(',');
                    list.Add(Tuple.Create(newResults[j].Replace("," + text[text.Count() - 2] + "," + text[text.Count() - 1], ""), Tuple.Create(text[text.Count() - 2], text[text.Count() - 1])));
                }
                outputs.Add(list);
            }
            return outputs;
        }
        private void ComputeOutputs(string x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if ((i.Kind() == SyntaxKind.AddExpression) || (i.Kind() == SyntaxKind.SubtractExpression) || (i.Kind() == SyntaxKind.MultiplyExpression) || (i.Kind() == SyntaxKind.DivideExpression) || (i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression) || (i.Kind() == SyntaxKind.LessThanExpression) || (i.Kind() == SyntaxKind.LessThanOrEqualExpression) || (i.Kind() == SyntaxKind.GreaterThanExpression) || (i.Kind() == SyntaxKind.GreaterThanOrEqualExpression) || (i.Kind() == SyntaxKind.LogicalAndExpression) || (i.Kind() == SyntaxKind.LogicalOrExpression) || (i.Kind() == SyntaxKind.ParenthesizedExpression))
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
                else if ((i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression) || (i.Kind() == SyntaxKind.LessThanExpression) || (i.Kind() == SyntaxKind.LessThanOrEqualExpression) || (i.Kind() == SyntaxKind.GreaterThanExpression) || (i.Kind() == SyntaxKind.GreaterThanOrEqualExpression))
                {
                    string[] left = i.ChildNodes().First().ToString().Split(',');
                    left[0] = left[0].Remove(0, 1);
                    left[1] = left[1].Remove(left[1].Length - 1);
                    string[] right = i.ChildNodes().Last().ToString().Split(',');
                    right[0] = right[0].Remove(0, 1);
                    right[1] = right[1].Remove(right[1].Length - 1);
                    List<List<Tuple<string, string>>> ip_prt = IP_PRT(left, right, i.Kind());
                    if (ip_prt.Count == 0)
                    {
                        isSatisfiable = false;
                        return;
                    }
                    Tuple<SyntaxNode, SyntaxNode> comparisonExpression = GetComparisonExpression(number);
                    List<List<Tuple<string, Tuple<string, string>>>> myList = new List<List<Tuple<string, Tuple<string, string>>>>();
                    for (int j = 0; j < ip_prt.Count; j++)
                    {
                        Update(comparisonExpression.Item1, ip_prt[j][0]);
                        Update(comparisonExpression.Item2, ip_prt[j][1]);
                        myList.Add(new List<Tuple<string, Tuple<string, string>>>(update));
                        update.Clear();
                    }
                    try
                    {
                        for (int j = 0; j < myList.Count; j++)
                        {
                            for (int k = 0; k < myList[j].Count; k++)
                            {
                                for (int l = k + 1; l < myList[j].Count; l++)
                                {
                                    if (myList[j][k].Item1 == myList[j][l].Item1)
                                    {
                                        Tuple<string, string> intersection = Intersection(myList[j][k].Item2, myList[j][l].Item2);
                                        if ((intersection.Item1 == "") && (intersection.Item2 == ""))
                                        {
                                            myList.RemoveAt(j);
                                            if (j == myList.Count)
                                            {
                                                j = j - 1;
                                            }
                                            k = 0;
                                            l = k;
                                        }
                                        else
                                        {
                                            string text = myList[j][k].Item1;
                                            myList[j].RemoveAt(l);
                                            myList[j].RemoveAt(k);
                                            myList[j].Add(Tuple.Create(text, intersection));
                                            l = k;
                                        }
                                    }
                                }
                            }
                        }
                        for (int j = 0; j < myList.Count; j++)
                        {
                            for (int k = 0; k < myList[j].Count; k++)
                            {
                                domain = domain + "(" + myList[j][k].Item1 + "," + myList[j][k].Item2.Item1 + "," + myList[j][k].Item2.Item2 + ")";
                            }
                            domain = domain + " ";
                        }
                        domain = domain.Remove(domain.Length - 1);
                        number = number + 1;
                    }
                    catch (Exception)
                    {
                        isSatisfiable = false;
                        return;
                    }
                }
                else if (i.Kind() == SyntaxKind.LogicalAndExpression)
                {
                    string[] left = i.ChildNodes().First().ToString().Split(' ');
                    string[] right = i.ChildNodes().Last().ToString().Split(' ');
                    List<List<Tuple<string, Tuple<string, string>>>> leftList = new List<List<Tuple<string, Tuple<string, string>>>>();
                    List<List<Tuple<string, Tuple<string, string>>>> rightList = new List<List<Tuple<string, Tuple<string, string>>>>();
                    List<Tuple<string, Tuple<string, string>>> auxiliaryList = new List<Tuple<string, Tuple<string, string>>>();
                    for (int j = 0; j < left.Count(); j++)
                    {
                        string[] newLeft = left[j].Split(new string[] { ")(" }, StringSplitOptions.None);
                        newLeft[0] = newLeft[0].Remove(0, 1);
                        newLeft[newLeft.Count() - 1] = newLeft[newLeft.Count() - 1].Remove(newLeft[newLeft.Count() - 1].Length - 1);
                        for (int k = 0; k < newLeft.Count(); k++)
                        {
                            string[] text = newLeft[k].Split(',');
                            auxiliaryList.Add(Tuple.Create(newLeft[k].Replace("," + text[text.Count() - 2] + "," + text[text.Count() - 1], ""), Tuple.Create(text[text.Count() - 2], text[text.Count() - 1])));
                        }
                        leftList.Add(new List<Tuple<string, Tuple<string, string>>>(auxiliaryList));
                        auxiliaryList.Clear();
                    }
                    for (int j = 0; j < right.Count(); j++)
                    {
                        string[] newRight = right[j].Split(new string[] { ")(" }, StringSplitOptions.None);
                        newRight[0] = newRight[0].Remove(0, 1);
                        newRight[newRight.Count() - 1] = newRight[newRight.Count() - 1].Remove(newRight[newRight.Count() - 1].Length - 1);
                        for (int k = 0; k < newRight.Count(); k++)
                        {
                            string[] text = newRight[k].Split(',');
                            auxiliaryList.Add(Tuple.Create(newRight[k].Replace("," + text[text.Count() - 2] + "," + text[text.Count() - 1], ""), Tuple.Create(text[text.Count() - 2], text[text.Count() - 1])));
                        }
                        rightList.Add(new List<Tuple<string, Tuple<string, string>>>(auxiliaryList));
                        auxiliaryList.Clear();
                    }
                    List<List<Tuple<string, Tuple<string, string>>>> myList = new List<List<Tuple<string, Tuple<string, string>>>>();
                    for (int j = 0; j < leftList.Count; j++)
                    {
                        for (int k = 0; k < rightList.Count; k++)
                        {
                            for (int m = 0; m < leftList[j].Count; m++)
                            {
                                auxiliaryList.Add(leftList[j][m]);
                            }
                            for (int n = 0; n < rightList[k].Count; n++)
                            {
                                auxiliaryList.Add(rightList[k][n]);
                            }
                            myList.Add(new List<Tuple<string, Tuple<string, string>>>(auxiliaryList));
                            auxiliaryList.Clear();
                        }
                    }
                    try
                    {
                        for (int j = 0; j < myList.Count; j++)
                        {
                            for (int k = 0; k < myList[j].Count; k++)
                            {
                                for (int l = k + 1; l < myList[j].Count; l++)
                                {
                                    if (myList[j][k].Item1 == myList[j][l].Item1)
                                    {
                                        Tuple<string, string> intersection = Intersection(myList[j][k].Item2, myList[j][l].Item2);
                                        if ((intersection.Item1 == "") && (intersection.Item2 == ""))
                                        {
                                            myList.RemoveAt(j);
                                            if (j == myList.Count)
                                            {
                                                j = j - 1;
                                            }
                                            k = 0;
                                            l = k;
                                        }
                                        else
                                        {
                                            string text = myList[j][k].Item1;
                                            myList[j].RemoveAt(l);
                                            myList[j].RemoveAt(k);
                                            myList[j].Add(Tuple.Create(text, intersection));
                                            l = k;
                                        }

                                    }
                                }
                            }
                        }
                        for (int j = 0; j < myList.Count; j++)
                        {
                            for (int k = 0; k < myList[j].Count; k++)
                            {
                                domain = domain + "(" + myList[j][k].Item1 + "," + myList[j][k].Item2.Item1 + "," + myList[j][k].Item2.Item2 + ")";
                            }
                            domain = domain + " ";
                        }
                        domain = domain.Remove(domain.Length - 1);
                    }
                    catch (Exception)
                    {
                        isSatisfiable = false;
                        return;
                    }
                }
                else if (i.Kind() == SyntaxKind.ParenthesizedExpression)
                {
                    domain = i.ChildNodes().First().ToString();
                }
                var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                var subRoot = (ExpressionSyntax)subTree.GetRoot();
                var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                ComputeOutputs(newRoot.ToString());
                break;
            }
        }
        private List<List<Tuple<string, string>>> IP_PRT(string[] x, string[] y, SyntaxKind z)
        {
            List<List<Tuple<string, string>>> currentSet = new List<List<Tuple<string, string>>>();
            List<List<Tuple<string, string>>> newSet = new List<List<Tuple<string, string>>>();
            List<Tuple<string, string>> domain = new List<Tuple<string, string>>()
            {
                Tuple.Create(x[0], x[1]), Tuple.Create(y[0], y[1])
            };
            bool consistency = CheckConsistency(domain[0], domain[1], z);
            if (consistency == true)
            {
                currentSet.Add(domain);
            }
            for (int i = 0; i < divisionParameter; i++)
            {
                if (currentSet == null)
                {
                    break;
                }
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < currentSet.Count; k++)
                    {
                        string lowerBound = currentSet[k][j].Item1;
                        string upperBound = currentSet[k][j].Item2;
                        double middel = (double.Parse(lowerBound) + double.Parse(upperBound)) / 2;
                        List<List<Tuple<string, string>>> currentSet1 = new List<List<Tuple<string, string>>>(currentSet.Count);
                        currentSet.ForEach((item) =>
                        {
                            currentSet1.Add(new List<Tuple<string, string>>(item));
                        });
                        currentSet1[k][j] = Tuple.Create(lowerBound, middel.ToString());
                        bool consistency1 = CheckConsistency(currentSet1[k][0], currentSet1[k][1], z);
                        if (consistency1 == true)
                        {
                            newSet.Add(currentSet1[k]);
                            currentSet1.Clear();
                        }
                        List<List<Tuple<string, string>>> currentSet2 = new List<List<Tuple<string, string>>>(currentSet.Count);
                        currentSet.ForEach((item) =>
                        {
                            currentSet2.Add(new List<Tuple<string, string>>(item));
                        });
                        currentSet2[k][j] = Tuple.Create(middel.ToString(), upperBound);
                        if (lowerBound != upperBound)
                        {
                            bool consistency2 = CheckConsistency(currentSet2[k][0], currentSet2[k][1], z);
                            if (consistency2 == true)
                            {
                                newSet.Add(currentSet2[k]);
                                currentSet2.Clear();
                            }
                        }
                    }
                    List<List<Tuple<string, string>>> newCurrentSet = new List<List<Tuple<string, string>>>(newSet.Count);
                    newSet.ForEach((item) =>
                    {
                        newCurrentSet.Add(new List<Tuple<string, string>>(item));
                    });
                    newSet.Clear();
                    currentSet = newCurrentSet;
                }
            }
            return currentSet;
        }
        private bool CheckConsistency(Tuple<string, string> x, Tuple<string, string> y, SyntaxKind z)
        {
            if ((double.Parse(x.Item1) == double.Parse(x.Item2)) && (double.Parse(y.Item1) == double.Parse(y.Item2)))
            {
                switch (z)
                {
                    case SyntaxKind.EqualsExpression:
                        if (double.Parse(x.Item1) == double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case SyntaxKind.NotEqualsExpression:
                        if (double.Parse(x.Item1) != double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case SyntaxKind.LessThanExpression:
                        if (double.Parse(x.Item1) < double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case SyntaxKind.LessThanOrEqualExpression:
                        if (double.Parse(x.Item1) <= double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case SyntaxKind.GreaterThanExpression:
                        if (double.Parse(x.Item1) > double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case SyntaxKind.GreaterThanOrEqualExpression:
                        if (double.Parse(x.Item1) >= double.Parse(y.Item1))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            }
            else if (double.Parse(x.Item1) == double.Parse(x.Item2))
            {
                switch (z)
                {
                    case SyntaxKind.EqualsExpression:
                        if ((double.Parse(x.Item1) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001)))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.NotEqualsExpression:
                        return true;
                    case SyntaxKind.LessThanExpression:
                        if (double.Parse(x.Item1) >= (double.Parse(y.Item2) - 0.001))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.LessThanOrEqualExpression:
                        if (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanExpression:
                        if (double.Parse(x.Item1) <= double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanOrEqualExpression:
                        if (double.Parse(x.Item1) < double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    default:
                        return false;
                }
            }
            else if (double.Parse(y.Item1) == double.Parse(y.Item2))
            {
                switch (z)
                {
                    case SyntaxKind.EqualsExpression:
                        if (((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > double.Parse(y.Item1)))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.NotEqualsExpression:
                        return true;
                    case SyntaxKind.LessThanExpression:
                        if (double.Parse(x.Item1) >= double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.LessThanOrEqualExpression:
                        if (double.Parse(x.Item1) > double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanExpression:
                        if ((double.Parse(x.Item2) - 0.001) <= double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanOrEqualExpression:
                        if ((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    default:
                        return false;
                }
            }
            else
            {
                switch (z)
                {
                    case SyntaxKind.EqualsExpression:
                        if (((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1)) || (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001)))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.NotEqualsExpression:
                        return true;
                    case SyntaxKind.LessThanExpression:
                        if (double.Parse(x.Item1) >= (double.Parse(y.Item2) - 0.001))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.LessThanOrEqualExpression:
                        if (double.Parse(x.Item1) > (double.Parse(y.Item2) - 0.001))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanExpression:
                        if ((double.Parse(x.Item2) - 0.001) <= double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    case SyntaxKind.GreaterThanOrEqualExpression:
                        if ((double.Parse(x.Item2) - 0.001) < double.Parse(y.Item1))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    default:
                        return false;
                }
            }
        }
        private Tuple<SyntaxNode, SyntaxNode> GetComparisonExpression(int x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_IntFloatDouble).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.EqualsExpression || i.Kind() == SyntaxKind.NotEqualsExpression || i.Kind() == SyntaxKind.LessThanExpression || i.Kind() == SyntaxKind.LessThanOrEqualExpression || i.Kind() == SyntaxKind.GreaterThanExpression || i.Kind() == SyntaxKind.GreaterThanOrEqualExpression)
                {
                    list.Add(i);
                }
            }
            list.Reverse();
            return Tuple.Create(list[x].ChildNodes().First(), list[x].ChildNodes().Last());
        }
        public void Update(SyntaxNode x, Tuple<string, string> y)
        {
            int m = 0;
            int n = 0;
            switch (x.Kind())
            {
                case SyntaxKind.ParenthesizedExpression:
                    Update(x.ChildNodes().First(), y);
                    break;
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.ElementAccessExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.IdentifierName:
                    if (update.Contains(Tuple.Create(x.ToString(), Tuple.Create(y.Item1, y.Item2))) == false)
                    {
                        update.Add(Tuple.Create(x.ToString(), Tuple.Create(y.Item1, y.Item2)));
                    }
                    break;
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                    SyntaxNode left = x.ChildNodes().First();
                    while (left.Kind() == SyntaxKind.ParenthesizedExpression)
                    {
                        left = left.ChildNodes().First();
                    }
                    SyntaxNode right = x.ChildNodes().Last();
                    while (right.Kind() == SyntaxKind.ParenthesizedExpression)
                    {
                        right = right.ChildNodes().First();
                    }
                    Tuple<string, string> leftDomain = null;
                    Tuple<string, string> rightDomain = null;
                    switch (x.Kind())
                    {
                        case SyntaxKind.AddExpression:
                            if ((left.Kind() == SyntaxKind.NumericLiteralExpression) || (left.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                rightDomain = Tuple.Create((double.Parse(y.Item1) - double.Parse(left.ToString())).ToString(), (double.Parse(y.Item2) - double.Parse(left.ToString())).ToString());
                                Update(right, rightDomain);
                            }
                            else if ((right.Kind() == SyntaxKind.NumericLiteralExpression) || (right.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                leftDomain = Tuple.Create((double.Parse(y.Item1) - double.Parse(right.ToString())).ToString(), (double.Parse(y.Item2) - double.Parse(right.ToString())).ToString());
                                Update(left, leftDomain);
                            }
                            else
                            {
                                double lowerBoundLeft = double.Parse(y.Item1) / 2;
                                double upperBoundLeft = double.Parse(y.Item2) / 2;
                                if (lowerBoundLeft > upperBoundLeft)
                                {
                                    double temporaryVariable = lowerBoundLeft;
                                    lowerBoundLeft = upperBoundLeft;
                                    upperBoundLeft = temporaryVariable;
                                }
                                double lowerBoundRight = double.Parse(y.Item1) / 2;
                                double upperBoundRight = double.Parse(y.Item2) / 2;
                                if (lowerBoundRight > upperBoundRight)
                                {
                                    double temporaryVariable = lowerBoundRight;
                                    lowerBoundRight = upperBoundRight;
                                    upperBoundRight = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBoundLeft.ToString(), upperBoundLeft.ToString());
                                rightDomain = Tuple.Create(lowerBoundRight.ToString(), upperBoundRight.ToString());
                                Update(left, leftDomain);
                                Update(right, rightDomain);
                            }
                            break;
                        case SyntaxKind.SubtractExpression:
                            if ((left.Kind() == SyntaxKind.NumericLiteralExpression) || (left.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                rightDomain = Tuple.Create((double.Parse(left.ToString()) - double.Parse(y.Item2)).ToString(), (double.Parse(left.ToString()) - double.Parse(y.Item1)).ToString());
                                Update(right, rightDomain);
                            }
                            else if ((right.Kind() == SyntaxKind.NumericLiteralExpression) || (right.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                leftDomain = Tuple.Create((double.Parse(y.Item1) + double.Parse(right.ToString())).ToString(), (double.Parse(y.Item2) + double.Parse(right.ToString())).ToString());
                                Update(left, leftDomain);
                            }
                            else
                            {
                                double lowerBoundLeft = double.Parse(y.Item2) + m * (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2;
                                double upperBoundLeft = (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2 + double.Parse(y.Item2) + m * (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2;
                                if (lowerBoundLeft > upperBoundLeft)
                                {
                                    double temporaryVariable = lowerBoundLeft;
                                    lowerBoundLeft = upperBoundLeft;
                                    upperBoundLeft = temporaryVariable;
                                }
                                double lowerBoundRight = (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2 + m * (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2;
                                double upperBoundRight = double.Parse(y.Item2) - double.Parse(y.Item1) + m * (double.Parse(y.Item2) - double.Parse(y.Item1)) / 2;
                                if (lowerBoundRight > upperBoundRight)
                                {
                                    double temporaryVariable = lowerBoundRight;
                                    lowerBoundRight = upperBoundRight;
                                    upperBoundRight = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBoundLeft.ToString(), upperBoundLeft.ToString());
                                rightDomain = Tuple.Create(lowerBoundRight.ToString(), upperBoundRight.ToString());
                                Update(left, leftDomain);
                                Update(right, rightDomain);
                            }
                            break;
                        case SyntaxKind.MultiplyExpression:
                            if ((left.Kind() == SyntaxKind.NumericLiteralExpression) || (left.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                double lowerBound = double.Parse(y.Item1) / double.Parse(left.ToString());
                                double upperBound = double.Parse(y.Item2) / double.Parse(left.ToString());
                                if (lowerBound > upperBound)
                                {
                                    double temporaryVariable = lowerBound;
                                    lowerBound = upperBound;
                                    upperBound = temporaryVariable;
                                }
                                rightDomain = Tuple.Create(lowerBound.ToString(), upperBound.ToString());
                                Update(right, rightDomain);
                            }
                            else if ((right.Kind() == SyntaxKind.NumericLiteralExpression) || (right.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                double lowerBound = double.Parse(y.Item1) / double.Parse(right.ToString());
                                double upperBound = double.Parse(y.Item2) / double.Parse(right.ToString());
                                if (lowerBound > upperBound)
                                {
                                    double temporaryVariable = lowerBound;
                                    lowerBound = upperBound;
                                    upperBound = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBound.ToString(), upperBound.ToString());
                                Update(left, leftDomain);
                            }
                            else
                            {
                                double lowerBoundLeft = 0;
                                double upperBoundLeft = 0;
                                double lowerBoundRight = 0;
                                double upperBoundRight = 0;
                                if (double.Parse(y.Item1) >= 0 && double.Parse(y.Item2) >= 0)
                                {
                                    lowerBoundLeft = Math.Sqrt(double.Parse(y.Item2));
                                    upperBoundLeft = Math.Sqrt(double.Parse(y.Item1));
                                    lowerBoundRight = Math.Sqrt(double.Parse(y.Item2));
                                    upperBoundRight = Math.Sqrt(double.Parse(y.Item1));
                                }
                                else if (double.Parse(y.Item1) < 0 && double.Parse(y.Item2) < 0)
                                {
                                    lowerBoundLeft = 1;
                                    upperBoundLeft = Math.Abs(double.Parse(y.Item2));
                                    lowerBoundRight = double.Parse(y.Item1) / Math.Abs(double.Parse(y.Item2));
                                    upperBoundRight = -1;
                                }
                                else if (double.Parse(y.Item1) < 0 && double.Parse(y.Item2) >= 0)
                                {
                                    lowerBoundLeft = -Math.Sqrt(Math.Abs(double.Parse(y.Item1)));
                                    upperBoundLeft = Math.Min(Math.Floor(Math.Sqrt(Math.Abs(double.Parse(y.Item1)))), Math.Floor(double.Parse(y.Item2) / Math.Sqrt(Math.Abs(double.Parse(y.Item1)))));
                                    lowerBoundRight = -Math.Min(Math.Floor(Math.Sqrt(Math.Abs(double.Parse(y.Item1)))), Math.Floor(double.Parse(y.Item2) / Math.Sqrt(Math.Abs(double.Parse(y.Item1)))));
                                    upperBoundRight = Math.Sqrt(Math.Abs(double.Parse(y.Item1)));
                                }
                                if (lowerBoundLeft > upperBoundLeft)
                                {
                                    double temporaryVariable = lowerBoundLeft;
                                    lowerBoundLeft = upperBoundLeft;
                                    upperBoundLeft = temporaryVariable;
                                }
                                if (lowerBoundRight > upperBoundRight)
                                {
                                    double temporaryVariable = lowerBoundRight;
                                    lowerBoundRight = upperBoundRight;
                                    upperBoundRight = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBoundLeft.ToString(), upperBoundLeft.ToString());
                                rightDomain = Tuple.Create(lowerBoundRight.ToString(), upperBoundRight.ToString());
                                Update(left, leftDomain);
                                Update(right, rightDomain);
                            }
                            break;
                        case SyntaxKind.DivideExpression:
                            if ((left.Kind() == SyntaxKind.NumericLiteralExpression) || (left.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                double lowerBound = double.Parse(left.ToString()) / double.Parse(y.Item1);
                                double upperBound = double.Parse(left.ToString()) / double.Parse(y.Item2);
                                if (lowerBound > upperBound)
                                {
                                    double temporaryVariable = lowerBound;
                                    lowerBound = upperBound;
                                    upperBound = temporaryVariable;
                                }
                                rightDomain = Tuple.Create(lowerBound.ToString(), upperBound.ToString());
                                Update(right, rightDomain);
                            }
                            else if ((right.Kind() == SyntaxKind.NumericLiteralExpression) || (right.Kind() == SyntaxKind.UnaryMinusExpression))
                            {
                                double lowerBound = double.Parse(right.ToString()) * double.Parse(y.Item1);
                                double upperBound = double.Parse(right.ToString()) * double.Parse(y.Item2);
                                if (lowerBound > upperBound)
                                {
                                    double temporaryVariable = lowerBound;
                                    lowerBound = upperBound;
                                    upperBound = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBound.ToString(), upperBound.ToString());
                                Update(left, leftDomain);
                            }
                            else
                            {
                                double lowerBoundLeft = (double.Parse(y.Item2) + 1) / (double.Parse(y.Item1) + 1) * Math.Pow(double.Parse(y.Item1), n + 1) * Math.Pow(double.Parse(y.Item2), n);
                                double upperBoundLeft = Math.Pow(double.Parse(y.Item1), n) * Math.Pow(double.Parse(y.Item2), n + 1);
                                if (lowerBoundLeft > upperBoundLeft)
                                {
                                    double temporaryVariable = lowerBoundLeft;
                                    lowerBoundLeft = upperBoundLeft;
                                    upperBoundLeft = temporaryVariable;
                                }
                                double lowerBoundRight = Math.Pow(double.Parse(y.Item1), n) * Math.Pow(double.Parse(y.Item2), n);
                                double upperBoundRight = (double.Parse(y.Item2) + 1) / (double.Parse(y.Item1) + 1) * Math.Pow(double.Parse(y.Item1), n) * Math.Pow(double.Parse(y.Item2), n + 1);
                                if (lowerBoundRight > upperBoundRight)
                                {
                                    double temporaryVariable = lowerBoundRight;
                                    lowerBoundRight = upperBoundRight;
                                    upperBoundRight = temporaryVariable;
                                }
                                leftDomain = Tuple.Create(lowerBoundLeft.ToString(), upperBoundLeft.ToString());
                                rightDomain = Tuple.Create(lowerBoundRight.ToString(), upperBoundRight.ToString());
                                Update(left, leftDomain);
                                Update(right, rightDomain);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
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