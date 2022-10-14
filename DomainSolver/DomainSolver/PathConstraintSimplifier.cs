using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class PathConstraintSimplifier
    {
        private string sourceCodePath;
        private string namespaceName;
        private string methodName;
        public string pathConstraint;
        public PathConstraintSimplifier(string sourceCodePath, string methodName, string pathConstraint)
        {
            this.sourceCodePath = sourceCodePath;
            namespaceName = sourceCodePath.Replace("\\" + sourceCodePath.Split('\\').Last(), "").Split('\\').Last();
            this.methodName = methodName;
            this.pathConstraint = pathConstraint;
        }
        public void PropagateNotOperators()
        {
            var tree = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.LogicalNotExpression)
                {
                    SyntaxNode j = i;
                    do
                    {
                        j = j.ChildNodes().First();
                    } while (j.Kind() == SyntaxKind.ParenthesizedExpression);
                    switch (j.Kind())
                    {
                        case SyntaxKind.EqualsExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + j.ChildNodes().First() + "!=" + j.ChildNodes().Last() + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.LessThanExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + j.ChildNodes().First() + ">=" + j.ChildNodes().Last() + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.LessThanOrEqualExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + j.ChildNodes().First() + ">" + j.ChildNodes().Last() + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.GreaterThanExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + j.ChildNodes().First() + "<=" + j.ChildNodes().Last() + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.GreaterThanOrEqualExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + j.ChildNodes().First() + "<" + j.ChildNodes().Last() + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.LogicalAndExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + "!(" + j.ChildNodes().First() + ")" + "||" + "!(" + j.ChildNodes().Last() + ")" + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.LogicalOrExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), "(" + "!(" + j.ChildNodes().First() + ")" + "&&" + "!(" + j.ChildNodes().Last() + ")" + ")");
                            PropagateNotOperators();
                            return;
                        case SyntaxKind.LogicalNotExpression:
                            pathConstraint = pathConstraint.Replace(i.ToString(), j.ChildNodes().First().ToString());
                            PropagateNotOperators();
                            return;
                        default:
                            break;
                    }
                }
            }
        }
        public void CalculateComputableExpressions()
        {
            var tree = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf().Reverse())
            {
                if ((i.Kind() == SyntaxKind.AddExpression) || (i.Kind() == SyntaxKind.SubtractExpression) || (i.Kind() == SyntaxKind.MultiplyExpression) || (i.Kind() == SyntaxKind.DivideExpression))
                {
                    SyntaxNode left = i.ChildNodes().First();
                    while (left.Kind() == SyntaxKind.ParenthesizedExpression)
                    {
                        left = left.ChildNodes().First();
                    }
                    SyntaxNode right = i.ChildNodes().Last();
                    while (right.Kind() == SyntaxKind.ParenthesizedExpression)
                    {
                        right = right.ChildNodes().First();
                    }
                    if (((left.Kind() == SyntaxKind.NumericLiteralExpression) || (left.Kind() == SyntaxKind.UnaryMinusExpression)) && ((right.Kind() == SyntaxKind.NumericLiteralExpression) || (right.Kind() == SyntaxKind.UnaryMinusExpression)))
                    {
                        DataTable dataTable = new DataTable();
                        string domain = dataTable.Compute(i.ToString(), "").ToString();
                        var subTree = SyntaxFactory.ParseExpression(domain).SyntaxTree;
                        var subRoot = (ExpressionSyntax)subTree.GetRoot();
                        root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                        pathConstraint = root.ToString();
                        CalculateComputableExpressions();
                        return;
                    }
                }
            }
        }
        public void AddArraysLengthConstraints()
        {
            var tree = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            var elementAccessExpressions = root.DescendantNodesAndSelf().Reverse().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
            {
                var argument = i.DescendantNodesAndSelf().OfType<ArgumentSyntax>().First().Expression;
                if (argument.Kind() != SyntaxKind.NumericLiteralExpression)
                {
                    SyntaxNode j = i;
                    while ((j.Parent != null) && (j.Parent.Kind() != SyntaxKind.EqualsExpression) && (j.Parent.Kind() != SyntaxKind.NotEqualsExpression) && (j.Parent.Kind() != SyntaxKind.LessThanExpression) && (j.Parent.Kind() != SyntaxKind.LessThanOrEqualExpression) && (j.Parent.Kind() != SyntaxKind.GreaterThanExpression) && (j.Parent.Kind() != SyntaxKind.GreaterThanOrEqualExpression))
                    {
                        j = j.Parent;
                    }
                    int arrayLength = DetermineLength(i.ChildNodes().First());
                    string newConstraints = "(" + j.Parent + "&&" + argument + ">=" + "0" + "&&" + argument + "<" + arrayLength + ")";
                    var subTree = SyntaxFactory.ParseExpression(newConstraints).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(j.Parent.Span), subRoot);
                }
            }
            pathConstraint = root.ToString();
        }
        public void AddStructsConstraints()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if ((i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression))
                {
                    list.Add(i);
                }
            }
            list.Reverse();
            foreach (var i in list)
            {
                string leftDataType = DetermineDataType(i.ChildNodes().First());
                string rightDataType = DetermineDataType(i.ChildNodes().Last());
                if ((leftDataType != null) && (leftDataType != "bool") && (leftDataType != "System.Boolean") && (leftDataType != "int") && (leftDataType != "System.Int32") && (leftDataType != "float") && (leftDataType != "System.Single") && (leftDataType != "double") && (leftDataType != "System.Double") && (leftDataType != "char") && (leftDataType != "System.Char") && (leftDataType != "string") && (leftDataType != "System.String") && (leftDataType.Contains("[]") == false))
                {
                    List<Tuple<string, string>> leftFields = DetermineFields(leftDataType);
                    List<Tuple<string, string>> rightFields = DetermineFields(rightDataType);
                    if (leftFields.Count != rightFields.Count)
                    {
                        pathConstraint = null;
                        return;
                    }
                    string newConstraints = null;
                    for (int j = 0; j < leftFields.Count; j++)
                    {
                        if (leftFields[j].Item2 != rightFields[j].Item2)
                        {
                            pathConstraint = null;
                            return;
                        }
                        if (i.Kind() == SyntaxKind.EqualsExpression)
                        {
                            newConstraints = newConstraints + i.ChildNodes().First() + "." + leftFields[j].Item1 + "==" + i.ChildNodes().Last() + "." + leftFields[j].Item1 + "&&";
                        }
                        else
                        {
                            newConstraints = newConstraints + i.ChildNodes().First() + "." + leftFields[j].Item1 + "!=" + i.ChildNodes().Last() + "." + leftFields[j].Item1 + "||";
                        }
                    }
                    newConstraints = newConstraints.Remove(newConstraints.Length - 2);
                    var subTree = SyntaxFactory.ParseExpression(newConstraints).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                }
            }
            pathConstraint = root.ToString();
        }
        public List<string> RemoveOrOperators()
        {
            List<string> list = new List<string>()
            {
                pathConstraint
            };
            for (int i = 0; i < list.Count; i++)
            {
                var tree = SyntaxFactory.ParseExpression(list[i]).SyntaxTree;
                var root = (ExpressionSyntax)tree.GetRoot();
                foreach (var j in root.DescendantNodesAndSelf())
                {
                    if (j.Kind() == SyntaxKind.LogicalOrExpression)
                    {
                        list.Add(list[i].Replace(j.ChildNodes().First() + "||", ""));
                        list.Add(list[i].Replace("||" + j.ChildNodes().Last(), ""));
                        list.RemoveAt(i);
                        i = i - 1;
                        break;
                    }
                }
            }
            return list;
        }
        private int DetermineLength(SyntaxNode x)
        {
            if (x.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                return 5;
            }
            else if (x.Kind() == SyntaxKind.IdentifierName)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceCodePath));
                var root = (CompilationUnitSyntax)tree.GetRoot();
                var methodDeclarations = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                foreach (var i in methodDeclarations)
                {
                    if (i.Identifier.ToString() == methodName)
                    {
                        var parameters = i.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                        foreach (var j in parameters)
                        {
                            if (j.Identifier.ToString() == x.ToString())
                            {
                                return 5;
                            }
                        }
                        var variableDeclarations = i.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                        foreach (var j in variableDeclarations)
                        {
                            if (j.ChildNodes().Last().ChildTokens().First().ToString() == x.ToString())
                            {
                                var text = j.DescendantNodesAndSelf().OfType<ArrayRankSpecifierSyntax>().Last().ChildNodes().First();
                                if (text.Kind() == SyntaxKind.NumericLiteralExpression)
                                {
                                    return int.Parse(text.ToString());
                                }
                            }
                        }
                        var assignmentExpressions = i.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>();
                        foreach (var j in assignmentExpressions)
                        {
                            if (j.ChildNodes().First().ChildTokens().First().ToString() == x.ToString())
                            {
                                var text = j.DescendantNodesAndSelf().OfType<ArrayRankSpecifierSyntax>().First().ChildNodes().First();
                                if (text.Kind() == SyntaxKind.NumericLiteralExpression)
                                {
                                    return int.Parse(text.ToString());
                                }
                                else
                                {
                                    return 5;
                                }
                            }
                        }
                        return 5;
                    }
                }
            }
            return 0;
        }
        private string DetermineDataType(SyntaxNode x)
        {
            string dataType = null;
            switch (x.Kind())
            {
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.ElementAccessExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                    break;
                case SyntaxKind.IdentifierName:
                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceCodePath));
                    var root = (CompilationUnitSyntax)tree.GetRoot();
                    var methodDeclarations = root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                    foreach (var i in methodDeclarations)
                    {
                        if (i.Identifier.ToString() == methodName)
                        {
                            var parameters = i.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                            foreach (var j in parameters)
                            {
                                if (j.Identifier.ToString() == x.ToString())
                                {
                                    dataType = j.Type.ToString();
                                    break;
                                }
                            }
                            var variableDeclarations = i.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                            foreach (var j in variableDeclarations)
                            {
                                if (j.ChildNodes().Last().ChildTokens().First().ToString() == x.ToString())
                                {
                                    dataType = j.Type.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return dataType;
        }
        private List<Tuple<string, string>> DetermineFields(string x)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            string[] text = x.Split('.');
            int counter;
            Assembly assembly;
            if (File.Exists(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text[0] + ".dll") == true)
            {
                counter = 1;
                assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text[0] + ".dll");
            }
            else
            {
                counter = 0;
                assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
            }
            Type type = null;
            var types = assembly.GetTypes();
            foreach (var i in types)
            {
                if ((counter < text.Count()) && (i.Name == text[counter]))
                {
                    types = i.GetNestedTypes();
                    counter = counter + 1;
                    type = i;
                }
            }
            var fields = type.GetFields();
            foreach (var i in fields)
            {
                list.Add(Tuple.Create(i.Name, i.FieldType.FullName));
            }
            return list;
        }
    }
}