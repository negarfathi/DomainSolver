
import sys
sys.path.append(r'..\..\NewFolder\Python38\Lib')
sys.path.append(r'..\..\NewFolder\Lib\site-packages')

import sympy
T_8, PCTEMP_LHS_7, T_9 = sympy.symbols('T_8, PCTEMP_LHS_7, T_9')
print(sympy.linsolve((sympy.Eq(T_8,15+PCTEMP_LHS_7), sympy.Eq(T_9,T_8+1)), (T_8, PCTEMP_LHS_7, T_9)))
