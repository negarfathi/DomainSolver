using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class InformationExtractor
    {
        private string sourceCodePath;
        private string namespaceName;
        private string className;
        private string methodName;
        private string pathConstraint;
        public InformationExtractor(string sourceCodePath, string methodName, string pathConstraint)
        {
            this.sourceCodePath = sourceCodePath;
            namespaceName = sourceCodePath.Replace("\\" + sourceCodePath.Split('\\').Last(), "").Split('\\').Last();
            className = sourceCodePath.Split('\\').Last().Replace(".cs", "");
            this.methodName = methodName;
            this.pathConstraint = pathConstraint;
        }
        public List<Tuple<string, Tuple<string, int>>> ExtractVariablesInformation()
        {
            List<SyntaxNode> variables = new List<SyntaxNode>();
            var tree1 = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root1 = (ExpressionSyntax)tree1.GetRoot();
            var invocationExpressions = root1.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();
            foreach (var m in invocationExpressions)
            {
                var arguments = m.DescendantNodesAndSelf().OfType<ArgumentSyntax>();
                foreach (var n in arguments)
                {
                    var elementAccessExpressions1 = n.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
                    foreach (var i in elementAccessExpressions1)
                    {
                        if (variables.Any(j => j.ToString() == i.ToString()) == false)
                        {
                            variables.Add(i);
                        }
                    }
                    var memberAccessExpressions1 = n.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
                    foreach (var i in memberAccessExpressions1)
                    {
                        if (variables.Any(j => j.ToString() == i.ToString()) == false)
                        {
                            SyntaxNode k = i;
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
                                variables.Add(i);
                            }
                        }
                    }
                    var identifierNames1 = n.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
                    foreach (var i in identifierNames1)
                    {
                        if (variables.Any(j => j.ToString() == i.ToString()) == false)
                        {
                            SyntaxNode k = i;
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
                                variables.Add(i);
                            }
                        }
                    }
                }
            }
            var elementAccessExpressions2 = root1.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var i in elementAccessExpressions2)
            {
                if (variables.Any(j => j.ToString() == i.ToString()) == false)
                {
                    SyntaxNode k = i;
                    bool flag = true;
                    do
                    {
                        if (k.Kind() == SyntaxKind.InvocationExpression)
                        {
                            flag = false;
                            break;
                        }
                        k = k.Parent;
                    } while (k != null);
                    if (flag == true)
                    {
                        variables.Add(i);
                    }
                }
            }
            var memberAccessExpressions2 = root1.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var i in memberAccessExpressions2)
            {
                if (variables.Any(j => j.ToString() == i.ToString()) == false)
                {
                    SyntaxNode k = i;
                    bool flag = true;
                    do
                    {
                        if (k.Kind() == SyntaxKind.InvocationExpression || (k.Kind() == SyntaxKind.ElementAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        k = k.Parent;
                    } while (k != null);
                    if (flag == true)
                    {
                        variables.Add(i);
                    }
                }
            }
            var identifierNames2 = root1.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>();
            foreach (var i in identifierNames2)
            {
                if (variables.Any(j => j.ToString() == i.ToString()) == false)
                {
                    SyntaxNode k = i;
                    bool flag = true;
                    do
                    {
                        if ((k.Kind() == SyntaxKind.InvocationExpression) || (k.Kind() == SyntaxKind.ElementAccessExpression) || (k.Kind() == SyntaxKind.SimpleMemberAccessExpression))
                        {
                            flag = false;
                            break;
                        }
                        k = k.Parent;
                    } while (k != null);
                    if (flag == true)
                    {
                        variables.Add(i);
                    }
                }
            }
            List<Tuple<string, Tuple<string, int>>> variablesInformation = new List<Tuple<string, Tuple<string, int>>>();
            var tree2 = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceCodePath));
            var root2 = (CompilationUnitSyntax)tree2.GetRoot();
            foreach (var i in variables)
            {
                if (i.Kind() == SyntaxKind.ElementAccessExpression)
                {
                    SyntaxNode text = i.ChildNodes().First();
                    if (text.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                    {
                        string text1 = text.ChildNodes().Last().ToString();
                        string[] text2 = null;
                        var methodDeclarations = root2.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                        foreach (var j in methodDeclarations)
                        {
                            if (j.Identifier.ToString() == methodName)
                            {
                                var parameters = j.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                                foreach (var k in parameters)
                                {
                                    if (k.Identifier.ToString() == text.ChildNodes().First().ToString())
                                    {
                                        text2 = k.Type.ToString().Split('.');
                                        break;
                                    }
                                }
                                var variableDeclarations = j.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                                foreach (var k in variableDeclarations)
                                {
                                    if (k.ChildNodes().Last().ChildTokens().First().ToString() == text.ChildNodes().First().ToString())
                                    {
                                        text2 = k.Type.ToString().Split('.');
                                        break;
                                    }
                                }
                            }
                        }
                        int counter;
                        Assembly assembly;
                        if (File.Exists(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text2[0] + ".dll") == true)
                        {
                            counter = 1;
                            assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text2[0] + ".dll");
                        }
                        else
                        {
                            counter = 0;
                            assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
                        }
                        Type type = null;
                        var types = assembly.GetTypes();
                        foreach (var j in types)
                        {
                            if ((counter < text2.Count()) && (j.Name == text2[counter]))
                            {
                                types = j.GetNestedTypes();
                                counter = counter + 1;
                                type = j;
                            }
                        }
                        var fields = type.GetFields();
                        foreach (var j in fields)
                        {
                            if (j.Name == text1)
                            {
                                int arrayLength = 0;
                                variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(j.FieldType.FullName, arrayLength)));
                                break;
                            }
                        }
                    }
                    else if (text.Kind() == SyntaxKind.IdentifierName)
                    {
                        var methodDeclarations = root2.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                        foreach (var j in methodDeclarations)
                        {
                            if (j.Identifier.ToString() == methodName)
                            {
                                var parameters = j.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                                foreach (var k in parameters)
                                {
                                    if (k.Identifier.ToString() == text.ToString())
                                    {
                                        int arrayLength = 0;
                                        variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(k.Type.ToString().Remove(k.Type.ToString().Length - 2), arrayLength)));
                                        break;
                                    }
                                }
                                var variableDeclarations = j.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                                foreach (var k in variableDeclarations)
                                {
                                    if (k.ChildNodes().Last().ChildTokens().First().ToString() == text.ToString())
                                    {
                                        int arrayLength = 0;
                                        variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(k.Type.ToString().Remove(k.Type.ToString().Length - 2), arrayLength)));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (i.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    string text1 = i.ChildNodes().Last().ToString();
                    string[] text2 = null;
                    var methodDeclarations = root2.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                    foreach (var j in methodDeclarations)
                    {
                        if (j.Identifier.ToString() == methodName)
                        {
                            var parameters = j.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                            foreach (var k in parameters)
                            {
                                if (k.Identifier.ToString() == i.ChildNodes().First().ToString())
                                {
                                    text2 = k.Type.ToString().Split('.');
                                    break;
                                }
                            }
                            var variableDeclarations = j.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                            foreach (var k in variableDeclarations)
                            {
                                if (k.ChildNodes().Last().ChildTokens().First().ToString() == i.ChildNodes().First().ToString())
                                {
                                    text2 = k.Type.ToString().Split('.');
                                    break;
                                }
                            }
                        }
                    }
                    int counter;
                    Assembly assembly;
                    if (File.Exists(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text2[0] + ".dll") == true)
                    {
                        counter = 1;
                        assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text2[0] + ".dll");
                    }
                    else
                    {
                        counter = 0;
                        assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
                    }
                    Type type = null;
                    var types = assembly.GetTypes();
                    foreach (var j in types)
                    {
                        if ((counter < text2.Count()) && (j.Name == text2[counter]))
                        {
                            types = j.GetNestedTypes();
                            counter = counter + 1;
                            type = j;
                        }
                    }
                    var fields = type.GetFields();
                    foreach (var j in fields)
                    {
                        if (j.Name == text1)
                        {
                            int arrayLength = 0;
                            if (j.FieldType.FullName.Contains("[]") == true)
                            {
                                arrayLength = DetermineArrayLength(i);
                            }
                            variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(j.FieldType.FullName, arrayLength)));
                            break;
                        }
                    }
                }
                else if (i.Kind() == SyntaxKind.IdentifierName)
                {
                    var methodDeclarations = root2.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                    foreach (var j in methodDeclarations)
                    {
                        if (j.Identifier.ToString() == methodName)
                        {
                            var parameters = j.DescendantNodesAndSelf().OfType<ParameterSyntax>();
                            foreach (var k in parameters)
                            {
                                if (k.Identifier.ToString() == i.ToString())
                                {
                                    int arrayLength = 0;
                                    if (k.Type.ToString().Contains("[]") == true)
                                    {
                                        arrayLength = DetermineArrayLength(i);
                                    }
                                    variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(k.Type.ToString(), arrayLength)));
                                    break;
                                }
                            }
                            var variableDeclarations = j.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                            foreach (var k in variableDeclarations)
                            {
                                if (k.ChildNodes().Last().ChildTokens().First().ToString() == i.ToString())
                                {
                                    int arrayLength = 0;
                                    if (k.Type.ToString().Contains("[]") == true)
                                    {
                                        arrayLength = DetermineArrayLength(i);
                                    }
                                    variablesInformation.Add(Tuple.Create(i.ToString(), Tuple.Create(k.Type.ToString(), arrayLength)));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return variablesInformation;
        }
        public List<Tuple<string, string, string, string, string, List<Tuple<SyntaxNode, Tuple<string, int>>>>> ExtractMethodCallsInformation()
        {
            List<SyntaxNode> methodCalls = new List<SyntaxNode>();
            var tree1 = SyntaxFactory.ParseExpression(pathConstraint).SyntaxTree;
            var root1 = (ExpressionSyntax)tree1.GetRoot();
            var invocationExpressions = root1.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();
            foreach (var i in invocationExpressions)
            {
                if (methodCalls.Any(j => j.ToString() == i.ToString()) == false)
                {
                    methodCalls.Add(i);
                }
            }
            List<Tuple<string, string, string, string, string, List<Tuple<SyntaxNode, Tuple<string, int>>>>> methodCallsInformation = new List<Tuple<string, string, string, string, string, List<Tuple<SyntaxNode, Tuple<string, int>>>>>();
            var tree2 = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceCodePath));
            var root2 = (CompilationUnitSyntax)tree2.GetRoot();
            foreach (var i in methodCalls)
            {
                SyntaxNode text = i.ChildNodes().First();
                if (text.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    string text1 = text.ChildNodes().Last().ToString();
                    string[] text2 = null;
                    var methodDeclarations = root2.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();
                    foreach (var j in methodDeclarations)
                    {
                        if (j.Identifier.ToString() == methodName)
                        {
                            var variableDeclarations = j.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>();
                            foreach (var k in variableDeclarations)
                            {
                                if (k.ChildNodes().Last().ChildTokens().First().ToString() == text.ChildNodes().First().ToString())
                                {
                                    text2 = k.Type.ToString().Split('.');
                                    break;
                                }
                            }
                        }
                    }
                    if (text2.Count() == 2)
                    {
                        Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text2[0] + ".dll");
                        var types = assembly.GetTypes();
                        foreach (var j in types)
                        {
                            if (j.Name == text2[1])
                            {
                                var methods = j.GetMethods();
                                foreach (var k in methods)
                                {
                                    if (k.Name == text1)
                                    {
                                        List<Tuple<SyntaxNode, Tuple<string, int>>> list = new List<Tuple<SyntaxNode, Tuple<string, int>>>();
                                        List<SyntaxNode> arguments = new List<SyntaxNode>();
                                        foreach (var m in i.DescendantNodesAndSelf().OfType<ArgumentSyntax>())
                                        {
                                            SyntaxNode n = m;
                                            bool flag = true;
                                            do
                                            {
                                                if ((n.Kind() == SyntaxKind.InvocationExpression) || (n.Kind() == SyntaxKind.ElementAccessExpression))
                                                {
                                                    flag = false;
                                                    break;
                                                }
                                                n = n.Parent;
                                            } while (n != i);
                                            if (flag == true)
                                            {
                                                arguments.Add(m.Expression);
                                            }
                                        }
                                        var parameters = k.GetParameters();
                                        for (int l = 0; l < arguments.Count; l++)
                                        {
                                            int arrayLength = 0;
                                            if (parameters[l].ParameterType.IsArray == true)
                                            {
                                                arrayLength = DetermineArrayLength(arguments[l]);
                                            }
                                            list.Add(Tuple.Create(arguments[l], Tuple.Create(parameters[l].ParameterType.FullName.Replace("+", "."), arrayLength)));
                                        }
                                        methodCallsInformation.Add(Tuple.Create(i.ToString(), text2[0], text2[1], text1, k.ReturnType.FullName, list));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (text2.Count() == 1)
                    {
                        Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
                        var types = assembly.GetTypes();
                        foreach (var j in types)
                        {
                            if (j.Name == text2[0])
                            {
                                var methods = j.GetMethods();
                                foreach (var k in methods)
                                {
                                    if (k.Name == text1)
                                    {
                                        List<Tuple<SyntaxNode, Tuple<string, int>>> list = new List<Tuple<SyntaxNode, Tuple<string, int>>>();
                                        List<SyntaxNode> arguments = new List<SyntaxNode>();
                                        foreach (var m in i.DescendantNodesAndSelf().OfType<ArgumentSyntax>())
                                        {
                                            SyntaxNode n = m;
                                            bool flag = true;
                                            do
                                            {
                                                if ((n.Kind() == SyntaxKind.InvocationExpression) || (n.Kind() == SyntaxKind.ElementAccessExpression))
                                                {
                                                    flag = false;
                                                    break;
                                                }
                                                n = n.Parent;
                                            } while (n != i);
                                            if (flag == true)
                                            {
                                                arguments.Add(m.Expression);
                                            }
                                        }
                                        var parameters = k.GetParameters();
                                        for (int l = 0; l < arguments.Count; l++)
                                        {
                                            int arrayLength = 0;
                                            if (parameters[l].ParameterType.IsArray == true)
                                            {
                                                arrayLength = DetermineArrayLength(arguments[l]);
                                            }
                                            list.Add(Tuple.Create(arguments[l], Tuple.Create(parameters[l].ParameterType.FullName.Replace("+", "."), arrayLength)));
                                        }
                                        methodCallsInformation.Add(Tuple.Create(i.ToString(), namespaceName, text2[0], text1, k.ReturnType.FullName, list));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (text.Kind() == SyntaxKind.IdentifierName)
                {
                    Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
                    var types = assembly.GetTypes();
                    foreach (var j in types)
                    {
                        if (j.Name == className)
                        {
                            var methods = j.GetMethods();
                            foreach (var k in methods)
                            {
                                if (k.Name == text.ToString())
                                {
                                    List<Tuple<SyntaxNode, Tuple<string, int>>> list = new List<Tuple<SyntaxNode, Tuple<string, int>>>();
                                    List<SyntaxNode> arguments = new List<SyntaxNode>();
                                    foreach (var m in i.DescendantNodesAndSelf().OfType<ArgumentSyntax>())
                                    {
                                        SyntaxNode n = m;
                                        bool flag = true;
                                        do
                                        {
                                            if ((n.Kind() == SyntaxKind.InvocationExpression) || (n.Kind() == SyntaxKind.ElementAccessExpression))
                                            {
                                                flag = false;
                                                break;
                                            }
                                            n = n.Parent;
                                        } while (n != i);
                                        if (flag == true)
                                        {
                                            arguments.Add(m.Expression);
                                        }
                                    }
                                    var parameters = k.GetParameters();
                                    for (int l = 0; l < arguments.Count; l++)
                                    {
                                        int arrayLength = 0;
                                        if (parameters[l].ParameterType.IsArray == true)
                                        {
                                            arrayLength = DetermineArrayLength(arguments[l]);
                                        }
                                        list.Add(Tuple.Create(arguments[l], Tuple.Create(parameters[l].ParameterType.FullName.Replace("+", "."), arrayLength)));
                                    }
                                    methodCallsInformation.Add(Tuple.Create(i.ToString(), namespaceName, className, text.ToString(), k.ReturnType.FullName, list));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return methodCallsInformation;
        }
        private int DetermineArrayLength(SyntaxNode x)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceCodePath));
            var root = (CompilationUnitSyntax)tree.GetRoot();
            if (x.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                return 5;
            }
            else if (x.Kind() == SyntaxKind.IdentifierName)
            {
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
    }
}