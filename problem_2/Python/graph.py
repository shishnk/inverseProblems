import numpy as np
import matplotlib.pyplot as plt
from scipy import interpolate

rPoints = []
values = []

with open("function.txt") as f:
    for line in f:
        point, value = line.split("\t")
        rPoints.append(float(point))
        values.append(float(value))

function = interpolate.interp1d(rPoints, values, kind = 'linear')        

plt.title(r'График распределения потенциала при Z = 0')
plt.xlabel(r'$r$')
plt.ylabel(r'$V(r)$')
plt.grid()
plt.plot(rPoints, function(rPoints))
plt.show()

