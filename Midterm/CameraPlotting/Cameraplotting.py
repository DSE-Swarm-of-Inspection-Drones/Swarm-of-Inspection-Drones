import matplotlib.pyplot as plt
import numpy as np
import math

distances = np.array([0.1, 0.2, 0.3, 0.5, 1, 2, 4, 6, 10])
Cam1 = np.array([0.06052, 0.1172, 0.1739, 0.2872, 0.5705, 1.1372,2.2705, 3.4039, 5.6705])

Cam2 = np.array([0.1, 0.2, 0.3, 0.5, 0.9, 1.9, 3.7, 5.6, 9.3])
Cam2telephotox = np.array([0.5, 1, 2, 4, 6, 10])
Cam2telephoto = np.array([0.2, 0.4, 0.8, 1.6, 2.4, 4.1])
Cam2superwide = np.array([0.3, 0.7, 1.0, 1.6, 3.2, 6.3, 12.6, 18.9, 31.5])
#numpy array for the x axis of 0.1 to 100 in steps of 0.001
x = np.arange(distances[0], distances[-1], 0.001)
x_tel = np.arange(Cam2telephotox[0], Cam2telephotox[-1], 0.001)

#numpy linear interpolation on Cam1resolutions
Cam1y = np.interp(x, distances, Cam1)
Cam2y = np.interp(x, distances, Cam2)
Cam2telephoto_y = np.interp(x_tel, Cam2telephotox, Cam2telephoto)
Cam2superwide_y = np.interp(x, distances, Cam2superwide)

plt.loglog(x, Cam1y, label='Camera 1', color = 'red')
plt.loglog(x, Cam2y, label='Camera 2 standard lens', color = 'blue')
plt.loglog(x_tel, Cam2telephoto_y, label='Camera 2 telephoto lens', color = 'blue')
plt.loglog(x, Cam2superwide_y, label='Camera 2 super wide lens', color = 'blue')

plt.title('Thermographic camera')
plt.legend()
plt.grid()
plt.xlabel('Distance from aircraft [m]')
plt.ylabel('IFOV [mm]')
#pt.yscale('log')
plt.show()


        
