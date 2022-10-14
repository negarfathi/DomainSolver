using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainSolver
{
    class DomainCalculator_MethodArguments
    {
        public int counter;
        public bool isSatisfiable;
        private int methodCounter = 20;
        private string unknownArgument;
        private Random random = new Random();
        public List<Tuple<string, object>> partialFinalOutputs;
        private List<Tuple<SyntaxNode, object>> list = new List<Tuple<SyntaxNode, object>>();

        private string sourceCodePath;
        public DomainCalculator_MethodArguments(string sourceCodePath)
        {
            this.sourceCodePath = sourceCodePath;
        }
        public void CalculateMethodArgumentsDomain(List<Tuple<string, object>> x)
        {
            counter = counter + 1;
            if (counter >= 20)
            {
                isSatisfiable = false;
                return;
            }
            partialFinalOutputs = new List<Tuple<string, object>>(x.Count);
            x.ForEach((item) =>
            {
                partialFinalOutputs.Add(Tuple.Create(item.Item1, item.Item2));
            });
            for (int i = 0; i < partialFinalOutputs.Count; i++)
            {
                int index = Form1.methodCallsInformation.FindIndex(j => j.Item1 == partialFinalOutputs[i].Item1);
                if (index != -1)
                {
                    List<object> input = new List<object>();
                    List<List<object>> inputsList = new List<List<object>>();
                    object output;
                    switch (Form1.methodCallsInformation[index].Item5)
                    {
                        case "System.Boolean":
                            for (int j = 0; j < methodCounter; j++)
                            {
                                int number = 0;
                                do
                                {
                                    number = number + 1;
                                    if (number >= 20)
                                    {
                                        partialFinalOutputs.Clear();
                                        CalculateMethodArgumentsDomain(x);
                                        return;
                                    }
                                    input.Clear();
                                    foreach (var k in Form1.methodCallsInformation[index].Item6)
                                    {
                                        input.Add(GenerateMethodInput(k));
                                    }
                                    output = RunMethod(Form1.methodCallsInformation[index].Item2, Form1.methodCallsInformation[index].Item3, Form1.methodCallsInformation[index].Item4, input);
                                } while (output.ToString() != partialFinalOutputs[i].Item2.ToString());
                                inputsList.Add(new List<object>(input));
                            }
                            break;
                        case "System.Int32":
                        case "System.Single":
                        case "System.Double":
                            Tuple<string, string> myTuple = partialFinalOutputs[i].Item2 as Tuple<string, string>;
                            for (int j = 0; j < methodCounter; j++)
                            {
                                int number = 0;
                                do
                                {
                                    number = number + 1;
                                    if (number > 100)
                                    {
                                        partialFinalOutputs.Clear();
                                        CalculateMethodArgumentsDomain(x);
                                        return;
                                    }
                                    input.Clear();
                                    foreach (var k in Form1.methodCallsInformation[index].Item6)
                                    {
                                        input.Add(GenerateMethodInput(k));
                                    }
                                    output = RunMethod(Form1.methodCallsInformation[index].Item2, Form1.methodCallsInformation[index].Item3, Form1.methodCallsInformation[index].Item4, input);
                                } while (!((Convert.ToDouble(output) >= double.Parse(myTuple.Item1)) && (Convert.ToDouble(output) <= double.Parse(myTuple.Item2))));
                                inputsList.Add(new List<object>(input));
                            }
                            break;
                        default:
                            break;
                    }
                    for (int j = 0; j < methodCounter; j++)
                    {
                        for (int k = 0; k < inputsList[j].Count; k++)
                        {
                            DetermineUnknownArgumentsValue(Form1.methodCallsInformation[index].Item6[k].Item1, inputsList[j][k]);
                        }
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((list[j].Item1.Kind() == SyntaxKind.InvocationExpression) || (list[j].Item1.Kind() == SyntaxKind.ElementAccessExpression) || (list[j].Item1.Kind() == SyntaxKind.SimpleMemberAccessExpression) || (list[j].Item1.Kind() == SyntaxKind.IdentifierName))
                        {
                            partialFinalOutputs.Add(Tuple.Create(list[j].Item1.ToString(), list[j].Item2));
                        }
                        else
                        {
                            switch (list[j].Item2.GetType().FullName)
                            {
                                case "System.Boolean":
                                    if ((bool)list[j].Item2 == true)
                                    {
                                        unknownArgument = list[j].Item1.ToString();
                                    }
                                    else if ((bool)list[j].Item2 == false)
                                    {
                                        unknownArgument = "!(" + list[j].Item1.ToString() + ")";
                                    }
                                    EditUnknownArgument();
                                    List<List<Tuple<string, bool>>> outputs_Bool = new List<List<Tuple<string, bool>>>();
                                    Solver_Bool solver_Bool = new Solver_Bool(sourceCodePath, unknownArgument);
                                    solver_Bool.ProcessMethods();
                                    Tuple<string, List<List<Tuple<string, bool>>>> obj = solver_Bool.GenerateOutputs();
                                    if ((obj.Item1 == "2") || ((obj.Item1 == "3") && (obj.Item2.Count() == 0)))
                                    {
                                        partialFinalOutputs.Clear();
                                        CalculateMethodArgumentsDomain(x);
                                        return;
                                    }
                                    outputs_Bool = (List<List<Tuple<string, bool>>>)obj.Item2;
                                    int index1 = random.Next(0, outputs_Bool.Count);
                                    foreach (var k in outputs_Bool[index1])
                                    {
                                        partialFinalOutputs.Add(Tuple.Create(list[j].Item1.ToString(), (object)k.Item2));
                                    }
                                    break;
                                default:
                                    Solver_IntFloatDouble solver_IntFloatDouble = new Solver_IntFloatDouble(sourceCodePath, list[j].Item1.ToString());
                                    solver_IntFloatDouble.Update(list[j].Item1, (Tuple<string, string>)list[j].Item2);
                                    List<Tuple<string, Tuple<string, string>>> update = solver_IntFloatDouble.update;
                                    foreach (var k in update)
                                    {
                                        int index2 = partialFinalOutputs.FindIndex(l => l.Item1 == k.Item1);
                                        if (index2 == -1)
                                        {
                                            partialFinalOutputs.Add(Tuple.Create(k.Item1, (object)k.Item2));
                                        }
                                        else
                                        {
                                            Tuple<string, string> intersection = Intersection(k.Item2, (Tuple<string, string>)partialFinalOutputs[index2].Item2);
                                            if ((intersection.Item1 == "") && (intersection.Item2 == ""))
                                            {
                                                partialFinalOutputs.Clear();
                                                CalculateMethodArgumentsDomain(x);
                                                return;
                                            }
                                            partialFinalOutputs.Add(Tuple.Create(k.Item1, (object)intersection));
                                            partialFinalOutputs.RemoveAt(index2);
                                            if (index2 < i)
                                            {
                                                i = i - 1;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    list.Clear();
                }
            }
        }
        private object GenerateMethodInput(Tuple<SyntaxNode, Tuple<string, int>> x)
        {
            object methodInput = null;
            if ((x.Item1 != null) && (x.Item1.Kind() == SyntaxKind.TrueLiteralExpression))
            {
                methodInput = true;
            }
            else if ((x.Item1 != null) && (x.Item1.Kind() == SyntaxKind.FalseLiteralExpression))
            {
                methodInput = false;
            }
            else if ((x.Item1 != null) && ((x.Item1.Kind() == SyntaxKind.NumericLiteralExpression) || (x.Item1.Kind() == SyntaxKind.UnaryMinusExpression)))
            {
                switch (x.Item2.Item1)
                {
                    case "System.Int32":
                        methodInput = Convert.ToInt32(double.Parse(x.Item1.ToString()));
                        break;
                    case "System.Single":
                        methodInput = Convert.ToSingle(double.Parse(x.Item1.ToString()));
                        break;
                    case "System.Double":
                        methodInput = double.Parse(x.Item1.ToString());
                        break;
                    default:
                        break;
                }
            }
            else if ((x.Item1 != null) && (x.Item1.Kind() == SyntaxKind.CharacterLiteralExpression))
            {
                methodInput = char.Parse(x.Item1.ToString().Replace("'", ""));
            }
            else if ((x.Item1 != null) && (x.Item1.Kind() == SyntaxKind.StringLiteralExpression))
            {
                methodInput = x.Item1.ToString().Replace("\"", "");
            }
            else
            {
                int index = partialFinalOutputs.FindIndex(i => i.Item1 == x.Item1.ToString());
                if (index != -1)
                {
                    switch (partialFinalOutputs[index].Item2.GetType().FullName)
                    {
                        case "System.Boolean":
                            methodInput = partialFinalOutputs[index].Item2;
                            break;
                        case "System.String":
                            methodInput = partialFinalOutputs[index].Item2;
                            break;
                        default:
                            Tuple<string, string> myTuple = partialFinalOutputs[index].Item2 as Tuple<string, string>;
                            switch (x.Item2.Item1)
                            {
                                case "System.Int32":
                                    methodInput = random.Next(Convert.ToInt32(double.Parse(myTuple.Item1)), Convert.ToInt32(double.Parse(myTuple.Item2)));
                                    break;
                                case "System.Single":
                                    methodInput = (float)random.NextDouble() * (Convert.ToInt32(double.Parse(myTuple.Item2)) - Convert.ToInt32(double.Parse(myTuple.Item1))) + Convert.ToInt32(double.Parse(myTuple.Item1));
                                    break;
                                case "System.Double":
                                    methodInput = random.NextDouble() * (Convert.ToInt32(double.Parse(myTuple.Item2)) - Convert.ToInt32(double.Parse(myTuple.Item1))) + Convert.ToInt32(double.Parse(myTuple.Item1));
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    switch (x.Item2.Item1)
                    {
                        case "System.Boolean":
                            if (random.NextDouble() < 0.5)
                            {
                                methodInput = true;
                            }
                            else
                            {
                                methodInput = false;
                            }
                            break;
                        case "System.Int32":
                            methodInput = random.Next(-8, 8);
                            break;
                        case "System.Single":
                            methodInput = (float)random.NextDouble() * (8 - (-8)) + (-8);
                            break;
                        case "System.Double":
                            methodInput = random.NextDouble() * (8 - (-8)) + (-8);
                            break;
                        case "System.Char":
                            string characters1 = "abcdefghijklmnopqrstuvwxyz";
                            methodInput = characters1[random.Next(0, 26)];
                            break;
                        case "System.String":
                            string characters2 = "abcdefghijklmnopqrstuvwxyz";
                            char[] stringCharacters = new char[random.Next(0, 11)];
                            for (int i = 0; i < stringCharacters.Length; i++)
                            {
                                stringCharacters[i] = characters2[random.Next(0, 26)];
                            }
                            methodInput = new string(stringCharacters);
                            break;
                        case "System.Boolean[]":
                            int arrayLength_bool;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_bool = 5;
                            }
                            else
                            {
                                arrayLength_bool = x.Item2.Item2;
                            }
                            bool[] array_bool = new bool[arrayLength_bool];
                            for (int i = 0; i < arrayLength_bool; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_bool[i] = (bool)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.Boolean", 0)));
                            }
                            methodInput = array_bool;
                            break;
                        case "System.Int32[]":
                            int arrayLength_int;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_int = 5;
                            }
                            else
                            {
                                arrayLength_int = x.Item2.Item2;
                            }
                            int[] array_int = new int[arrayLength_int];
                            for (int i = 0; i < arrayLength_int; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_int[i] = (int)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.Int32", 0)));
                            }
                            methodInput = array_int;
                            break;
                        case "System.Single[]":
                            int arrayLength_float;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_float = 5;
                            }
                            else
                            {
                                arrayLength_float = x.Item2.Item2;
                            }
                            float[] array_float = new float[arrayLength_float];
                            for (int i = 0; i < arrayLength_float; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_float[i] = (float)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.Single", 0)));
                            }
                            methodInput = array_float;
                            break;
                        case "System.Double[]":
                            int arrayLength_double;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_double = 5;
                            }
                            else
                            {
                                arrayLength_double = x.Item2.Item2;
                            }
                            double[] array_double = new double[arrayLength_double];
                            for (int i = 0; i < arrayLength_double; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_double[i] = (double)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.Double", 0)));
                            }
                            methodInput = array_double;
                            break;
                        case "System.Char[]":
                            int arrayLength_char;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_char = 5;
                            }
                            else
                            {
                                arrayLength_char = x.Item2.Item2;
                            }
                            char[] array_char = new char[arrayLength_char];
                            for (int i = 0; i < arrayLength_char; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_char[i] = (char)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.Char", 0)));
                            }
                            methodInput = array_char;
                            break;
                        case "System.String[]":
                            int arrayLength_string;
                            if (x.Item2.Item2 == 0)
                            {
                                arrayLength_string = 5;
                            }
                            else
                            {
                                arrayLength_string = x.Item2.Item2;
                            }
                            string[] array_string = new string[arrayLength_string];
                            for (int i = 0; i < arrayLength_string; i++)
                            {
                                SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                array_string[i] = (string)GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create("System.String", 0)));
                            }
                            methodInput = array_string;
                            break;
                        default:
                            if (x.Item2.Item1.Contains("[]") == false)
                            {
                                string[] text = x.Item2.Item1.Split('.');
                                int counter = 1;
                                Type type = null;
                                Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text[0] + ".dll");
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
                                var instance = Activator.CreateInstance(type);
                                var fields = type.GetFields();
                                foreach (var j in fields)
                                {
                                    SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "." + j.Name).SyntaxTree.GetRoot();
                                    j.SetValue(instance, GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create(j.FieldType.FullName.Replace("+", "."), 0))));
                                }
                                methodInput = instance;
                            }
                            else
                            {
                                string[] text = x.Item2.Item1.Replace("[]", "").Split('.');
                                int counter = 1;
                                Type type = null;
                                Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + text[0] + ".dll");
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
                                int arrayLength_struct = x.Item2.Item2;
                                Array array_struct = Array.CreateInstance(type, arrayLength_struct);
                                for (int i = 0; i < arrayLength_struct; i++)
                                {
                                    SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x.Item1.ToString() + "[" + i + "]").SyntaxTree.GetRoot();
                                    array_struct.SetValue(GenerateMethodInput(Tuple.Create(syntaxNode, Tuple.Create(x.Item2.Item1.Replace("[]", ""), 0))), i);
                                }
                                methodInput = array_struct;
                            }
                            break;
                    }
                }
            }
            return methodInput;
        }
        private object RunMethod(string x, string y, string z, List<object> w)
        {
            object methodOutput = null;
            Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + x + ".dll");
            var types = assembly.GetTypes();
            foreach (var i in types)
            {
                if (i.Name == y)
                {
                    var methods = i.GetMethods();
                    foreach (var j in methods)
                    {
                        if (j.Name == z)
                        {
                            methodOutput = j.Invoke(Activator.CreateInstance(i), w.ToArray());
                        }
                    }
                }
            }
            return methodOutput;
        }
        private void DetermineUnknownArgumentsValue(SyntaxNode x, object y)
        {
            Type type = y.GetType();
            switch (type.FullName)
            {
                case "System.Boolean":
                    if ((x.Kind() != SyntaxKind.TrueLiteralExpression) && (x.Kind() != SyntaxKind.FalseLiteralExpression) && (partialFinalOutputs.Any(i => i.Item1 == x.ToString()) == false))
                    {
                        if (list.Contains(Tuple.Create(x, y)) == false)
                        {
                            list.Add(Tuple.Create(x, y));
                        }
                    }
                    break;
                case "System.Int32":
                case "System.Single":
                case "System.Double":
                    if ((x.Kind() != SyntaxKind.NumericLiteralExpression) && (x.Kind() != SyntaxKind.UnaryMinusExpression) && (partialFinalOutputs.Any(i => i.Item1 == x.ToString()) == false))
                    {
                        int index = list.FindIndex(i => i.Item1 == x);
                        if (index == -1)
                        {
                            list.Add(Tuple.Create(x, (object)Tuple.Create(y.ToString(), y.ToString())));
                        }
                        else
                        {
                            Tuple<string, string> myTuple = list[index].Item2 as Tuple<string, string>;
                            list.Add(Tuple.Create(x, (object)Tuple.Create(Math.Min(double.Parse(y.ToString()), double.Parse(myTuple.Item1)).ToString(), Math.Max(double.Parse(y.ToString()), double.Parse(myTuple.Item1)).ToString())));
                            list.RemoveAt(index);
                        }
                    }
                    break;
                case "System.Char":
                case "System.String":
                    if ((x.Kind() != SyntaxKind.CharacterLiteralExpression) && (x.Kind() != SyntaxKind.StringLiteralExpression) && (partialFinalOutputs.Any(i => i.Item1 == x.ToString()) == false))
                    {
                        if (list.Contains(Tuple.Create(x, y)) == false)
                        {
                            list.Add(Tuple.Create(x, y));
                        }
                    }
                    break;
                case "System.Boolean[]":
                case "System.Int32[]":
                case "System.Single[]":
                case "System.Double[]":
                case "System.Char[]":
                case "System.String[]":
                    Array myArray = y as Array;
                    foreach (var i in myArray)
                    {
                        SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x + "[" + i + "]").SyntaxTree.GetRoot();
                        DetermineUnknownArgumentsValue(syntaxNode, i);
                    }
                    break;
                default:
                    if (type.Name.Contains("[]") == false)
                    {
                        ValueType myStruct = y as ValueType;
                        var fields = myStruct.GetType().GetFields();
                        foreach (var i in fields)
                        {
                            SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x + "." + i.Name).SyntaxTree.GetRoot();
                            DetermineUnknownArgumentsValue(syntaxNode, i.GetValue(myStruct));
                        }
                    }
                    else
                    {
                        Array myArrayInStruct = y as Array;
                        for (int i = 0; i < myArrayInStruct.Length; i++)
                        {
                            SyntaxNode syntaxNode = SyntaxFactory.ParseExpression(x + "[" + i + "]").SyntaxTree.GetRoot();
                            DetermineUnknownArgumentsValue(syntaxNode, myArrayInStruct.GetValue(i));
                        }
                    }
                    break;
            }
        }
        private void EditUnknownArgument()
        {
            var tree = SyntaxFactory.ParseExpression(unknownArgument).SyntaxTree;
            var root = (ExpressionSyntax)tree.GetRoot();
            foreach (var i in root.DescendantNodesAndSelf())
            {
                if (i.Kind() == SyntaxKind.InvocationExpression)
                {
                    int index = partialFinalOutputs.FindIndex(j => j.Item1 == i.ToString());
                    if (index != -1)
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
                            string result = partialFinalOutputs[index].Item2.ToString();
                            var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                            var subRoot = (ExpressionSyntax)subTree.GetRoot();
                            var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                            unknownArgument = newRoot.ToString();
                            EditUnknownArgument();
                            break;
                        }
                    }
                }
                if (i.Kind() == SyntaxKind.ElementAccessExpression)
                {
                    int index = partialFinalOutputs.FindIndex(j => j.Item1 == i.ToString());
                    if (index != -1)
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
                            string result = partialFinalOutputs[index].Item2.ToString();
                            var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                            var subRoot = (ExpressionSyntax)subTree.GetRoot();
                            var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                            unknownArgument = newRoot.ToString();
                            EditUnknownArgument();
                            break;
                        }
                    }
                }
                else if (i.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    int index = partialFinalOutputs.FindIndex(j => j.Item1 == i.ToString());
                    if (index != -1)
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
                            string result = partialFinalOutputs[index].Item2.ToString();
                            var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                            var subRoot = (ExpressionSyntax)subTree.GetRoot();
                            var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                            unknownArgument = newRoot.ToString();
                            EditUnknownArgument();
                            break;
                        }
                    }
                }
                else if ((i.Kind() == SyntaxKind.IdentifierName) && (i.ToString().ToLower() != "true") && (i.ToString().ToLower() != "false"))
                {
                    int index = partialFinalOutputs.FindIndex(j => j.Item1 == i.ToString());
                    if (index != -1)
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
                            string result = partialFinalOutputs[index].Item2.ToString();
                            var subTree = SyntaxFactory.ParseExpression(result).SyntaxTree;
                            var subRoot = (ExpressionSyntax)subTree.GetRoot();
                            var newRoot = root.ReplaceNode(root.FindNode(i.Span), subRoot);
                            unknownArgument = newRoot.ToString();
                            EditUnknownArgument();
                            break;
                        }
                    }
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