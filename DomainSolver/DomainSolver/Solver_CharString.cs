using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class Solver_CharString
    {
        private int counter = 0;
        private bool isSatisfiable = true;
        private Random random = new Random();
        private List<Tuple<List<string>, SyntaxKind, List<string>>> finalList;
        private List<Tuple<string, int>> variablesLength = new List<Tuple<string, int>>();

        private string pathConstraint_CharString;
        public Solver_CharString(string pathConstraint_CharString)
        {
            this.pathConstraint_CharString = pathConstraint_CharString;
        }
        public object DetermineVariablesLength()
        {
            var tree = SyntaxFactory.ParseExpression(pathConstraint_CharString).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            List<string> allVariables = DetermineAllVaribles(root).ConvertAll(i => i.ToString());
            allVariables = allVariables.Distinct().ToList();
            foreach (var i in allVariables)
            {
                int index1 = Form1.variablesInformation.FindIndex(j => j.Item1 == i);
                string dataType = Form1.variablesInformation[index1].Item2.Item1;
                if ((dataType == "char") || (dataType == "System.Char"))
                {
                    variablesLength.Add(Tuple.Create(i, 1));
                    continue;
                }
                int index2 = Form1.stringsLength.FindIndex(j => j.Item1 == i);
                if (index2 != -1)
                {
                    variablesLength.Add(Tuple.Create(i, Form1.stringsLength[index2].Item2));
                    continue;
                }
            }
            object obj = GenerateLinearEquationsSystem();
            if (obj.GetType() == typeof(string))
            {
                return "Unsatisfiable";
            }
            Tuple<string, string> linearEquationsSystem = (Tuple<string, string>)obj;
            if ((linearEquationsSystem.Item1 != null) && (linearEquationsSystem.Item2 != null))
            {
                string result = SolveLinearEquationsSystem(linearEquationsSystem.Item1, linearEquationsSystem.Item2);
                if (result == "Unsatisfiable")
                {
                    return "Unsatisfiable";
                }
                result = result.Remove(0, 11);
                result = result.Remove(result.Length - 4);
                if ((result.Contains(", Variable_001, Variable_001") == true) || (result.Contains(", Variable_002, Variable_002") == true))
                {
                    result = result.Remove(result.Length - 28);
                }
                string[] results = result.Split(',');
                List<Tuple<string, int>> variablesInResult = new List<Tuple<string, int>>();
                foreach (var i in results)
                {
                    List<SyntaxNode> list1 = new List<SyntaxNode>();
                    var tree1 = SyntaxFactory.ParseExpression(i).SyntaxTree;
                    var root1 = (ExpressionSyntax)tree1.GetRoot();
                    list1 = DetermineAllVaribles(root1);
                    foreach (var j in list1)
                    {
                        if (variablesInResult.Any(k => k.Item1 == j.ToString()) == false)
                        {
                            variablesInResult.Add(Tuple.Create(j.ToString(), 1));
                        }
                    }
                }
                bool flag;
                string[] newResults;
                DataTable dataTable = new DataTable();
                do
                {
                    flag = true;
                    newResults = new string[results.Length];
                    results.CopyTo(newResults, 0);
                    for (int i = 0; i < newResults.Count(); i++)
                    {
                        foreach (var j in variablesInResult)
                        {
                            if (newResults[i].Contains(j.Item1) == true)
                            {
                                newResults[i] = newResults[i].Replace(j.Item1, j.Item2.ToString());
                            }
                        }
                    }
                    for (int i = 0; i < newResults.Count(); i++)
                    {
                        if ((int)dataTable.Compute(newResults[i], "") <= 0)
                        {
                            for (int j = 0; j < variablesInResult.Count(); j++)
                            {
                                if (results[i].Contains(variablesInResult[j].Item1) == true)
                                {
                                    variablesInResult[j] = Tuple.Create(variablesInResult[j].Item1, (variablesInResult[j].Item2 - (int)dataTable.Compute(newResults[i], "") + 1));
                                }
                            }
                            flag = false;
                            break;
                        }
                    }
                } while (flag == false);
                string[] linearEquationsSystem_variables = linearEquationsSystem.Item1.Split(',');
                for (int i = 0; i < linearEquationsSystem_variables.Count(); i++)
                {
                    linearEquationsSystem_variables[i] = linearEquationsSystem_variables[i].Trim();
                }
                for (int i = 0; i < linearEquationsSystem_variables.Count(); i++)
                {
                    variablesLength.Add(Tuple.Create(linearEquationsSystem_variables[i], (int)dataTable.Compute(newResults[i], "")));
                }
                for (int i = 0; i < variablesLength.Count(); i++)
                {
                    if (variablesLength[i].Item1.Contains("_001") == true)
                    {
                        if (variablesLength[i].Item1.Contains("[") == true)
                        {
                            variablesLength[i] = Tuple.Create(variablesLength[i].Item1.Replace("_001", ""), variablesLength[i].Item2);
                            variablesLength.RemoveAt(variablesLength.IndexOf(Tuple.Create(variablesLength[i].Item1.Split('[').First() + "_002" + "[" + variablesLength[i].Item1.Split('[').Last(), variablesLength[i].Item2)));
                        }
                        else
                        {
                            variablesLength[i] = Tuple.Create(variablesLength[i].Item1.Replace("_001", ""), variablesLength[i].Item2);
                            variablesLength.RemoveAt(variablesLength.IndexOf(Tuple.Create(variablesLength[i].Item1 + "_002", variablesLength[i].Item2)));
                        }
                    }
                    else if (variablesLength[i].Item1.Contains("_002") == true)
                    {
                        if (variablesLength[i].Item1.Contains("[") == true)
                        {
                            variablesLength[i] = Tuple.Create(variablesLength[i].Item1.Replace("_002", ""), variablesLength[i].Item2);
                            variablesLength.RemoveAt(variablesLength.IndexOf(Tuple.Create(variablesLength[i].Item1.Split('[').First() + "_001" + "[" + variablesLength[i].Item1.Split('[').Last(), variablesLength[i].Item2)));
                        }
                        else
                        {
                            variablesLength[i] = Tuple.Create(variablesLength[i].Item1.Replace("_002", ""), variablesLength[i].Item2);
                            variablesLength.RemoveAt(variablesLength.IndexOf(Tuple.Create(variablesLength[i].Item1 + "_001", variablesLength[i].Item2)));
                        }
                    }
                }
            }
            foreach (var i in allVariables)
            {
                int index = variablesLength.FindIndex(j => j.Item1 == i.ToString());
                if (index == -1)
                {
                    variablesLength.Add(Tuple.Create(i, 3));
                }
            }
            return variablesLength;
        }
        public object GenerateLinearEquationsSystem()
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_CharString).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.EqualsExpression)
                {
                    list.Add(i);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = EditEquation_Literals(list[i].ToString());
                list[i] = EditEquation_VariablesWithKnownLength(list[i].ToString());
                object obj = CheckEquationRemovability(list[i]);
                if (obj.GetType() == typeof(string))
                {
                    return "Unsatisfiable";
                }
                else if ((bool)obj == true)
                {
                    list.RemoveAt(i);
                    i = i - 1;
                }
            }
            List<string> duplicateVariables = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                foreach (var j in DetermineDuplicateVaribles(list[i]))
                {
                    list[i] = EditEquation_DuplicateVaribles(list[i].ToString(), j);
                    if (duplicateVariables.Contains(j) == false)
                    {
                        duplicateVariables.Add(j);
                    }
                }
            }
            string equations = null;
            foreach (var i in list)
            {
                equations = equations + "sympy.Eq(" + i.ToString().Replace("==", ",") + "), ";
            }
            foreach (var i in duplicateVariables)
            {
                if (i.Contains("[") == true)
                {
                    equations = equations + "sympy.Eq(" + i.Split('[').First() + "_001" + "[" + i.Split('[').Last() + ", " + i.Split('[').First() + "_002" + "[" + i.Split('[').Last() + "), ";
                }
                else
                {
                    equations = equations + "sympy.Eq(" + i + "_001, " + i + "_002), ";
                }
            }
            if (equations != null)
            {
                equations = equations.Remove(equations.Length - 2);
            }
            string variables = null;
            List<string> allVariables = new List<string>();
            foreach (var i in list)
            {
                foreach (var j in DetermineAllVaribles(i).ConvertAll(k => k.ToString()))
                {
                    if (allVariables.Contains(j) == false)
                    {
                        variables = variables + j + ", ";
                        allVariables.Add(j);
                    }
                }
            }
            if (variables != null)
            {
                variables = variables.Remove(variables.Length - 2);
            }
            return Tuple.Create(variables, equations);
        }
        public string SolveLinearEquationsSystem(string x, string y)
        {
            string variables = x.Replace(".", "").Replace("[", "").Replace("]", "");
            string equations = y.Replace(".", "").Replace("sympyEq", "sympy.Eq").Replace("[", "").Replace("]", "");
            if ((x.Split(',').Count() == 1) || (Regex.Matches(y, "sympy.Eq").Count == 1))
            {
                x = x + ", Variable_001, Variable_002";
                variables = variables + ", Variable_001, Variable_002";
                equations = equations + ", sympy.Eq(Variable_001, Variable_002)";
            }
            var pythonScriptPath = @"..\..\NewFolder\PythonScript.py";
            string pythonScript =
@"
import sys
sys.path.append(r'..\..\NewFolder\Python38\Lib')
sys.path.append(r'..\..\NewFolder\Lib\site-packages')

import sympy
" + variables + @" = sympy.symbols('" + x + @"')
print(sympy.linsolve((" + equations + @"), (" + variables + @")))
";
            File.WriteAllText(pythonScriptPath, pythonScript);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"..\..\NewFolder\Python38\python.exe",
                Arguments = $"\"{pythonScriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process process = Process.Start(processStartInfo);
            string result = process.StandardOutput.ReadToEnd();
            if (result == "")
            {
                return "Unsatisfiable";
            }
            return result;
        }
        public ExpressionSyntax EditEquation_Literals(string x)
        {
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf().Reverse())
            {
                if ((i.Kind() == SyntaxKind.CharacterLiteralExpression) || (i.Kind() == SyntaxKind.StringLiteralExpression))
                {
                    string result = (i.ToString().Length - 2).ToString();
                    var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                }
            }
            return root;
        }
        public ExpressionSyntax EditEquation_VariablesWithKnownLength(string x)
        {
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf().Reverse())
            {
                int index = variablesLength.FindIndex(j => j.Item1 == i.ToString());
                if (index != -1)
                {
                    string result = variablesLength[index].Item2.ToString();
                    var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                }
            }
            return root;
        }
        public object CheckEquationRemovability(SyntaxNode x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            list = DetermineAllVaribles(x);
            if (list.Count() == 0)
            {
                DataTable dataTable = new DataTable();
                string leftChild = x.ChildNodes().First().ToString();
                string rightChild = x.ChildNodes().Last().ToString();
                if ((int)dataTable.Compute(leftChild, "") == (int)dataTable.Compute(rightChild, ""))
                {
                    return true;
                }
                return "Unsatisfiable";
            }
            return false;
        }
        public ExpressionSyntax EditEquation_DuplicateVaribles(string x, string y)
        {
            var tree = SyntaxFactory.ParseExpression(x).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.ChildNodes().First().DescendantNodesAndSelf().Reverse())
            {
                if (i.ToString() == y)
                {
                    string result = null;
                    if (i.Kind() == SyntaxKind.ElementAccessExpression)
                    {
                        result = y.Split('[').First() + "_001" + "[" + y.Split('[').Last();
                    }
                    else if ((i.Kind() == SyntaxKind.SimpleMemberAccessExpression) || (i.Kind() == SyntaxKind.IdentifierName))
                    {
                        result = y + "_001";
                    }
                    var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                }
            }
            foreach (var i in root.ChildNodes().Last().DescendantNodesAndSelf())
            {
                if (i.ToString() == y)
                {
                    string result = null;
                    if (i.Kind() == SyntaxKind.ElementAccessExpression)
                    {
                        result = y.Split('[').First() + "_002" + "[" + y.Split('[').Last();
                    }
                    else if ((i.Kind() == SyntaxKind.SimpleMemberAccessExpression) || (i.Kind() == SyntaxKind.IdentifierName))
                    {
                        result = y + "_002";
                    }
                    var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                    var subRoot = (ExpressionSyntax)subTree.GetRoot();
                    root = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                }

            }
            return root;
        }
        public List<SyntaxNode> DetermineAllVaribles(SyntaxNode x)
        {
            List<SyntaxNode> list = new List<SyntaxNode>();
            var elementAccessExpressions = x.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions)
            {
                list.Add(i);
            }
            var memberAccessExpressions = x.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions)
            {
                SyntaxNode j = i;
                bool flag = true;
                do
                {
                    if (j.Kind() == SyntaxKind.ElementAccessExpression)
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
            var identifierNames = x.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames)
            {
                SyntaxNode j = i;
                bool flag = true;
                do
                {
                    if ((j.Kind() == SyntaxKind.ElementAccessExpression) || (j.Kind() == SyntaxKind.SimpleMemberAccessExpression))
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
            return list;
        }
        public List<string> DetermineDuplicateVaribles(SyntaxNode x)
        {
            List<string> leftVariables = DetermineAllVaribles(x.ChildNodes().First()).ConvertAll(i => i.ToString());
            leftVariables = leftVariables.Distinct().ToList();
            List<string> rightVariables = DetermineAllVaribles(x.ChildNodes().Last()).ConvertAll(i => i.ToString());
            rightVariables = rightVariables.Distinct().ToList();
            List<string> duplicateVaribles = new List<string>();
            foreach (var i in leftVariables)
            {
                foreach (var j in rightVariables)
                {
                    if (i == j)
                    {
                        duplicateVaribles.Add(i);
                    }
                }
            }
            return duplicateVaribles;
        }
        public object GenerateOutputs(List<Tuple<string, int>> x)
        {
            List<Tuple<List<string>, SyntaxKind, List<string>>> list = new List<Tuple<List<string>, SyntaxKind, List<string>>>();
            var tree = SyntaxFactory.ParseExpression(pathConstraint_CharString).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if ((i.Kind() == SyntaxKind.EqualsExpression) || (i.Kind() == SyntaxKind.NotEqualsExpression) || (i.Kind() == SyntaxKind.LessThanExpression) || (i.Kind() == SyntaxKind.LessThanOrEqualExpression) || (i.Kind() == SyntaxKind.GreaterThanExpression) || (i.Kind() == SyntaxKind.GreaterThanOrEqualExpression))
                {
                    List<SyntaxNode> left = new List<SyntaxNode>();
                    foreach (var j in i.ChildNodes().First().DescendantNodesAndSelf())
                    {
                        if (j.Kind() == SyntaxKind.ElementAccessExpression)
                        {
                            left.Add(j);
                        }
                        else if (j.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                        {
                            SyntaxNode k = j;
                            bool flag = true;
                            do
                            {
                                if (k.Kind() == SyntaxKind.ElementAccessExpression)
                                {
                                    flag = false;
                                    break;
                                }
                                k = k.Parent;
                            } while (k != null);
                            if (flag == true)
                            {
                                left.Add(j);
                            }
                        }
                        else if (j.Kind() == SyntaxKind.IdentifierName)
                        {
                            SyntaxNode k = j;
                            bool flag = true;
                            do
                            {
                                if ((k.Kind() == SyntaxKind.ElementAccessExpression) || (k.Kind() == SyntaxKind.SimpleMemberAccessExpression))
                                {
                                    flag = false;
                                    break;
                                }
                                k = k.Parent;
                            } while (k != null);
                            if (flag == true)
                            {
                                left.Add(j);
                            }
                        }
                        else if ((j.Kind() == SyntaxKind.CharacterLiteralExpression) || (j.Kind() == SyntaxKind.StringLiteralExpression))
                        {
                            left.Add(j);
                        }
                    }
                    List<SyntaxNode> right = new List<SyntaxNode>();
                    foreach (var j in i.ChildNodes().Last().DescendantNodesAndSelf())
                    {
                        if (j.Kind() == SyntaxKind.ElementAccessExpression)
                        {
                            right.Add(j);
                        }
                        else if (j.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                        {
                            SyntaxNode k = j;
                            bool flag = true;
                            do
                            {
                                if (k.Kind() == SyntaxKind.ElementAccessExpression)
                                {
                                    flag = false;
                                    break;
                                }
                                k = k.Parent;
                            } while (k != null);
                            if (flag == true)
                            {
                                right.Add(j);
                            }
                        }
                        else if (j.Kind() == SyntaxKind.IdentifierName)
                        {
                            SyntaxNode k = j;
                            bool flag = true;
                            do
                            {
                                if ((k.Kind() == SyntaxKind.ElementAccessExpression) || (k.Kind() == SyntaxKind.SimpleMemberAccessExpression))
                                {
                                    flag = false;
                                    break;
                                }
                                k = k.Parent;
                            } while (k != null);
                            if (flag == true)
                            {
                                right.Add(j);
                            }
                        }
                        else if ((j.Kind() == SyntaxKind.CharacterLiteralExpression) || (j.Kind() == SyntaxKind.StringLiteralExpression))
                        {
                            right.Add(j);
                        }
                    }
                    list.Add(Tuple.Create(GenerateList(left, x), i.Kind(), GenerateList(right, x)));
                }
            }
            List<Tuple<List<string>, SyntaxKind, List<string>>> newList = new List<Tuple<List<string>, SyntaxKind, List<string>>>(list.Count);
            list.ForEach((item) =>
            {
                newList.Add(Tuple.Create(new List<string>(item.Item1), item.Item2, new List<string>(item.Item3)));
            });
            object obj = SatisfyConstraints_FixedCharacters(newList);
            if (obj.GetType() == typeof(string))
            {
                return "Unsatisfiable";
            }
            newList = (List<Tuple<List<string>, SyntaxKind, List<string>>>)obj;
            SatisfyConstraints_Operators(newList);
            if (isSatisfiable == false)
            {
                return "Unsatisfiable";
            }
            newList = finalList;
            List<Tuple<string, string>> results = new List<Tuple<string, string>>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].Item1.Count; j++)
                {
                    if (list[i].Item1[j].StartsWith("$") == true)
                    {
                        if (results.Contains(Tuple.Create(list[i].Item1[j], newList[i].Item1[j])) == false)
                        {
                            results.Add(Tuple.Create(list[i].Item1[j], newList[i].Item1[j]));
                        }
                    }
                }
                for (int j = 0; j < list[i].Item3.Count; j++)
                {
                    if (list[i].Item3[j].StartsWith("$") == true)
                    {
                        if (results.Contains(Tuple.Create(list[i].Item3[j], newList[i].Item3[j])) == false)
                        {
                            results.Add(Tuple.Create(list[i].Item3[j], newList[i].Item3[j]));
                        }
                    }
                }
            }
            string result;
            for (int i = 0; i < results.Count; i++)
            {
                result = results[i].Item1;
                result = result.Remove(0, 1);
                result = result.Remove(result.Length - (result.Count() - result.LastIndexOf("_")));
                results[i] = Tuple.Create(result, results[i].Item2);
            }
            string output;
            int counter = 0;
            List<Tuple<string, string>> outputs = new List<Tuple<string, string>>();
            while (counter < results.Count)
            {
                output = results[counter].Item2;
                while ((counter + 1 < results.Count) && (results[counter + 1].Item1 == results[counter].Item1))
                {
                    output = output + results[counter + 1].Item2;
                    counter = counter + 1;
                }
                outputs.Add(Tuple.Create(results[counter].Item1, output));
                counter = counter + 1;
            }
            return outputs;
        }
        public List<string> GenerateList(List<SyntaxNode> x, List<Tuple<string, int>> y)
        {
            List<string> list = new List<string>();
            int variableLength = 0;
            foreach (var i in x)
            {
                if ((i.Kind() == SyntaxKind.ElementAccessExpression) || (i.Kind() == SyntaxKind.SimpleMemberAccessExpression) || (i.Kind() == SyntaxKind.IdentifierName))
                {
                    int index = y.FindIndex(j => j.Item1 == i.ToString());
                    variableLength = y[index].Item2;
                    for (int j = 1; j <= variableLength; j++)
                    {
                        list.Add("$" + i.ToString() + "_" + j);
                    }
                }
                else if ((i.Kind() == SyntaxKind.CharacterLiteralExpression) || (i.Kind() == SyntaxKind.StringLiteralExpression))
                {
                    variableLength = i.ToString().Length - 2;
                    for (int j = 1; j <= variableLength; j++)
                    {
                        list.Add(i.ToString()[j].ToString());
                    }
                }
            }
            return list;
        }
        public object SatisfyConstraints_FixedCharacters(List<Tuple<List<string>, SyntaxKind, List<string>>> x)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            do
            {
                list.Clear();
                foreach (var i in x)
                {
                    if (i.Item2 == SyntaxKind.EqualsExpression)
                    {
                        for (int j = 0; j < i.Item1.Count; j++)
                        {
                            if ((i.Item1[j].StartsWith("$") == false) && (i.Item3[j].StartsWith("$") == false))
                            {
                                if (i.Item1[j] != i.Item3[j])
                                {
                                    return "Unsatisfiable";
                                }
                            }
                            else if ((i.Item1[j].StartsWith("$") == true) && (i.Item3[j].StartsWith("$") == false))
                            {
                                int index = list.FindIndex(k => k.Item1 == i.Item1[j]);
                                if (index == -1)
                                {
                                    list.Add(Tuple.Create(i.Item1[j], i.Item3[j]));
                                }
                                else
                                {
                                    if (i.Item3[j] != list[index].Item2)
                                    {
                                        return "Unsatisfiable";
                                    }
                                }
                            }
                            else if ((i.Item1[j].StartsWith("$") == false) && (i.Item3[j].StartsWith("$") == true))
                            {
                                int index = list.FindIndex(k => k.Item1 == i.Item3[j]);
                                if (index == -1)
                                {
                                    list.Add(Tuple.Create(i.Item3[j], i.Item1[j]));
                                }
                                else
                                {
                                    if (i.Item1[j] != list[index].Item2)
                                    {
                                        return "Unsatisfiable";
                                    }
                                }
                            }
                        }
                    }
                }
                x = PropagateConstraints(x, list);
            } while (list.Count > 0);
            return x;
        }
        public void SatisfyConstraints_Operators(List<Tuple<List<string>, SyntaxKind, List<string>>> x)
        {
            counter = counter + 1;
            if (counter >= 20)
            {
                isSatisfiable = false;
                return;
            }
            finalList = new List<Tuple<List<string>, SyntaxKind, List<string>>>(x.Count);
            x.ForEach((item) =>
            {
                finalList.Add(Tuple.Create(new List<string>(item.Item1), item.Item2, new List<string>(item.Item3)));
            });
            Tuple<string, string> tuple;
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            foreach (var i in finalList)
            {
                if (i.Item2 == SyntaxKind.EqualsExpression)
                {
                    for (int j = 0; j < i.Item1.Count; j++)
                    {
                        if (i.Item1[j].StartsWith("$") == true)
                        {
                            int index1 = list.FindIndex(k => k.Item1 == i.Item1[j]);
                            int index2 = list.FindIndex(k => k.Item1 == i.Item3[j]);
                            if ((index1 == -1) && (index2 == -1))
                            {
                                tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                list.Add(Tuple.Create(i.Item3[j], tuple.Item1));
                            }
                            else if ((index1 != -1) && (index2 == -1))
                            {
                                list.Add(Tuple.Create(i.Item3[j], list[index1].Item2));
                            }
                            else if ((index1 == -1) && (index2 != -1))
                            {
                                list.Add(Tuple.Create(i.Item1[j], list[index2].Item2));
                            }
                            else
                            {
                                if (list[index1].Item2 != list[index2].Item2)
                                {
                                    finalList.Clear();
                                    SatisfyConstraints_Operators(x);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            finalList = PropagateConstraints(finalList, list);
            list.Clear();
            foreach (var i in finalList)
            {
                if ((i.Item2 == SyntaxKind.NotEqualsExpression) || (i.Item2 == SyntaxKind.LessThanExpression) || (i.Item2 == SyntaxKind.LessThanOrEqualExpression) || (i.Item2 == SyntaxKind.GreaterThanExpression) || (i.Item2 == SyntaxKind.GreaterThanOrEqualExpression))
                {
                    string status = "unknown";
                    int minLength = Math.Min(i.Item1.Count, i.Item3.Count);
                    for (int j = 0; j < minLength; j++)
                    {
                        if (status == "unknown")
                        {
                            if ((i.Item1[j].StartsWith("$") == false) && (i.Item3[j].StartsWith("$") == false))
                            {
                                status = CheckSatisfiability(i.Item1[j], i.Item2, i.Item3[j]);
                            }
                            else if ((i.Item1[j].StartsWith("$") == true) && (i.Item3[j].StartsWith("$") == false))
                            {
                                tuple = SelectAppropriateCharacters(i.Item1[j], i.Item2, i.Item3[j]);
                                list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                                status = "satisfied";
                            }
                            else if ((i.Item1[j].StartsWith("$") == false) && (i.Item3[j].StartsWith("$") == true))
                            {
                                tuple = SelectAppropriateCharacters(i.Item1[j], i.Item2, i.Item3[j]);
                                list.Add(Tuple.Create(i.Item3[j], tuple.Item2));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                                status = "satisfied";
                            }
                            else
                            {
                                if (i.Item1[j] == i.Item3[j])
                                {
                                    tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                    list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                    finalList = PropagateConstraints(finalList, list);
                                    list.Clear();
                                }
                                else
                                {
                                    tuple = SelectAppropriateCharacters(i.Item1[j], i.Item2, i.Item3[j]);
                                    list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                    list.Add(Tuple.Create(i.Item3[j], tuple.Item2));
                                    finalList = PropagateConstraints(finalList, list);
                                    list.Clear();
                                    status = "satisfied";
                                }
                            }
                        }
                        else if (status == "satisfied")
                        {
                            if (i.Item1[j].StartsWith("$") == true)
                            {
                                tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                            }
                            if (i.Item3[j].StartsWith("$") == true)
                            {
                                tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                list.Add(Tuple.Create(i.Item3[j], tuple.Item1));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                            }
                        }
                        else if (status == "unsatisfiable")
                        {
                            finalList.Clear();
                            SatisfyConstraints_Operators(x);
                            return;
                        }
                    }
                    if ((i.Item1.Count == i.Item3.Count) && (status == "unknown"))
                    {
                        if ((i.Item2 == SyntaxKind.NotEqualsExpression) || (i.Item2 == SyntaxKind.LessThanExpression) || (i.Item2 == SyntaxKind.GreaterThanExpression))
                        {
                            finalList.Clear();
                            SatisfyConstraints_Operators(x);
                            return;
                        }
                        status = "satisfied";
                    }
                    else if (i.Item1.Count > i.Item3.Count)
                    {
                        if ((status == "unknown") && ((i.Item2 == SyntaxKind.LessThanExpression) || (i.Item2 == SyntaxKind.LessThanOrEqualExpression)))
                        {
                            finalList.Clear();
                            SatisfyConstraints_Operators(x);
                            return;
                        }
                        for (int j = minLength; j < i.Item1.Count; j++)
                        {
                            if (i.Item1[j].StartsWith("$") == true)
                            {
                                tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                list.Add(Tuple.Create(i.Item1[j], tuple.Item1));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                                break;
                            }
                        }
                        if (status == "unknown")
                        {
                            status = "satisfied";
                        }
                    }
                    else if (i.Item1.Count < i.Item3.Count)
                    {
                        if ((status == "unknown") && ((i.Item2 == SyntaxKind.GreaterThanExpression) || (i.Item2 == SyntaxKind.GreaterThanOrEqualExpression)))
                        {
                            finalList.Clear();
                            SatisfyConstraints_Operators(x);
                            return;
                        }
                        for (int j = minLength; j < i.Item3.Count; j++)
                        {
                            if (i.Item3[j].StartsWith("$") == true)
                            {
                                tuple = Tuple.Create(((char)random.Next(97, 123)).ToString(), "");
                                list.Add(Tuple.Create(i.Item3[j], tuple.Item1));
                                finalList = PropagateConstraints(finalList, list);
                                list.Clear();
                                break;
                            }
                        }
                        if (status == "unknown")
                        {
                            status = "satisfied";
                        }
                    }
                }
            }
        }
        private string CheckSatisfiability(string x, SyntaxKind y, string z)
        {
            string status = "unknown";
            switch (y)
            {
                case SyntaxKind.NotEqualsExpression:
                    if (x[0] != z[0])
                    {
                        status = "satisfied";
                    }
                    break;
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                    if (x[0] < z[0])
                    {
                        status = "satisfied";
                    }
                    else if (x[0] > z[0])
                    {
                        status = "unsatisfiable";
                    }
                    break;
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    if (x[0] < z[0])
                    {
                        status = "unsatisfiable";
                    }
                    else if (x[0] > z[0])
                    {
                        status = "satisfied";
                    }
                    break;
                default:
                    break;
            }
            return status;
        }
        public Tuple<string, string> SelectAppropriateCharacters(string x, SyntaxKind y, string z)
        {
            Tuple<string, string> tuple = null;
            char left;
            char right;
            switch (y)
            {
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                    if ((x.StartsWith("$") == true) && (z.StartsWith("$") == true))
                    {
                        left = (char)random.Next(97, 97 + ((123 - 97) / 2));
                        right = (char)random.Next(97 + ((123 - 97) / 2), 123);
                        tuple = Tuple.Create(left.ToString(), right.ToString());
                    }
                    else if ((x.StartsWith("$") == true) && (z == ""))
                    {
                        left = (char)random.Next(97, 123);
                        tuple = Tuple.Create(left.ToString(), "");
                    }
                    else if ((x == "") && (z.StartsWith("$") == true))
                    {
                        right = (char)random.Next(97, 123);
                        tuple = Tuple.Create("", right.ToString());
                    }
                    else if (x.StartsWith("$") == true)
                    {
                        left = (char)random.Next(97, z[0]);
                        tuple = Tuple.Create(left.ToString(), z);
                    }
                    else if (z.StartsWith("$") == true)
                    {
                        right = (char)random.Next(x[0], 123);
                        tuple = Tuple.Create(x, right.ToString());
                    }
                    break;
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    if ((x.StartsWith("$") == true) && (z.StartsWith("$") == true))
                    {
                        left = (char)random.Next(97 + ((123 - 97) / 2), 123);
                        right = (char)random.Next(97, 97 + ((123 - 97) / 2));
                        tuple = Tuple.Create(left.ToString(), right.ToString());
                    }
                    else if ((x.StartsWith("$") == true) && (z == ""))
                    {
                        left = (char)random.Next(97, 123);
                        tuple = Tuple.Create(left.ToString(), "");
                    }
                    else if ((x == "") && (z.StartsWith("$") == true))
                    {
                        right = (char)random.Next(97, 123);
                        tuple = Tuple.Create("", right.ToString());
                    }
                    else if (x.StartsWith("$") == true)
                    {
                        left = (char)random.Next(z[0], 123);
                        tuple = Tuple.Create(left.ToString(), z);
                    }
                    else if (z.StartsWith("$") == true)
                    {
                        right = (char)random.Next(97, x[0]);
                        tuple = Tuple.Create(x, right.ToString());
                    }
                    break;
                default:
                    break;
            }
            return tuple;
        }
        public List<Tuple<List<string>, SyntaxKind, List<string>>> PropagateConstraints(List<Tuple<List<string>, SyntaxKind, List<string>>> x, List<Tuple<string, string>> y)
        {
            foreach (var i in y)
            {
                foreach (var j in x)
                {
                    for (int k = 0; k < j.Item1.Count; k++)
                    {
                        if (j.Item1[k] == i.Item1)
                        {
                            j.Item1[k] = i.Item2;
                        }
                    }
                    for (int k = 0; k < j.Item3.Count; k++)
                    {
                        if (j.Item3[k] == i.Item1)
                        {
                            j.Item3[k] = i.Item2;
                        }
                    }
                }
            }
            return x;
        }
    }
}