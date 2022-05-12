import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np
import math

distances = np.array([0.1, 10])
Cam1 = np.array([0.06052, 5.6705])
Cam1telephotox = np.array([1, 10])
Cam1telephoto = np.array([0.2691, 2.8191])
Cam1widex = np.array([0.25, 10])
Cam1wide = np.array([0.2992, 11.3492])

Cam2 = np.array([0.1, 9.3])
Cam2telephotox = np.array([0.5, 1, 2, 4, 6, 10])
Cam2telephoto = np.array([0.2, 0.4, 0.8, 1.6, 2.4, 4.1])
Cam2superwide = np.array([0.3, 31.5])
#numpy array for the x axis of 0.1 to 100 in steps of 0.001
x = np.arange(distances[0], distances[-1], 0.001)
telex2 = np.arange(Cam2telephotox[0], Cam2telephotox[-1], 0.001)
telex1 = np.arange(Cam1telephotox[0], Cam1telephotox[-1], 0.001)
widex1 = np.arange(Cam1widex[0], Cam1widex[-1], 0.001)

#numpy linear interpolation on Cam1resolutions
Cam1y = np.interp(x, distances, Cam1)
Cam2y = np.interp(x, distances, Cam2)
Cam2telephoto_y = np.interp(telex2, Cam2telephotox, Cam2telephoto)
Cam2superwide_y = np.interp(x, distances, Cam2superwide)
Cam1telephoto_y = np.interp(telex1, Cam1telephotox, Cam1telephoto)
Cam1wide_y = np.interp(widex1, Cam1widex, Cam1wide)

fig = plt.figure()
ax = fig.gca()
ax.loglog(x, Cam1y, label='VarioCAM 1 Standard lens', color = 'red')
ax.loglog(telex1, Cam1telephoto_y, label='VarioCAM Telephoto lens', color = 'red', linestyle='dashdot',)
ax.loglog(widex1, Cam1wide_y, label='VarioCAM Wide lens', color = 'red',  linestyle='dashed')

ax.loglog(x, Cam2y, label='thermoIMAGER Standard lens', color = 'blue')
ax.loglog(telex2, Cam2telephoto_y, label='thermoIMAGER Telephoto lens', color = 'blue',  linestyle='dashdot')
ax.loglog(x, Cam2superwide_y, label='thermoIMAGER Wide lens', color = 'blue',  linestyle='dashed')

ax.set_xticks([0.1, 0.2, 0.3, 0.5, 1, 2, 5, 10])
ax.get_xaxis().set_major_formatter(mpl.ticker.ScalarFormatter())
ax.set_yticks([0.1, 0.2, 0.3, 0.5, 1, 2, 5, 10, 20, 30])
ax.get_yaxis().set_major_formatter(mpl.ticker.ScalarFormatter())

plt.title('Thermographic Camera Resolutions')
plt.legend()
plt.grid()
plt.xlabel('Distance from aircraft [m]')
plt.ylabel('IFOV [mm]')
#pt.yscale('log')
plt.show()


        
"""
distances = np.array([0.1, 0.2, 0.3, 0.5, 1, 2, 4, 6, 10])
Cam1 = np.array([0.06052, 0.1172, 0.1739, 0.2872, 0.5705, 1.1372,2.2705, 3.4039, 5.6705])
Cam1telephotox = np.array([1, 10])
Cam1telephoto = np.array([0.2691, 2.8191])
Cam1widex = np.array([0.25, 10])
Cam1wide = np.array([0.2992, 11.3492])

Cam2 = np.array([0.1, 0.2, 0.3, 0.5, 0.9, 1.9, 3.7, 5.6, 9.3])
Cam2telephotox = np.array([0.5, 1, 2, 4, 6, 10])
Cam2telephoto = np.array([0.2, 0.4, 0.8, 1.6, 2.4, 4.1])
Cam2superwide = np.array([0.3, 0.7, 1.0, 1.6, 3.2, 6.3, 12.6, 18.9, 31.5])"""