using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class PathConstraintClassifier
    {
        public string pathConstraint_Bool;
        public string pathConstraint_IntFloatDouble;
        public string pathConstraint_CharString;
        public string pathConstraint_Array;
        public PathConstraintClassifier(string pathConstraint)
        {
            pathConstraint_Bool = pathConstraint;
            pathConstraint_IntFloatDouble = pathConstraint;
            pathConstraint_CharString = pathConstraint;
            pathConstraint_Array = pathConstraint;
        }
        public void PathConstraint_Bool()
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
            var elementAccessExpressions = root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
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
            var memberAccessExpressions = root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
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
                    list.Add(i);
                }
            }
            var identifierNames = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                if (i.ToString() != "")
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
            foreach (var i in list)
            {
                string dataType;
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.methodCallsInformation[index].Item5;
                }
                else
                {
                    int index = Form1.variablesInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.variablesInformation[index].Item2.Item1;
                }
                if ((dataType != "bool") && (dataType != "System.Boolean"))
                {
                    SyntaxNode j = i;
                    while ((j.Parent != null) && (j.Parent.Kind() != SyntaxKind.LogicalAndExpression))
                    {
                        j = j.Parent;
                    }
                    if (j.Parent == null)
                    {
                        pathConstraint_Bool = pathConstraint_Bool.Replace(j.ToString(), "");
                    }
                    else
                    {
                        string domain = null;
                        if (j.Parent.ChildNodes().First() == j)
                        {
                            domain = j.Parent.ChildNodes().Last().ToString();
                        }
                        else if (j.Parent.ChildNodes().Last() == j)
                        {
                            domain = j.Parent.ChildNodes().First().ToString();
                        }
                        var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                        var subRoot = (ExpressionSyntax)subTree.GetRoot();
                        root = root.ReplaceNode(root.FindNode(j.Parent.Span), subRoot);
                        pathConstraint_Bool = root.ToString();
                    }
                    PathConstraint_Bool();
                    break;
                }
            }
        }
        public void PathConstraint_IntFloatDouble()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_IntFloatDouble).SyntaxTree;
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
            var elementAccessExpressions = root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
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
            var memberAccessExpressions = root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
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
            var identifierNames = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                if (i.ToString() != "")
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
            foreach (var i in list)
            {
                string dataType;
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.methodCallsInformation[index].Item5;
                }
                else
                {
                    int index = Form1.variablesInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.variablesInformation[index].Item2.Item1;
                }
                if ((dataType != "int") && (dataType != "System.Int32") && (dataType != "float") && (dataType != "System.Single") && (dataType != "double") && (dataType != "System.Double"))
                {
                    SyntaxNode j = i;
                    while ((j.Parent != null) && (j.Parent.Kind() != SyntaxKind.LogicalAndExpression))
                    {
                        j = j.Parent;
                    }
                    if (j.Parent == null)
                    {
                        pathConstraint_IntFloatDouble = pathConstraint_IntFloatDouble.Replace(j.ToString(), "");
                    }
                    else
                    {
                        string domain = null;
                        if (j.Parent.ChildNodes().First() == j)
                        {
                            domain = j.Parent.ChildNodes().Last().ToString();
                        }
                        else if (j.Parent.ChildNodes().Last() == j)
                        {
                            domain = j.Parent.ChildNodes().First().ToString();
                        }
                        var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                        var subRoot = (ExpressionSyntax)subTree.GetRoot();
                        root = root.ReplaceNode(root.FindNode(j.Parent.Span), subRoot);
                        pathConstraint_IntFloatDouble = root.ToString();
                    }
                    PathConstraint_IntFloatDouble();
                    break;
                }
            }
        }
        public void PathConstraint_CharString()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_CharString).SyntaxTree;
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
            var elementAccessExpressions = root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
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
            var memberAccessExpressions = root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
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
            var identifierNames = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                if (i.ToString() != "")
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
            foreach (var i in list)
            {
                string dataType;
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.methodCallsInformation[index].Item5;
                }
                else
                {
                    int index = Form1.variablesInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.variablesInformation[index].Item2.Item1;
                }
                if ((dataType != "char") && (dataType != "System.Char") && (dataType != "string") && (dataType != "System.String"))
                {
                    SyntaxNode j = i;
                    while ((j.Parent != null) && (j.Parent.Kind() != SyntaxKind.LogicalAndExpression))
                    {
                        j = j.Parent;
                    }
                    if (j.Parent == null)
                    {
                        pathConstraint_CharString = pathConstraint_CharString.Replace(j.ToString(), "");
                    }
                    else
                    {
                        string domain = null;
                        if (j.Parent.ChildNodes().First() == j)
                        {
                            domain = j.Parent.ChildNodes().Last().ToString();
                        }
                        else if (j.Parent.ChildNodes().Last() == j)
                        {
                            domain = j.Parent.ChildNodes().First().ToString();
                        }
                        var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                        var subRoot = (ExpressionSyntax)subTree.GetRoot();
                        root = root.ReplaceNode(root.FindNode(j.Parent.Span), subRoot);
                        pathConstraint_CharString = root.ToString();
                    }
                    PathConstraint_CharString();
                    break;
                }
            }
        }
        public void PathConstraint_Array()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_Array).SyntaxTree;
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
            var elementAccessExpressions = root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
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
            var memberAccessExpressions = root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
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
                    list.Add(i);
                }
            }
            var identifierNames = root.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                if (i.ToString() != "")
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
            foreach (var i in list)
            {
                string dataType;
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.methodCallsInformation[index].Item5;
                }
                else
                {
                    int index = Form1.variablesInformation.FindIndex(j => j.Item1 == i.ToString());
                    dataType = Form1.variablesInformation[index].Item2.Item1;
                }
                if (dataType.Contains("[]") == false)
                {
                    SyntaxNode j = i;
                    while ((j.Parent != null) && (j.Parent.Kind() != SyntaxKind.LogicalAndExpression))
                    {
                        j = j.Parent;
                    }
                    if (j.Parent == null)
                    {
                        pathConstraint_Array = pathConstraint_Array.Replace(j.ToString(), "");
                    }
                    else
                    {
                        string domain = null;
                        if (j.Parent.ChildNodes().First() == j)
                        {
                            domain = j.Parent.ChildNodes().Last().ToString();
                        }
                        else if (j.Parent.ChildNodes().Last() == j)
                        {
                            domain = j.Parent.ChildNodes().First().ToString();
                        }
                        var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                        var subRoot = (ExpressionSyntax)subTree.GetRoot();
                        root = root.ReplaceNode(root.FindNode(j.Parent.Span), subRoot);
                        pathConstraint_Array = root.ToString();
                    }
                    PathConstraint_Array();
                    break;
                }
            }
        }
    }
}