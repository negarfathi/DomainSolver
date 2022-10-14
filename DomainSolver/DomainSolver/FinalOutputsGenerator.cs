using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DomainSolver
{
    class FinalOutputsGenerator
    {
        private string sourceCodePath;
        public FinalOutputsGenerator(string sourceCodePath)
        {
            this.sourceCodePath = sourceCodePath;
        }
        public List<List<Tuple<string, object>>> GenerateFinalOutputs_VariablesAndMethods(List<List<Tuple<string, bool>>> x, List<List<Tuple<string, Tuple<string, string>>>> y, List<List<Tuple<string, string>>> z)
        {
            List<List<Tuple<string, object>>> finalOutputs = new List<List<Tuple<string, object>>>();
            List<Tuple<string, object>> list = new List<Tuple<string, object>>();
            if ((x.Count() > 0) && (y.Count() > 0) && (z.Count() > 0))
            {
                for (int i = 0; i < x.Count; i++)
                {
                    for (int j = 0; j < y.Count; j++)
                    {
                        for (int k = 0; k < z.Count; k++)
                        {
                            foreach (var l in x[i])
                            {
                                list.Add(Tuple.Create(l.Item1, (object)l.Item2));
                            }
                            foreach (var l in y[j])
                            {
                                list.Add(Tuple.Create(l.Item1, (object)l.Item2));
                            }
                            foreach (var l in z[k])
                            {
                                list.Add(Tuple.Create(l.Item1, (object)l.Item2));
                            }
                            finalOutputs.Add(new List<Tuple<string, object>>(list));
                            list.Clear();
                        }
                    }
                }
            }
            else if ((x.Count() > 0) && (y.Count() > 0))
            {
                for (int i = 0; i < x.Count; i++)
                {
                    for (int j = 0; j < y.Count; j++)
                    {
                        foreach (var k in x[i])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        foreach (var k in y[j])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        finalOutputs.Add(new List<Tuple<string, object>>(list));
                        list.Clear();
                    }
                }
            }
            else if ((x.Count() > 0) && (z.Count() > 0))
            {
                for (int i = 0; i < x.Count; i++)
                {
                    for (int j = 0; j < z.Count; j++)
                    {
                        foreach (var k in x[i])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        foreach (var k in z[j])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        finalOutputs.Add(new List<Tuple<string, object>>(list));
                        list.Clear();
                    }
                }
            }
            else if ((y.Count() > 0) && (z.Count() > 0))
            {
                for (int i = 0; i < y.Count; i++)
                {
                    for (int j = 0; j < z.Count; j++)
                    {
                        foreach (var k in y[i])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        foreach (var k in z[j])
                        {
                            list.Add(Tuple.Create(k.Item1, (object)k.Item2));
                        }
                        finalOutputs.Add(new List<Tuple<string, object>>(list));
                        list.Clear();
                    }
                }
            }
            else if (x.Count() > 0)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    foreach (var j in x[i])
                    {
                        list.Add(Tuple.Create(j.Item1, (object)j.Item2));
                    }
                    finalOutputs.Add(new List<Tuple<string, object>>(list));
                    list.Clear();
                }
            }
            else if (y.Count() > 0)
            {
                for (int i = 0; i < y.Count; i++)
                {
                    foreach (var j in y[i])
                    {
                        list.Add(Tuple.Create(j.Item1, (object)j.Item2));
                    }
                    finalOutputs.Add(new List<Tuple<string, object>>(list));
                    list.Clear();
                }
            }
            else if (z.Count() > 0)
            {
                for (int i = 0; i < z.Count; i++)
                {
                    foreach (var j in z[i])
                    {
                        list.Add(Tuple.Create(j.Item1, (object)j.Item2));
                    }
                    finalOutputs.Add(new List<Tuple<string, object>>(list));
                    list.Clear();
                }
            }
            return finalOutputs;
        }
        public List<List<Tuple<string, object>>> GenerateFinalOutputs_MethodArguments(List<List<Tuple<string, object>>> x)
        {
            DomainCalculator_MethodArguments methodArgumentsDomainCalculator = new DomainCalculator_MethodArguments(sourceCodePath);
            for (int i = 0; i < x.Count; i++)
            {
                methodArgumentsDomainCalculator.counter = 0;
                methodArgumentsDomainCalculator.isSatisfiable = true;
                methodArgumentsDomainCalculator.CalculateMethodArgumentsDomain(x[i]);
                if (methodArgumentsDomainCalculator.isSatisfiable == true)
                {
                    x[i] = new List<Tuple<string, object>>(methodArgumentsDomainCalculator.partialFinalOutputs);
                    methodArgumentsDomainCalculator.partialFinalOutputs.Clear();
                }
                else
                {
                    x.RemoveAt(i);
                    i = i - 1;
                }
            }
            return x;
        }
    }
}