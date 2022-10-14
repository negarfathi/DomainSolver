using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace DomainSolver
{
    class DomainCalculator_Method
    {
        private int methodCounter = 20;
        private Random random = new Random();

        private string sourceCodePath;
        private string namespaceName;
        private string className;
        private string methodName;
        private List<Tuple<SyntaxNode, Tuple<string, int>>> arguments = new List<Tuple<SyntaxNode, Tuple<string, int>>>();
        public DomainCalculator_Method(string sourceCodePath, string namespaceName, string className, string methodName, List<Tuple<SyntaxNode, Tuple<string, int>>> arguments)
        {
            this.sourceCodePath = sourceCodePath;
            this.namespaceName = namespaceName;
            this.className = className;
            this.methodName = methodName;
            this.arguments = arguments;
        }
        public Tuple<bool, bool> CalculateMethodDomain_Bool()
        {
            Tuple<bool, bool> result = null;
            List<object> input = new List<object>();
            List<object> outputsList = new List<object>();
            for (int i = 0; i < methodCounter; i++)
            {
                foreach (var j in arguments)
                {
                    input.Add(GenerateMethodInput(j));
                }
                outputsList.Add(RunMethod(input));
                input.Clear();
            }
            if ((outputsList.Contains(true) == true) && (outputsList.Contains(false) == true))
            {
                result = Tuple.Create(true, true);
            }
            else if (outputsList.Contains(true) == true)
            {
                result = Tuple.Create(true, false);
            }
            else if (outputsList.Contains(false) == true)
            {
                result = Tuple.Create(false, true);
            }
            return result;
        }
        public Tuple<string, string> CalculateMethodDomain_IntFloatDouble()
        {
            Tuple<string, string> result;
            List<object> input = new List<object>();
            List<object> outputsList = new List<object>();
            for (int i = 0; i < methodCounter; i++)
            {
                foreach (var j in arguments)
                {
                    input.Add(GenerateMethodInput(j));
                }
                outputsList.Add(RunMethod(input));
                input.Clear();
            }
            result = Tuple.Create(outputsList.Min().ToString(), outputsList.Max().ToString());
            return result;
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
                            array_bool[i] = (bool)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.Boolean", 0)));
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
                            array_int[i] = (int)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.Int32", 0)));
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
                            array_float[i] = (float)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.Single", 0)));
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
                            array_double[i] = (double)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.Double", 0)));
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
                            array_char[i] = (char)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.Char", 0)));
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
                            array_string[i] = (string)GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create("System.String", 0)));
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
                                j.SetValue(instance, GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create(j.FieldType.FullName.Replace("+", "."), 0))));
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
                                array_struct.SetValue(GenerateMethodInput(Tuple.Create((SyntaxNode)null, Tuple.Create(x.Item2.Item1.Replace("[]", ""), 0))), i);
                            }
                            methodInput = array_struct;
                        }
                        break;
                }
            }
            return methodInput;
        }
        private object RunMethod(List<object> x)
        {
            object methodOutput = null;
            Assembly assembly = Assembly.LoadFrom(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "bin\\Debug\\" + namespaceName + ".dll");
            var types = assembly.GetTypes();
            foreach (var i in types)
            {
                if (i.Name == className)
                {
                    var methods = i.GetMethods();
                    foreach (var j in methods)
                    {
                        if (j.Name == methodName)
                        {
                            methodOutput = j.Invoke(Activator.CreateInstance(i), x.ToArray());
                        }
                    }
                }
            }
            return methodOutput;
        }
    }
}