using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DomainSolver
{
    public partial class Form1 : Form
    {
        private int number = 0;
        private string sourceCodePath;
        private List<string> pathConstraints = new List<string>();
        public static List<Tuple<string, int>> stringsLength = new List<Tuple<string, int>>();
        public static List<Tuple<string, Tuple<string, int>>> variablesInformation = new List<Tuple<string, Tuple<string, int>>>();
        public static List<Tuple<string, string, string, string, string, List<Tuple<SyntaxNode, Tuple<string, int>>>>> methodCallsInformation = new List<Tuple<string, string, string, string, string, List<Tuple<SyntaxNode, Tuple<string, int>>>>>();

        public Form1()
        {
            InitializeComponent();
        }
        private void buttonProjectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxProjectPath.Text = folderBrowserDialog.SelectedPath;
            }
        }
        private void buttonPathConstraint_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxPathConstraint.Text = openFileDialog.FileName;
            }
        }
        private void buttonSolve_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            string pathConstraint = File.ReadAllText(textBoxPathConstraint.Text);
            pathConstraint = string.Concat(pathConstraint.Where(i => char.IsWhiteSpace(i) == false));

            sourceCodePath = Directory.GetFiles(textBoxProjectPath.Text, textBoxClassName.Text + ".cs", SearchOption.AllDirectories)[0];

            File.WriteAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "");

            PathConstraintSimplifier pathConstraintSimplifier = new PathConstraintSimplifier(sourceCodePath, textBoxMethodName.Text, pathConstraint);
            pathConstraintSimplifier.PropagateNotOperators();
            pathConstraintSimplifier.CalculateComputableExpressions();
            pathConstraintSimplifier.AddArraysLengthConstraints();
            pathConstraintSimplifier.AddStructsConstraints();
            if (pathConstraintSimplifier.pathConstraint == null)
            {
                File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Path Constraint:" + "\n" + pathConstraint + "\n" + "\n" + "Results:" + "\n" + "Unsatisfiable" + "\n" + "\n" + "Execution Time: " + watch.ElapsedMilliseconds + " ms");
                MessageBox.Show("The solving process is finished." + "\n" + sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt");
                Close();
                return;
            }

            InformationExtractor informationExtractor = new InformationExtractor(sourceCodePath, textBoxMethodName.Text, pathConstraintSimplifier.pathConstraint);
            variablesInformation = informationExtractor.ExtractVariablesInformation();
            methodCallsInformation = informationExtractor.ExtractMethodCallsInformation();

            pathConstraints = pathConstraintSimplifier.RemoveOrOperators();

            SolvePathConstraint(number);

            File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Execution Time: " + watch.ElapsedMilliseconds + " ms");

            MessageBox.Show("The solving process is finished." + "\n" + sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt");
            Close();
            return;
        }
        public void SolvePathConstraint(int x)
        {
            if (x < pathConstraints.Count)
            {
                File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Path Constraint:" + "\n" + pathConstraints[x] + "\n" + "\n" + "Results:" + "\n");

                PathConstraintClassifier pathConstraintClassifier = new PathConstraintClassifier(pathConstraints[x]);

                pathConstraintClassifier.PathConstraint_Bool();
                string pathConstraint_Bool = pathConstraintClassifier.pathConstraint_Bool;

                pathConstraintClassifier.PathConstraint_IntFloatDouble();
                string pathConstraint_IntFloatDouble = pathConstraintClassifier.pathConstraint_IntFloatDouble;

                pathConstraintClassifier.PathConstraint_CharString();
                string pathConstraint_CharString = pathConstraintClassifier.pathConstraint_CharString;

                pathConstraintClassifier.PathConstraint_Array();
                string pathConstraint_Array = pathConstraintClassifier.pathConstraint_Array;

                List<List<Tuple<string, bool>>> outputs_Bool = new List<List<Tuple<string, bool>>>();
                if (pathConstraint_Bool != "")
                {
                    Solver_Bool solver_Bool = new Solver_Bool(sourceCodePath, pathConstraint_Bool);
                    solver_Bool.ProcessMethods();
                    Tuple<string, List<List<Tuple<string, bool>>>> obj = solver_Bool.GenerateOutputs();
                    if ((obj.Item1 == "2") || ((obj.Item1 == "3") && (obj.Item2.Count() == 0)))
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                    outputs_Bool = (List<List<Tuple<string, bool>>>)obj.Item2;
                }

                List<List<Tuple<string, Tuple<string, string>>>> outputs_IntFloatDouble = new List<List<Tuple<string, Tuple<string, string>>>>();
                if (pathConstraint_IntFloatDouble != "")
                {
                    Solver_IntFloatDouble solver_IntFloatDouble = new Solver_IntFloatDouble(sourceCodePath, pathConstraint_IntFloatDouble);
                    solver_IntFloatDouble.ProcessConstants();
                    solver_IntFloatDouble.ProcessVariables();
                    solver_IntFloatDouble.ProcessMethods();
                    object obj = solver_IntFloatDouble.GenerateOutputs();
                    if (obj.GetType() == typeof(string))
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                    outputs_IntFloatDouble = (List<List<Tuple<string, Tuple<string, string>>>>)obj;

                    Random random = new Random();
                    int index1 = random.Next(0, outputs_IntFloatDouble.Count);
                    foreach (var i in outputs_IntFloatDouble[index1])
                    {
                        int index2 = methodCallsInformation.FindIndex(j => (j.Item1 == i.Item1) && (j.Item4 == "Length"));
                        if (index2 != -1)
                        {
                            int length = random.Next(Convert.ToInt32(double.Parse(i.Item2.Item1)), Convert.ToInt32(double.Parse(i.Item2.Item2)));
                            stringsLength.Add(Tuple.Create(methodCallsInformation[index2].Item6[0].Item1.ToString(), length));
                        }
                    }
                    if (stringsLength.Count != 0)
                    {
                        List<Tuple<string, Tuple<string, string>>> tuple = outputs_IntFloatDouble[index1];
                        outputs_IntFloatDouble.Clear();
                        outputs_IntFloatDouble.Add(tuple);
                    }
                }

                List<List<Tuple<string, string>>> outputs_CharString = new List<List<Tuple<string, string>>>();
                if (pathConstraint_CharString != "")
                {
                    Solver_CharString solver_CharString = new Solver_CharString(pathConstraint_CharString);
                    object obj1 = solver_CharString.DetermineVariablesLength();
                    if (obj1.GetType() == typeof(string))
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                    List<Tuple<string, int>> variablesLength = (List<Tuple<string, int>>)obj1;
                    object obj2 = solver_CharString.GenerateOutputs(variablesLength);
                    if (obj2.GetType() == typeof(string))
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                    outputs_CharString.Add(new List<Tuple<string, string>>((List<Tuple<string, string>>)obj2));
                }

                List<List<Tuple<string, object>>> finalOutputs = new List<List<Tuple<string, object>>>();
                FinalOutputsGenerator finalOutputsGenerator = new FinalOutputsGenerator(sourceCodePath);
                finalOutputs = finalOutputsGenerator.GenerateFinalOutputs_VariablesAndMethods(outputs_Bool, outputs_IntFloatDouble, outputs_CharString);
                finalOutputs = finalOutputsGenerator.GenerateFinalOutputs_MethodArguments(finalOutputs);
                if (finalOutputs.Count() == 0)
                {
                    File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                    number = number + 1;
                    SolvePathConstraint(number);
                    return;
                }
                
                if (pathConstraint_Array != "")
                {
                    Solver_Bool solver_Bool = new Solver_Bool(sourceCodePath, pathConstraint_Array);
                    Tuple<string, List<List<Tuple<string, bool>>>> obj = solver_Bool.GenerateOutputs();
                    if ((obj.Item1 == "2") || ((obj.Item1 == "3") && (obj.Item2.Count() == 0)))
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                    Solver_Array solver_Array = new Solver_Array(pathConstraint_Array);
                    finalOutputs = solver_Array.GenerateOutputs(finalOutputs);
                    if (finalOutputs.Count() == 0)
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "Unsatisfiable" + "\n" + "\n");
                        number = number + 1;
                        SolvePathConstraint(number);
                        return;
                    }
                }
                
                foreach (var i in finalOutputs)
                {
                    foreach (var j in i)
                    {
                        File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "(" + j.Item1 + ", " + j.Item2.ToString() + ")" + "\n");
                    }
                    File.AppendAllText(sourceCodePath.Replace(sourceCodePath.Split('\\').Last(), "") + "output.txt", "\n");
                }

                number = number + 1;
                SolvePathConstraint(number);
            }
        }
    }
}